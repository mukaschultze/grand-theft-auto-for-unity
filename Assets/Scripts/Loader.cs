using System;
using System.Collections.Generic;
using System.Globalization;
using GrandTheftAuto.Data;
using GrandTheftAuto.Dff;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Ide;
using GrandTheftAuto.Img;
using GrandTheftAuto.Ipl;
using GrandTheftAuto.Shared;
using GrandTheftAuto.Txd;
using GrandTheftAuto.Water;
using UnityEngine;

namespace GrandTheftAuto {
    public class Loader : IDisposable {
        public string Path { get; private set; }
        public GtaVersion Version { get; private set; }

        public static Loader Current { get; private set; }
        public static ImgFile Gta3img { get; private set; }
        public static DataFile DefaultData { get; private set; }
        public static DataFile SpecificData { get; private set; }
        public static DefinitionCollection IdeCollection { get; private set; }
        public static PlacementCollection IplCollection { get; private set; }
        public static TextureCollection TxdCollection { get; private set; }
        public static ModelCollection ModelCollection { get; private set; }

        private readonly HashSet<string> objectsToDisable = new() {
            // doesn't match original map, now sure what these are
            "rockovergay",
            "dirtover",
            "MALLUNDER", // gta vc mall collision or something like that
            "mlmallroof01", // duplicated gta vc mall roof
        };

        private readonly HashSet<string> objectsToEnable = new() {
            "VegasSland40", // ground on vegas near highway, for some reason it has "shadow" flag
        };


        public Loader(GtaVersion version) {
            if(Current != null)
                throw new Exception("Another loader already in progress, make sure only one loader run at a time");

            Current = this;
            Path = Directories.GetPathFromVersion(Version = version);
        }

        public Loader(string path) {
            if(Current != null)
                throw new Exception("Another loader already in progress, make sure only one loader run at a time");

            Current = this;
            Version = Directories.GetVersionFromPath(Path = path);

            if(Version == GtaVersion.Unknown)
                throw new ArgumentException("Invalid Gta path: " + path);

            Log.Message("No version specified, loaded {0} for {1}", Version, path);
        }

        public Loader(DefinitionCollection itemDefinitions, ModelCollection modelCollection, TextureCollection textureCollection) {
            if(Current != null)
                throw new Exception("Another loader already in progress, make sure only one loader run at a time");

            Current = this;
            IdeCollection = itemDefinitions;
            ModelCollection = modelCollection;
            TxdCollection = textureCollection;
        }

        ~Loader() {
            if(Current == null)
                return;

            Dispose();
            Log.Warning("Loader not disposed, make sure to dispose it when the loading is over");
        }

        public void Load() {
            using(new Timing("Loading " + Version.GetFormatedGTAName(true)))
            using(new TempCultureInfo(CultureInfo.InvariantCulture))
            using(new MemoryCounter())
            using(var workingFolder = new TempWorkingFolder(Path))
            using(var progress = new ProgressBar("Loading " + Version.GetFormatedGTAName() + " map", 0, workingFolder, 1))
                try {
                    DefaultData = DataFile.GetMainData(Version);
                    SpecificData = DataFile.GetVersionSpecificData(Version);

                    Gta3img = ImgFile.GetMainImg(Version);

                    IdeCollection = new DefinitionCollection() { DefaultData, SpecificData };

                    IplCollection = new PlacementCollection();
                    IplCollection.Add(DefaultData);
                    IplCollection.Add(SpecificData);
                    IplCollection.Add(Gta3img);

                    TxdCollection = new TextureCollection() { DefaultData, SpecificData, Gta3img };
                    ModelCollection = new ModelCollection() { DefaultData, SpecificData, Gta3img };

                    IplCollection.ResolveNamedLOD();

                    TxdCollection.AddTextureParent(DefaultData);
                    TxdCollection.AddTextureParent(SpecificData);

                    ItemDefinition.TransformModifiers += SetStatic;

                    new WaterFile("data/water.dat", Version).CreateUnityWater();

                    var allPlacements = new Dictionary<string, ItemPlacement[]>();

                    foreach(var ipl in IplCollection.TextIPLs)
                        allPlacements.Add(ipl.Key, ipl.Value.Placements);

                    foreach(var ipl in IplCollection.StreamingIPLs)
                        allPlacements.Add(ipl.Key, ipl.Value.Placements);

                    progress.Count = allPlacements.Count;
                    progress.Current = 0;
                    foreach(var ipl in allPlacements) {
                        progress.Increment(ipl.Key);
                        var parent = new GameObject(ipl.Key);
                        foreach(var placement in ipl.Value) {
                            // progress.Increment(string.Format("(ID {1}) {0}", placement.ItemName, placement.DefinitionID));
                            if(!IplCollection.IsLOD(placement))
                                Place(placement, parent);

                            if(progress.Canceled)
                                return;
                        }
                    }

                    // if(!Camera.main.GetComponent<FreeCamera>())
                    //     Camera.main.gameObject.AddComponent<FreeCamera>();
                    if(!Camera.main.GetComponent<CameraLod>())
                        Camera.main.gameObject.AddComponent<CameraLod>();
                    if(!Camera.main.GetComponent<DynamicLod>())
                        Camera.main.gameObject.AddComponent<DynamicLod>();
                } catch(Exception e) {
                    Log.Error("FAILED TO LOAD");
                    Log.Exception(e);
                } finally {
                    ItemDefinition.TransformModifiers -= SetStatic;
#if UNITY_EDITOR
                    UnityEditor.Lightmapping.bakedGI = false;
                    UnityEditor.Lightmapping.realtimeGI = false;
                    UnityEditor.EditorApplication.Beep();
#endif
                }
        }

        public void SetStatic(Transform transform, Frame frame, ItemDefinition definition) {
            transform.gameObject.isStatic = true;
        }

        public GameObject Place(ItemPlacement placement, GameObject parent = null) {
            using(new Timing("Placing"))
                try {
                    var definition = IdeCollection[placement.DefinitionID];
                    var obj = definition.GetObject(placement.Position, placement.Rotation);
                    obj.transform.SetParent(parent?.transform, true);

                    if(IplCollection.GetLodVersion(placement, out placement))
                        if(!obj.GetComponent<LODGroup>()) {
                            var lodGroupGO = new GameObject(obj.name + " (LOD Group)");
                            var lodObj = Place(placement, lodGroupGO);
                            var lodGroup = lodGroupGO.AddComponent<LODGroup>();

                            lodGroupGO.transform.SetParent(parent?.transform, true);
                            obj.transform.SetParent(lodGroupGO.transform);
                            lodGroupGO.layer = Layer.LODGroup;

                            foreach(var child in obj.GetComponentsInChildren<Transform>())
                                child.gameObject.layer = Layer.LODGroup;
                            foreach(var child in lodObj.GetComponentsInChildren<Transform>())
                                child.gameObject.layer = Layer.LODGroup;

                            lodGroup.SetLODs(new LOD[] {
                                new (0.5f, obj.GetComponentsInChildren<Renderer>()),
                                new (0f, lodObj.GetComponentsInChildren<Renderer>())
                            });
                            lodGroup.animateCrossFading = true;
                            lodGroup.fadeMode = LODFadeMode.CrossFade;
                        }

                    if(objectsToDisable.Contains(obj.name))
                        obj.SetActive(false);

                    if(objectsToEnable.Contains(obj.name))
                        obj.SetActive(true);

                    return obj;
                } catch(Exception e) {
                    Log.Error("Failed to place object \"{0}\" (ID {1}): {2}", placement.ItemName, placement.DefinitionID, e);
                    return null;
                }
        }

        public void Dispose() {
            Current = null;
            Gta3img = null;
            DefaultData = null;
            SpecificData = null;
            IdeCollection = null;
            IplCollection = null;
            TxdCollection = null;
            ModelCollection = null;
        }
    }
}