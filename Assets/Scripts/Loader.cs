using System;
using System.Globalization;
using System.Linq;
using GrandTheftAuto.Data;
using GrandTheftAuto.Dff;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Ide;
using GrandTheftAuto.Img;
using GrandTheftAuto.Ipl;
using GrandTheftAuto.Shared;
using GrandTheftAuto.Txd;
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
            using(var progress = new ProgressBar("Loading " + Version.GetFormatedGTAName() + " map", 0, workingFolder, 32))
            try {
                DefaultData = DataFile.GetMainData(Version);
                SpecificData = DataFile.GetVersionSpecificData(Version);

                Gta3img = ImgFile.GetMainImg(Version);

                IdeCollection = new DefinitionCollection() { DefaultData, SpecificData };
                IplCollection = new PlacementCollection() { DefaultData, SpecificData, Gta3img };
                TxdCollection = new TextureCollection() { DefaultData, SpecificData, Gta3img };
                ModelCollection = new ModelCollection() { DefaultData, SpecificData, Gta3img };

                TxdCollection.AddTextureParent(DefaultData);
                TxdCollection.AddTextureParent(SpecificData);

                //ItemDefinition.TransformModifiers += SetStatic;
                progress.Count = IplCollection.AllPlacements.Count();

                foreach(var placement in IplCollection) {
                    Place(placement, progress);

                    if(progress.Canceled)
                        return;
                }

                if(!Camera.main.GetComponent<FreeCamera>())
                    Camera.main.gameObject.AddComponent<FreeCamera>();
                if(!Camera.main.GetComponent<CameraLod>())
                    Camera.main.gameObject.AddComponent<CameraLod>();
            }
            catch(Exception e) {
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

        public GameObject Place(ItemPlacement placement, ProgressBar progress) {
            using(new Timing("Placing"))
            try {
                progress.Increment(string.Format("(ID {1}) {0}", placement.ItemName, placement.DefinitionID));

                var definition = IdeCollection[placement.DefinitionID];
                var obj = definition.GetObject(false);

                obj.transform.position = placement.Position;
                obj.transform.rotation = placement.Rotation;
                //obj.transform.localScale = plac.Scale; Unnecessary?

                if(IplCollection.GetLodVersion(placement, out placement))
                    if(!obj.GetComponent<LODGroup>()) {
                        var lodObj = Place(placement, progress);
                        var lodGroupGO = new GameObject(obj.name + " (LOD Group)");
                        var lodGroup = lodGroupGO.AddComponent<LODGroup>();

                        obj.transform.SetParent(lodGroupGO.transform);
                        lodObj.transform.SetParent(lodGroupGO.transform);
                        lodGroupGO.layer = Layer.LODGroup;

                        foreach(var child in obj.GetComponentsInChildren<Transform>())
                            child.gameObject.layer = Layer.LODGroup;
                        foreach(var child in lodObj.GetComponentsInChildren<Transform>())
                            child.gameObject.layer = Layer.LODGroup;

                        lodGroup.SetLODs(new LOD[] {
                            new LOD(0.5f, obj.GetComponentsInChildren<Renderer>()),
                                new LOD(0f, lodObj.GetComponentsInChildren<Renderer>())
                        });
                    }

                return obj;
            }
            catch(Exception e) {
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