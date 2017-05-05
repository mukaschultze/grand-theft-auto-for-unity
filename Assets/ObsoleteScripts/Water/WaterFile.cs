using System;
using System.Collections.Generic;
using System.IO;
using GrandTheftAuto.Diagnostics;
using UnityEngine;

namespace GrandTheftAuto.Water {
    public sealed class WaterFile : TextFileParser {

        public GtaVersion Version { get; private set; }
        public List<WaterPlane> Planes { get; private set; }

        protected override char CommentChar { get { return ';'; } }
        protected override char EofChar { get { return '*'; } }

        public WaterFile(string path) {
            using(new Timing("Loading Water")) {
                FilePath = path;
                Version = GetVersion(path);
                Log.Message("No version specified for {0}, loaded GTA {1}", path, Version);
                Planes = new List<WaterPlane>();
                Load();
            }
        }

        public WaterFile(string path, GtaVersion version) {
            using(new Timing("Loading Water")) {
                FilePath = path;
                Version = version;
                Planes = new List<WaterPlane>();
                Load();
            }
        }

        public static GtaVersion GetVersion(string path) {
            if(File.ReadAllLines(path)[0] == "processed")
                return GtaVersion.SanAndreas;
            return GtaVersion.ViceCity;
        }

        protected override void ParseLine(string line) {
            if(line == "processed")
                return;

            var toks = new string[0];

            switch(Version) {
                case GtaVersion.III:
                case GtaVersion.ViceCity:
                    toks = line.Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if(toks.Length != 5) {
                        Log.Error("Invalid number of tokens, expected 5, found {0}", toks.Length);
                        return;
                    }

                    var level = float.Parse(toks[0]);
                    var xMin = float.Parse(toks[1]);
                    var yMin = float.Parse(toks[2]);
                    var xMax = float.Parse(toks[3]);
                    var yMax = float.Parse(toks[4]);

                    Planes.Add(new WaterPlane(new Vector3(xMin, level, yMax), new Vector3(xMax, level, yMax), new Vector3(xMax, level, yMin), new Vector3(xMin, level, yMin)));

                    break;

                //http://gtaforums.com/topic/211733-sadoc-waterdat/
                case GtaVersion.SanAndreas:
                    toks = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if(toks.Length == 29)
                        Planes.Add(new WaterPlane(
                            new Vector3(float.Parse(toks[7 * 1 + 0]), float.Parse(toks[7 * 1 + 2]), float.Parse(toks[7 * 1 + 1])),
                            new Vector3(float.Parse(toks[7 * 0 + 0]), float.Parse(toks[7 * 0 + 2]), float.Parse(toks[7 * 0 + 1])),
                            new Vector3(float.Parse(toks[7 * 2 + 0]), float.Parse(toks[7 * 2 + 2]), float.Parse(toks[7 * 2 + 1])),
                            new Vector3(float.Parse(toks[7 * 3 + 0]), float.Parse(toks[7 * 3 + 2]), float.Parse(toks[7 * 3 + 1])),
                            int.Parse(toks[28])
                        ));
                    else if(toks.Length == 22)
                        Planes.Add(new WaterPlane(
                            new Vector3(float.Parse(toks[7 * 2 + 0]), float.Parse(toks[7 * 2 + 2]), float.Parse(toks[7 * 2 + 1])),
                            new Vector3(float.Parse(toks[7 * 0 + 0]), float.Parse(toks[7 * 0 + 2]), float.Parse(toks[7 * 0 + 1])),
                            new Vector3(float.Parse(toks[7 * 1 + 0]), float.Parse(toks[7 * 1 + 2]), float.Parse(toks[7 * 1 + 1])),
                            int.Parse(toks[21])
                        ));
                    else
                        Log.Error("Invalid number of tokens, expected 22 or 29, found {0}", toks.Length);
                    break;
            }
        }
    }
}