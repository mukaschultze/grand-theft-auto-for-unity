using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GrandTheftAuto.Data;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Img;
using UnityEngine;

namespace GrandTheftAuto.Ipl {
    public class PlacementCollection : IEnumerable<ItemPlacement> {

        private const string STREAMING_IPL_NAME_FORMAT = "{0}_stream{1}.ipl";

        private readonly Dictionary<string, IplFile> textIPLs = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<ItemPlacement> allPlacements = new();
        private readonly Dictionary<ItemPlacement, ItemPlacement> lodLinks = new();
        private readonly HashSet<ItemPlacement> lodPlacements = new();

        public ItemPlacement[] AllPlacements { get { return allPlacements.ToArray(); } }

        public void Add(DataFile data) {
            using(new Timing("Adding Placements (data)"))
                for(var i = 0; i < data.IPLs.Count; i++)
                    Add(data.IPLs[i]);
        }

        public void Add(IplFile ipl) {
            using(new Timing("Adding Placements (ipl)")) {
                textIPLs.Add(Path.GetFileNameWithoutExtension(ipl.FilePath), ipl);
                allPlacements.AddRange(ipl);
            }
        }

        public void Add(ImgFile img) {
            if(img.Version < GtaVersion.SanAndreas) {
                Log.Message("Ignoring streaming placements because img version is below 2.0 ({0})", img.FilePath);
                return;
            }

            // clear LOD loaded from other IPLs
            allPlacements.Clear();

            using(new Timing("Adding Placements (streaming)"))
                foreach(var kvp in textIPLs) {
                    var textIplName = kvp.Key;
                    var placements = new List<ItemPlacement>(kvp.Value);

                    try {
                        for(var i = 0; ;) {
                            var binaryIplName = string.Format(STREAMING_IPL_NAME_FORMAT, textIplName, i++);
                            placements.AddRange(new BinaryIpl(img[binaryIplName]));
                        }
                    } catch(KeyNotFoundException) { } catch(Exception e) { Log.Exception(e); }

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

                    allPlacements.AddRange(placements);
                }
        }

        public void ResolveNamedLOD() {
            if(Loader.Current.Version == GtaVersion.SanAndreas)
                return;

            using(new Timing("Resolving LODs (Named)")) {
                var lods = allPlacements.Where(placement => placement.ItemName.StartsWith("LOD"));

                foreach(var lod in lods) {
                    try {
                        var nearest = allPlacements.First(placement => Vector3.SqrMagnitude(placement.Position - lod.Position) < 0.01 && placement.ItemName.Substring(3) == lod.ItemName.Substring(3));
                        lodLinks.Add(nearest, lod);
                        lodPlacements.Add(lod);
                    } catch(Exception e) {
                        Log.Error("Failed to resolve LOD link, object {0}: {1}", lod.ItemName, e);
                    }
                }
            }
        }

        public bool GetLodVersion(ItemPlacement highResVersion, out ItemPlacement lowResVersion) {
            return lodLinks.TryGetValue(highResVersion, out lowResVersion);
        }

        public IEnumerator<ItemPlacement> GetEnumerator() {
            var nonLodPlacements = (from plac in allPlacements
                                    where !lodPlacements.Contains(plac)
                                    orderby plac.DefinitionID
                                    select plac).ToArray();

            return ((IEnumerable<ItemPlacement>)nonLodPlacements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            var nonLodPlacements = (from plac in allPlacements
                                    where !lodPlacements.Contains(plac)
                                    orderby plac.DefinitionID
                                    select plac).ToArray();

            return ((IEnumerable<ItemPlacement>)nonLodPlacements).GetEnumerator();
        }
    }
}