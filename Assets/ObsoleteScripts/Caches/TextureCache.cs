//using System;
//using System.Collections.Generic;
//using System.IO;
//using GrandTheftAuto.Data;
//using GrandTheftAuto.Diagnostics;
//using GrandTheftAuto.Img;
//using GrandTheftAuto.Txd;
//using UnityEngine;
//using Material = GrandTheftAuto.Dff.Material;
//using Object = UnityEngine.Object;
//using Texture = GrandTheftAuto.Txd.Texture;
//using UnityMaterial = UnityEngine.Material;

//namespace GrandTheftAuto.Caches {
//    public class TextureCache : Cache {

//        private readonly UnityMaterial baseMaterial;
//        private readonly Dictionary<Material, UnityMaterial> materials = new Dictionary<Material, UnityMaterial>();
//        private readonly Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
//        private readonly Dictionary<string, Texture> alphaTextures = new Dictionary<string, Texture>();

//        public TextureCache() : this(new Texture[0]) { }

//        public TextureCache(params Texture[] textures) {
//            AddTexture(textures);
//            baseMaterial = new UnityMaterial(Shader.Find("Standard"));
//        }

//        public void AddTexture(Texture texture) {
//            using(new Timing("Adding texture")) {
//                try {
//                    textures.Add(texture.FullName, texture);
//                }
//                catch {
//                    Log.Warning("The texture is already present in the cache: {0}", texture.FullName);
//                }
//                try {
//                    if(!string.IsNullOrEmpty(texture.AlphaName))
//                        alphaTextures.Add(texture.FullALphaName, texture);
//                }
//                catch {
//                    Log.Warning("The texture alpha is already present in the cache: {0}", texture.FullALphaName);
//                }
//            }
//        }

//        public void AddTexture(Texture[] textures) {
//            using(new Timing("Adding texture"))
//                for(int i = 0; i < textures.Length; i++)
//                    AddTexture(textures[i]);
//        }

//        public void AddTexture(List<Texture> textures) {
//            using(new Timing("Adding texture"))
//                for(int i = 0; i < textures.Count; i++)
//                    AddTexture(textures[i]);
//        }

//        public void AddTxd(TxdFile txd) {
//            using(new Timing("Adding Txd"))
//                AddTexture(txd.Textures);
//        }

//        public void AddTxd(TxdFile[] txds) {
//            using(new Timing("Adding Txd"))
//                for(int i = 0; i < txds.Length; i++)
//                    AddTxd(txds[i]);
//        }

//        public void AddTxd(List<TxdFile> txds) {
//            using(new Timing("Adding Txd"))
//                for(int i = 0; i < txds.Count; i++)
//                    AddTxd(txds[i]);
//        }

//        public Texture GetTexture(string name) {
//            using(new Timing("Retrieving Texture"))
//                try {
//                    return textures[name];
//                }
//                catch {
//                    Log.Warning("Could not find \"{0}\" in cache", name);
//                    return null;
//                }
//        }

//        public Texture GetAlphaTexture(string name) {
//            using(new Timing("Retrieving Texture"))
//                try {
//                    return alphaTextures[name];
//                }
//                catch {
//                    Log.Warning("Could not find \"{0}\" in cache", name);
//                    return null;
//                }
//        }

//        public Texture2D GetTexture2D(string name) {
//            using(new Timing("Retrieving Texture"))
//                try {
//                    return textures[name].Texture2D;
//                }
//                catch {
//                    Log.Warning("Could not find \"{0}\" in cache", name);
//                    return null;
//                }
//        }

//        public Texture2D GetAlphaTexture2D(string name) {
//            using(new Timing("Retrieving Texture"))
//                try {
//                    return alphaTextures[name].Texture2D;
//                }
//                catch {
//                    Log.Warning("Could not find \"{0}\" in cache", name);
//                    return null;
//                }
//        }

//        public UnityMaterial GetMaterial(Material source, string txdName) {
//            using(new Timing("Retrieving Material"))
//                try {
//                    return materials[source];
//                }
//                catch {
//                    return materials[source] = CreateMaterial(source, txdName);
//                }
//        }

//        private UnityMaterial CreateMaterial(Material source, string txdName) {
//            using(new Timing("Creating Material")) {
//                var material = Object.Instantiate(baseMaterial);

//                material.color = source.Color;
//                material.name = source.TextureName;
//                material.SetFloat("_Glossiness", 1f - source.Diffuse);
//                material.SetFloat("_Metallic", 1f - source.Specular);

//                if(source.Textured) {
//                    if(!string.IsNullOrEmpty(source.TextureName))
//                        material.SetTexture("_MainTex", GetTexture2D(txdName + "/" + source.TextureName));
//                    if(!string.IsNullOrEmpty(source.MaskName))
//                        material.SetTexture("_AlphaMask", GetAlphaTexture2D(txdName + "/" + source.MaskName));
//                }

//                return material;
//            }
//        }

//        public override void Load(BufferReader reader) {
//            using(new Timing("Reading Cache")) {
//                var textureCount = reader.ReadInt32();

//                for(int i = 0; i < textureCount; i++) {
//                    var name = reader.ReadString();
//                    var alphaName = reader.ReadString();
//                    var parentName = reader.ReadString();

//                    reader.PrewarmBuffer(16);

//                    var width = reader.ReadInt32();
//                    var height = reader.ReadInt32();
//                    var format = reader.ReadInt32();
//                    var rawData = reader.ReadBytes(reader.ReadInt32());

//                    var texture2D = new Texture2D(width, height, (TextureFormat)format, true);

//                    texture2D.name = name;
//                    texture2D.LoadRawTextureData(rawData);
//                    texture2D.Apply(true, true);

//                    AddTexture(new Texture(texture2D, name, alphaName, parentName));
//                }
//            }
//        }

//        public override void Save(BinaryWriter writer) {
//            using(new Timing("Writing Cache")) {
//                writer.Write(textures.Count);

//                foreach(var texture in textures.Values) {
//                    var texture2d = texture.Texture2D;
//                    var rawData = texture2d.GetRawTextureData();

//                    writer.Write(texture.Name);
//                    writer.Write(texture.AlphaName);
//                    writer.Write(texture.FullName.Replace(texture.Name, string.Empty));
//                    writer.Write(texture.Width);
//                    writer.Write(texture.Height);
//                    writer.Write((int)texture2d.format);
//                    writer.Write(rawData.Length);
//                    writer.Write(rawData);
//                }
//            }
//        }

//        public override void Create(GtaVersion version) {
//            using(new TempWorkingFolder(Directories.GetPathFromVersion(version)))
//            using(new Timing("Exporting texture cache"))
//            using(new GCPass())
//                try {
//                    var mainData = DataFile.GetMainData(version);
//                    var specificData = DataFile.GetVersionSpecificData(version);
//                    var imgs = new List<ImgFile> { ImgFile.GetMainImg(version) };

//                    imgs.AddRange(mainData.IMGs);
//                    imgs.AddRange(specificData.IMGs);

//                    foreach(var img in imgs)
//                        foreach(var entry in img.Entries)
//                            if(entry.FileName.EndsWith("txd", StringComparison.InvariantCultureIgnoreCase))
//                                AddTxd(new TxdFile(entry));

//                    AddTxd(mainData.TXDs);
//                    AddTxd(specificData.TXDs);
//                    Save(version);
//                }
//                catch(Exception e) {
//                    Log.Exception(e);
//                }
//        }

//        /* File format:
//        * 
//        * int32 textureCount
//        * array[textureCount] {
//        *   string name
//        *   string alphaName
//        *   string parentName (ends with '/')
//        *   int32 width
//        *   int32 height
//        *   int32 format
//        *   int32 dataLenght
//        *   byte[dataLenght] textureData
//        * }
//        */
//    }
//}
