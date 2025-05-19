using System;
using System.Collections;
using System.Collections.Generic;
using GrandTheftAuto.Diagnostics;
using UnityEngine;

namespace GrandTheftAuto.Ipl {
    //TODO: Read other sections, if necessary
    public sealed class IplFile : TextFileParser, IEnumerable<ItemPlacement> {
        //TODO: Implement GTA SA IPL sections
        //http://gtaforums.com/topic/202532-sadoc-ipl-definitions/
        private enum IPLSection {
            Unknow,
            Inst,
            Cull,
            Pick,
            Zone,
            End,
        }

        public GtaVersion Version { get; private set; }
        public ItemPlacement this[int index] { get { return placements[index]; } }

        protected override char CommentChar { get { return '#'; } }
        protected override char EofChar { get { return '#'; } }

        private IPLSection currentSection = IPLSection.End;
        private List<ItemPlacement> placements = new List<ItemPlacement>();
        private readonly static Dictionary<GtaVersion, int> requiredTokens = new Dictionary<GtaVersion, int> {
            { GtaVersion.III, 12 },
            { GtaVersion.ViceCity, 13 },
            { GtaVersion.SanAndreas, 11 }
        };

        public IplFile(string path, GtaVersion version) {
            using(new Timing("Loading IPL")) {
                FilePath = path;
                Version = version;
                Load();
            }
        }

        protected override void ParseLine(string line) {
            ItemPlacement obj;

            switch(currentSection) {
                case IPLSection.End:
                    ProcessNewSectionStart(line);
                    break;

                default:
                    if(line.Equals("end", StringComparison.Ordinal))
                        currentSection = IPLSection.End;
                    else if(ProcessSectionItem(line, out obj))
                        placements.Add(obj);
                    break;
            }
        }

        private void ProcessNewSectionStart(string line) {
            try {
                currentSection = (IPLSection)Enum.Parse(typeof(IPLSection), line, true);
            } catch {
                Log.Warning("Unknow IPL section {0} in \"{1}\"", line.ToUpper(), FilePath);
                currentSection = IPLSection.Unknow;
            }
        }

        private bool ProcessSectionItem(string line, out ItemPlacement placement) {
            placement = new ItemPlacement();

            try {
                switch(currentSection) {
                    case IPLSection.Inst:
                        var toks = line.Split(new string[] { ", " }, StringSplitOptions.None);

                        if(toks.Length != requiredTokens[Version]) {
                            Log.Error("Incorrect number of tokens in {0} section, read {1} expected {2} in file \"{3}\" line \"{4}\"", currentSection, toks.Length, requiredTokens[Version], FilePath, line);
                            return false;
                        }

                        placement.DefinitionID = int.Parse(toks[0]);
                        placement.ItemName = toks[1];
                        placement.Scale = Vector3.one;
                        placement.LodDefinitionID = -1;

                        //Commented for performance reasons, it doesn't ever happen
                        //if(string.IsNullOrEmpty(placement.ItemName)) {
                        //    Log.Error("Object {0} doesn't have a name", placement.DefinitionID);
                        //    return false;
                        //}

                        switch(Version) {
                            case GtaVersion.III:
                                placement.Position = new Vector3(float.Parse(toks[2]), float.Parse(toks[4]), float.Parse(toks[3]));
                                placement.Scale = new Vector3(float.Parse(toks[5]), float.Parse(toks[6]), float.Parse(toks[7]));
                                placement.Rotation = new Quaternion(float.Parse(toks[8]), float.Parse(toks[10]), float.Parse(toks[9]), float.Parse(toks[11]));
                                break;

                            case GtaVersion.ViceCity:
                                placement.Position = new Vector3(float.Parse(toks[3]), float.Parse(toks[5]), float.Parse(toks[4]));
                                placement.Scale = new Vector3(float.Parse(toks[6]), float.Parse(toks[7]), float.Parse(toks[8]));
                                placement.Rotation = new Quaternion(float.Parse(toks[9]), float.Parse(toks[11]), float.Parse(toks[10]), float.Parse(toks[12]));
                                break;

                            case GtaVersion.SanAndreas:
                                placement.Position = new Vector3(float.Parse(toks[3]), float.Parse(toks[5]), float.Parse(toks[4]));
                                placement.Rotation = new Quaternion(float.Parse(toks[6]), float.Parse(toks[8]), float.Parse(toks[7]), float.Parse(toks[9]));
                                placement.LodDefinitionID = int.Parse(toks[10]);
                                break;
                        }
                        return true;

                    default:
                        return false;
                }

            } catch(Exception e) {
                Log.Error("Failed to parse IPL line at \"{0}\", line \"{1}\", section {2}: {3}", FilePath, line, currentSection, e);
                return false;
            }
        }

        public IEnumerator<ItemPlacement> GetEnumerator() {
            return ((IEnumerable<ItemPlacement>)placements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable<ItemPlacement>)placements).GetEnumerator();
        }
    }
}