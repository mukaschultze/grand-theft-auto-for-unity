using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GrandTheftAuto.Data;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Img;
using UnityEngine;

namespace GrandTheftAuto.Ipl {
    public class PlacementCollection {

        private const string STREAMING_IPL_NAME_FORMAT = "{0}_stream{1}.ipl";

        private readonly Dictionary<string, IplFile> textIPLs = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, BinaryIpl> streamingIPLs = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<ItemPlacement, ItemPlacement> lodLinks = new();
        private readonly HashSet<ItemPlacement> lodPlacements = new();

        // public ItemPlacement[] AllPlacements { get { return allPlacements.Values.ToArray(); } }
        public Dictionary<string, IplFile> TextIPLs { get { return textIPLs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); } }
        public Dictionary<string, BinaryIpl> StreamingIPLs { get { return streamingIPLs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); } }

        public void Add(DataFile data) {
            using(new Timing("Adding Placements (data)"))
                for(var i = 0; i < data.IPLs.Count; i++)
                    Add(data.IPLs[i]);
        }

        public void Add(IplFile ipl) {
            using(new Timing("Adding Placements (ipl)")) {
                textIPLs.Add(Path.GetFileNameWithoutExtension(ipl.FilePath), ipl);
            }
        }

        public void Add(ImgFile img) {
            if(img.Version < GtaVersion.SanAndreas) {
                Log.Message("Ignoring streaming placements because img version is below 2.0 ({0})", img.FilePath);
                return;
            }

            using(new Timing("Adding Placements (streaming)"))
                foreach(var kvp in textIPLs) {
                    var textIplName = kvp.Key;
                    var placements = new List<ItemPlacement>(kvp.Value.Placements);

                    try {
                        for(var i = 0; ;) {
                            var binaryIplName = string.Format(STREAMING_IPL_NAME_FORMAT, textIplName, i++);

                            if(!img.Contains(binaryIplName))
                                break;

                            var binaryIpl = new BinaryIpl(img[binaryIplName]);
                            streamingIPLs.Add(binaryIplName, binaryIpl);
                            placements.AddRange(binaryIpl.Placements);
                        }
                    } catch(Exception e) { Log.Exception(e); }

                    using(new Timing("Resolving LODs"))
                        for(var i = 0; i < placements.Count; i++) {
                            var placement = placements[i];

                            if(placement.LodDefinitionID < 0)
                                continue;

                            try {
                                var lodVersion = placements[placement.LodDefinitionID];
                                lodPlacements.Add(lodVersion);
                                lodLinks.Add(placement, placements[placement.LodDefinitionID] = lodVersion);
                            } catch(Exception e) { Log.Error("Failed to resolve LOD link, object {0}({1}) with lod {2}: {3}", placement.ItemName, placement.DefinitionID, placement.LodDefinitionID, e); }
                        }

                    // allPlacements.AddRange(placements);
                }
        }

        public void ResolveNamedLOD() {
            if(Loader.Current.Version == GtaVersion.SanAndreas)
                return;

            using(new Timing("Resolving LODs (Named)")) {
                var placements = textIPLs.Values.SelectMany(ipl => ipl.Placements);
                var lods = placements.Where(placement => placement.ItemName.StartsWith("LOD"));

                foreach(var lod in lods) {
                    try {
                        var nearest = placements.First(placement => Vector3.SqrMagnitude(placement.Position - lod.Position) < 0.01 && placement.ItemName.Substring(3) == lod.ItemName.Substring(3));
                        lodLinks.Add(nearest, lod);
                        lodPlacements.Add(lod);
                    } catch(Exception e) {
                        Log.Error("Failed to resolve LOD link, object {0}: {1}", lod.ItemName, e);
                    }
                }
            }
        }

        public bool IsLOD(ItemPlacement placement) {
            return lodPlacements.Contains(placement);
        }

        public bool GetLodVersion(ItemPlacement highResVersion, out ItemPlacement lowResVersion) {
            return lodLinks.TryGetValue(highResVersion, out lowResVersion);
        }
    }
}