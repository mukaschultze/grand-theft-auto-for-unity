using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GrandTheftAuto.Data;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Img;

namespace GrandTheftAuto.Ipl {
    public class PlacementCollection : IEnumerable<ItemPlacement> {

        private const string STREAMING_IPL_NAME_FORMAT = "{0}_stream{1}.ipl";

        private Dictionary<string, IplFile> textIPLs = new Dictionary<string, IplFile>(StringComparer.OrdinalIgnoreCase);
        private List<ItemPlacement> allPlacements = new List<ItemPlacement>();
        private Dictionary<ItemPlacement, ItemPlacement> lodLinks = new Dictionary<ItemPlacement, ItemPlacement>();

        public ItemPlacement[] AllPlacements { get { return allPlacements.ToArray(); } }
        public ItemPlacement[] NonLodPlacements { get; private set; }

        public void Add(DataFile data) {
            using(Timing.Get("Adding Placements (data)"))
            for(var i = 0; i < data.IPLs.Count; i++)
                Add(data.IPLs[i]);
        }

        public void Add(IplFile ipl) {
            using(Timing.Get("Adding Placements (ipl)")) {
                textIPLs.Add(Path.GetFileNameWithoutExtension(ipl.FilePath), ipl);
                allPlacements.AddRange(ipl);
                NonLodPlacements = AllPlacements;
            }
        }

        public void Add(ImgFile img) {
            if(img.Version < GtaVersion.SanAndreas) {
                Log.Message("Ignoring streaming placements because img version is below 2.0 ({0})", img.FilePath);
                return;
            }

            allPlacements = new List<ItemPlacement>();

            using(Timing.Get("Adding Placements (streaming)"))
            foreach(var kvp in textIPLs) {
                var textIplName = kvp.Key;
                var placements = new List<ItemPlacement>(kvp.Value);

                try {
                    for(var i = 0;;) {
                        var binaryIplName = string.Format(STREAMING_IPL_NAME_FORMAT, textIplName, i++);
                        placements.AddRange(new BinaryIpl(img[binaryIplName]));
                    }
                } catch(KeyNotFoundException) { } catch(Exception e) { Log.Exception(e); }

                using(Timing.Get("Resolving LODs"))
                for(var i = 0; i < placements.Count; i++) {
                    var placement = placements[i];

                    if(placement.LodDefinitionID < 0)
                        continue;

                    try {
                        var lodVersion = placements[placement.LodDefinitionID];
                        lodVersion.IsLOD = true;
                        lodLinks.Add(placement, placements[placement.LodDefinitionID] = lodVersion);
                    } catch(Exception e) { Log.Error("Failed to resolve LOD link, object {0}({1}) with lod {2}: {3}", placement.ItemName, placement.DefinitionID, placement.LodDefinitionID, e); }
                }

                allPlacements.AddRange(placements);
            }

            NonLodPlacements = (from plac in allPlacements where!plac.IsLOD select plac).ToArray();
        }

        public bool GetLodVersion(ItemPlacement highResVersion, out ItemPlacement lowResVersion) {
            return lodLinks.TryGetValue(highResVersion, out lowResVersion);
        }

        public IEnumerator<ItemPlacement> GetEnumerator() {
            NonLodPlacements = (from plac in NonLodPlacements orderby plac.DefinitionID select plac).ToArray();

            return ((IEnumerable<ItemPlacement>)NonLodPlacements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            NonLodPlacements = (from plac in NonLodPlacements orderby plac.DefinitionID select plac).ToArray();

            return ((IEnumerable<ItemPlacement>)NonLodPlacements).GetEnumerator();
        }
    }
}