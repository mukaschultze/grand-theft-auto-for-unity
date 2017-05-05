//using System;
//using System.Collections.Generic;
//using System.IO;
//using GrandTheftAuto.Data;
//using GrandTheftAuto.Dff;
//using GrandTheftAuto.Diagnostics;
//using GrandTheftAuto.Ide;
//using GrandTheftAuto.Img;
//using GrandTheftAuto.Ipl;
//using GrandTheftAuto.Txd;
//using UnityEngine;
//using Material = GrandTheftAuto.Dff.Material;
//using Object = UnityEngine.Object;
//using Texture = GrandTheftAuto.Txd.Texture;
//using UnityMaterial = UnityEngine.Material;

//namespace GrandTheftAuto {
//    public class Cache {

//        private readonly bool useFullTextureName;
//        private readonly Dictionary<string, FileProxy> files;
//        private readonly Dictionary<string, Texture> txds;
//        private readonly Dictionary<Material, UnityMaterial> materials;
//        private readonly Dictionary<int, GameObject> gameObjects;
//        private readonly Dictionary<ItemPlacement, ItemPlacement> lodPlacements;

//        private readonly Dictionary<int, ItemDefinition> itemsIds;
//        private readonly Dictionary<string, ItemDefinition> itemsNames;

//        private readonly Texture2D defaultTexture;

//        public List<ItemPlacement> Placements { get; private set; }

//        public Cache(TxdFile[] txds, bool useFullTextureName = false) {
//            using(new Timing("Initializing")) {
//                this.useFullTextureName = useFullTextureName;
//                this.txds = new Dictionary<string, Texture>();
//                files = new Dictionary<string, FileProxy>();

//                foreach(var txd in txds)
//                    foreach(var texture in txd.Textures) {
//                        this.txds[useFullTextureName ? texture.FullName : texture.Name] = texture;
//                        this.txds[useFullTextureName ? texture.FullALphaName : texture.AlphaName] = texture;
//                    }

//                defaultTexture = Resources.Load<Texture2D>("Default");
//                materials = new Dictionary<Material, UnityMaterial>();
//            }
//        }

//        public Cache(ImgFile[] imgs, DataFile[] datas, bool useFullTextureName = false) {
//            using(new Timing("Initializing")) {
//                this.useFullTextureName = useFullTextureName;
//                files = new Dictionary<string, FileProxy>();
//                Placements = new List<ItemPlacement>();
//                lodPlacements = new Dictionary<ItemPlacement, ItemPlacement>();

//                foreach(var img in imgs)
//                    foreach(var proxy in img)
//                        if(proxy.Name.EndsWith(".dff") || proxy.Name.EndsWith(".txd") || proxy.Name.EndsWith(".ipl"))
//                            files[proxy.Name] = proxy;

//                itemsIds = new Dictionary<int, ItemDefinition>();
//                itemsNames = new Dictionary<string, ItemDefinition>();

//                foreach(var data in datas)
//                    foreach(var ide in data.IDEs)
//                        foreach(var definition in ide.Objects) {
//                            itemsIds[definition.ID] = definition;
//                            itemsNames[definition.DffName] = definition;
//                        }

//                gameObjects = new Dictionary<int, GameObject>(itemsIds.Count);

//                foreach(var data in datas)
//                    foreach(var ipl in data.IPLs) {

//                        for(int i = 0; i < 20; i++) {
//                            var binaryIplName = string.Format("{0}_stream{1}.ipl", Path.GetFileNameWithoutExtension(ipl.FilePath).ToLower(), i);

//                            if(!files.ContainsKey(binaryIplName))
//                                continue;

//                            ipl.Placements.AddRange(new BinaryIpl(files[binaryIplName]).Placements);
//                        }

//                        for(int i = 0; i < ipl.Placements.Count; i++) {
//                            var placement = ipl.Placements[i];
//                            if(placement.LodID < 0)
//                                continue;
//                            lodPlacements.Add(placement, ipl.Placements[placement.LodID]);
//                        }

//                        Placements.AddRange(ipl.Placements);
//                    }

//                txds = new Dictionary<string, Texture>();

//                foreach(var data in datas)
//                    foreach(var txd in data.TXDs)
//                        foreach(var texture in txd.Textures) {
//                            txds[useFullTextureName ? texture.FullName : texture.Name] = texture;
//                            txds[useFullTextureName ? texture.FullALphaName : texture.AlphaName] = texture;
//                        }

//                defaultTexture = Resources.Load<Texture2D>("Default");
//                materials = new Dictionary<Material, UnityMaterial>();
//            }
//        }

//        #region GameObjects
//        public GameObject GetGameObject(int id) {
//            if(!itemsIds.ContainsKey(id)) {
//                Log.Error("Placment not found, ID: {0}", id);
//                return new GameObject(id + "(Error)");
//            }

//            return GetGameObject(itemsIds[id]);
//        }

//        public GameObject GetGameObject(string name) {
//            if(!itemsNames.ContainsKey(name)) {
//                Log.Error("Placment not found, Name: {0}", name);
//                return new GameObject(name + "(Error)");
//            }

//            return GetGameObject(itemsNames[name].ID);
//        }

//        public GameObject GetGameObject(ItemPlacement placement) {

//            oldDefinition definition;

//            if(itemsIds.ContainsKey(placement.Id))
//                definition = itemsIds[placement.Id];
//            else if(itemsNames.ContainsKey(placement.Name))
//                definition = itemsNames[placement.Name];
//            else {
//                Log.Error("Placement not found, ID: {0}, Name: {1}", placement.Id, placement.Name);
//                return new GameObject(placement.Name + "(Error)");
//            }

//            if(lodPlacements.ContainsValue(placement))
//                return null;

//            var result = GetGameObject(definition);

//            if(lodPlacements.ContainsKey(placement)) {
//                var lodPlac = lodPlacements[placement];
//                var lodObj = GetGameObject(lodPlac.Id);

//                foreach(var r in result.GetComponentsInChildren<Renderer>()) {
//                    var lod = r.GetComponent<DistanceLod>();
//                    if(!lod)
//                        lod = r.gameObject.AddComponent<DistanceLod>();
//                    lod.lods = lodObj.GetComponentsInChildren<Renderer>();
//                }

//                lodObj.transform.position = lodPlac.Position;
//                lodObj.transform.rotation = lodPlac.Rotation;
//                lodObj.transform.localScale = lodPlac.Scale;
//            }

//            result.transform.position = placement.Position;
//            result.transform.rotation = placement.Rotation;
//            result.transform.localScale = placement.Scale;

//            return result;
//        }

//        public GameObject GetGameObject(ItemDefinition definition) {
//            try {
//                GameObject result;
//                FileProxy file;

//                if(gameObjects.TryGetValue(definition.ID, out result)) {
//                    result = Object.Instantiate(result);
//                    result.name = result.name.Replace("(Clone)", "(From cache)");
//                }
//                else if(files.TryGetValue(definition.DffName + ".dff", out file)) {
//                    var dff = new DffFile(file);
//                    dff.Load();
//                    gameObjects[definition.ID] = result = dff.GetUnityObject(definition.TxdName, this, false);

//                    //Objects with "shad" or "shadow" in the name are shadows geometry 
//                    //But we are not in 2004, so we don't need them anymore
//                    if(definition.DffName.Contains("shad"))
//                        result.SetActive(false);

//                    foreach(var t in result.GetComponentsInChildren<Transform>())
//                        //Every object with a draw distance higher than 300 meters is a lod version
//                        if(definition.DrawDistance > 300f) {
//                            if(result.name.StartsWith("islandlod"))
//                                t.gameObject.layer = LayerMask.NameToLayer("IslandLOD");
//                            else
//                                t.gameObject.layer = LayerMask.NameToLayer("LOD");
//                        }
//                        else if((definition.Flags | DefinitionFlags.DrawDistanceOff) == definition.Flags)
//                            t.gameObject.layer = LayerMask.NameToLayer("CullOff");
//                        else if(definition.DrawDistance <= 50f)
//                            t.gameObject.layer = LayerMask.NameToLayer("LowDistance");
//                        else if(definition.DrawDistance <= 100f)
//                            t.gameObject.layer = LayerMask.NameToLayer("MediumDistance");
//                        else if(definition.DrawDistance <= 200f)
//                            t.gameObject.layer = LayerMask.NameToLayer("HighDistance");
//                        else
//                            t.gameObject.layer = LayerMask.NameToLayer("VeryHighDistance");
//                }
//                else {
//                    gameObjects[definition.ID] = result = new GameObject(definition.DffName + "(Not found)");
//                    Log.Error("Dff not found: {0}", definition.DffName);
//                }

//                return result;
//            }
//            catch(Exception e) {
//                Log.Exception(e);
//                return gameObjects[definition.ID] = new GameObject(definition.DffName + " (Error)");
//            }
//        }
//        #endregion

//        #region Textures
//        public Texture2D GetTexture(string name) {
//            if(string.IsNullOrEmpty(name))
//                return null;

//            try {
//                FileProxy file;
//                Texture texture;
//                Texture2D result;

//                var txdName = string.Empty;

//                if(name.Contains("/")) {
//                    txdName = name.Split('/')[0] + ".txd";

//                    if(!useFullTextureName)
//                        name = name.Split('/')[1];
//                }

//                if(txds.TryGetValue(name, out texture))
//                    result = texture.Texture2D;
//                else if(files.TryGetValue(txdName, out file)) {
//                    var txd = new TxdFile(file);

//                    foreach(var t in txd.Textures) {
//                        txds[useFullTextureName ? t.FullName : t.Name] = t;
//                        txds[useFullTextureName ? t.FullALphaName : t.AlphaName] = t;
//                    }

//                    try {
//                        result = txds[name].Texture2D;
//                    }
//                    catch {
//                        result = defaultTexture;
//                        Log.Warning("The texture \"{0}\" is not inside the file \"{1}\"", name, txdName);
//                    }
//                }
//                else {
//                    result = defaultTexture;
//                    Log.Warning("Txd \"{0}\" not found", txdName);
//                }

//                return result;
//            }
//            catch(Exception e) {
//                Log.Exception(e);
//                Log.Warning("Error loading texture, loaded {0}: {1}", defaultTexture.name, name);
//                return defaultTexture;
//            }

//        }
//        #endregion

//        #region Materials
//        public UnityMaterial GetMaterial(Material material, string txdName) {
//            try {
//                UnityMaterial result;

//                if(!materials.TryGetValue(material, out result))
//                    result = materials[material] = material.GetUnityMaterial(txdName, this);

//                return result;
//            }
//            catch(Exception e) {
//                Log.Exception(e);
//                return null;
//            }
//        }
//        #endregion
//    }
//}