//using System;
//using GrandTheftAuto.Data;
//using GrandTheftAuto.Diagnostics;
//using GrandTheftAuto.Img;
//using GrandTheftAuto.Water;

//namespace GrandTheftAuto.Caches { }

//namespace GrandTheftAuto {
//    public static class OldLoader {

//        public static void Load(GtaVersion version, int maxObjects = int.MaxValue) {
//            Load(Directories.GetPathFromVersion(version), version, maxObjects);
//        }

//        public static void Load(string path, int maxObjects = int.MaxValue) {
//            if(!path.EndsWith("/") && !path.EndsWith("\\"))
//                path += "/";

//            var version = Directories.GetVersionFromPath(path);
//            Log.Message("No version specified, loaded GTA {0} for {1}", version, path);
//            Load(path, version, maxObjects);
//        }

//        public static void Load(string path, GtaVersion version, int maxObjects = int.MaxValue) {
//            using(new Timing(path)) {
//#if UNITY_EDITOR
//                UnityEditor.Lightmapping.bakedGI = false;
//                UnityEditor.Lightmapping.realtimeGI = false;
//#endif

//                //var txdCache = new TextureCache();
//                //var frameCache = new FrameCache();

//                //txdCache.LoadOrCreate(version);
//                //frameCache.LoadOrCreate(version);

//                using(new TempWorkingFolder(path))
//                using(new GCPass())
//                    try {
//                        var imgs = new ImgFile[] { ImgFile.GetMainImg(version) };
//                        var datas = new DataFile[] { DataFile.GetMainData(version), DataFile.GetVersionSpecificData(version) };
//                        var cache = new OldCache(imgs, datas);
//                        var placementsCount = cache.Placements.Count;

//                        new WaterFile(Directories.WATER_DEFAULT, version).CreateUnityWater();

//                        for(var i = 0; i < placementsCount && i < maxObjects; i++)
//                            try {
//                                var plac = cache.Placements[i];
//                                var def = cache.ItemsIDs[plac.DefinitionID];
//                                //var obj = def.GetObject(frameCache, txdCache, false);

//                                //obj.transform.position = plac.Position;
//                                //obj.transform.rotation = plac.Rotation;
//                                //obj.transform.localScale = plac.Scale; Unnecessary?
//                            }
//                            catch {
//                                Log.Message("{0}/{1}", cache.Placements[i].DefinitionID, cache.Placements[i].ItemName);
//                            }
//                    }
//                    catch(Exception e) {
//                        Log.Exception(e);
//                    }
//            }
//        }
//    }
//}