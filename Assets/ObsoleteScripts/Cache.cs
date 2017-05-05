//using System;
//using System.Collections.Generic;
//using System.IO;
//using GrandTheftAuto.Data;
//using GrandTheftAuto.Diagnostics;
//using GrandTheftAuto.Ide;
//using GrandTheftAuto.Img;
//using GrandTheftAuto.Ipl;
//using GrandTheftAuto.Txd;

//namespace GrandTheftAuto {
//    //TODO: Serialize
//    public class OldCache {

//        public Dictionary<string, BinaryIpl> IPLs { get; private set; }
//        public Dictionary<int, ItemDefinition> ItemsIDs { get; private set; }
//        public Dictionary<string, ItemDefinition> ItemsNames { get; private set; }
//        public Dictionary<ItemPlacement, ItemPlacement> LodPlacements { get; private set; }
//        public Dictionary<TxdFile, TxdFile> ParentsTxd { get; private set; }
//        public List<ItemPlacement> Placements { get; private set; }

//        public OldCache(ImgFile[] imgs, DataFile[] datas) {
//            using(new Timing("Initializing")) {
//                IPLs = new Dictionary<string, BinaryIpl>();
//                ItemsIDs = new Dictionary<int, ItemDefinition>();
//                ItemsNames = new Dictionary<string, ItemDefinition>();
//                Placements = new List<ItemPlacement>();
//                LodPlacements = new Dictionary<ItemPlacement, ItemPlacement>();
//                ParentsTxd = new Dictionary<TxdFile, TxdFile>();

//                foreach(var img in imgs)
//                    foreach(var proxy in img.Entries)
//                        if(proxy.FileName.EndsWith(".ipl"))
//                            IPLs.Add(proxy.FileName, new BinaryIpl(proxy));

//                foreach(var data in datas) {
//                    foreach(var ide in data.IDEs) {
//                        foreach(var definition in ide.Objects)
//                            try {
//                                ItemsIDs[definition.Key] = definition.Value;
//                                ItemsNames[definition.Value.DffName] = definition.Value;
//                            }
//                            catch {
//                                Log.Warning("Null definition");
//                            }
//                    }

//                    foreach(var ipl in data.IPLs) {
//                        var i = 0;
//                        var iplName = Path.GetFileNameWithoutExtension(ipl.FilePath).ToLower();
//                        var binaryIplName = string.Format("{0}_stream{1}.ipl", iplName, i);

//                        //while(IPLs.ContainsKey(binaryIplName)) {
//                        //    ipl.AddRange(IPLs[binaryIplName]);
//                        //    binaryIplName = string.Format("{0}_stream{1}.ipl", iplName, ++i);
//                        //}

//                        foreach(var placement in ipl) {
//                            if(placement.LodDefinitionID == -1)
//                                continue;

//                            try {
//                                LodPlacements.Add((ItemPlacement)placement, ipl[(int)placement.LodDefinitionID]);
//                            }
//                            catch(Exception e) { Log.Exception(e); }
//                        }

//                        Placements.AddRange(ipl);
//                    }

//                    foreach(var placement in LodPlacements.Values)
//                        Placements.Remove(placement);

//                    //foreach(var txd in data.TXDs)
//                    //    txdcache.AddTxd(txd);
//                }

//                //foreach(var data in datas)
//                //    foreach(var ide in data.IDEs)
//                //        foreach(var parenting in ide.TextureParents) {
//                //            var parent = TXDs[parenting.ParentName];
//                //            var child = TXDs[parenting.TextureName];

//                //            ParentsTxd.Add(parent, child);
//                //        }
//            }
//        }
//    }
//}