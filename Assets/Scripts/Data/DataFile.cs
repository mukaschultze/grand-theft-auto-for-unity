using System;
using System.Collections.Generic;
using System.IO;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Ide;
using GrandTheftAuto.Img;
using GrandTheftAuto.Ipl;
using GrandTheftAuto.Txd;

namespace GrandTheftAuto.Data {
    public sealed class DataFile : TextFileParser {

        public const string DAT_MAIN = "data/default.dat";
        public const string DAT_III = "data/gta3.dat";
        public const string DAT_VC = "data/gta_vc.dat";
        public const string DAT_SA = "data/gta.dat";

        private enum Keyword {
            NONE = 0,
            /// <summary>
            /// Txd
            /// </summary>
            TEXDICTION = 1,
            /// <summary>
            /// Ide
            /// </summary>
            IDE = 2,
            /// <summary>
            /// Ipl
            /// </summary>
            IPL = 3,
            /// <summary>
            /// Img, San Andreas only
            /// </summary>
            IMG = 4,
            /// <summary>
            /// Splash
            /// </summary>
            SPLASH = 5,
            /// <summary>
            /// Collision
            /// </summary>
            COLFILE = 6,
            /// <summary>
            /// Mapzone, III only
            /// </summary>
            MAPZONE = 7,
            /// <summary>
            /// Dff
            /// </summary>
            MODELFILE = 8
        }

        public GtaVersion Version { get; private set; }
        public List<IdeFile> IDEs { get; private set; }
        public List<ImgFile> IMGs { get; private set; }
        public List<IplFile> IPLs { get; private set; }
        public List<TxdFile> TXDs { get; private set; }

        protected override char CommentChar { get { return '#'; } }
        protected override char EofChar { get { return '#'; } }

        public DataFile(string filePath, GtaVersion version) {
            using(new Timing("Loading DAT")) {
                if(version == GtaVersion.Unknown)
                    throw new ArgumentException("Unsupported GTA version");

                FilePath = filePath;
                Version = version;
                IDEs = new List<IdeFile>();
                IMGs = new List<ImgFile>();
                IPLs = new List<IplFile>();
                TXDs = new List<TxdFile>();
                Load();
            }
        }

        private Keyword ParseKeyword(ref string line) {
            try {
                var split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var keyword = (Keyword)Enum.Parse(typeof(Keyword), split[0], true);

                line = split[1];
                return keyword;
            }
            catch {
                Log.Error("Invalid line in dat file: {0}", line);
                return Keyword.NONE;
            }
        }

        protected override void ParseLine(string line) {
            var keyword = ParseKeyword(ref line);

            switch(keyword) {
                case Keyword.TEXDICTION:
                    TXDs.Add(new TxdFile(line.ToLower()));
                    break;

                case Keyword.IDE:
                    IDEs.Add(new IdeFile(line));
                    break;

                case Keyword.MAPZONE:
                case Keyword.IPL:
                    IPLs.Add(new IplFile(line, Version));
                    break;

                case Keyword.IMG:
                    IMGs.Add(new ImgFile(line, Version));
                    break;

                default:
                    Log.Message("Ignoring line with keyword {0}", keyword);
                    break;
            }
        }

        public static DataFile GetMainData(GtaVersion version) {
            var gtaPath = Directories.GetPathFromVersion(version);
            return new DataFile(Path.Combine(gtaPath, DAT_MAIN), version);
        }

        public static DataFile GetVersionSpecificData(GtaVersion version) {
            var gtaPath = Directories.GetPathFromVersion(version);

            switch(version) {
                case GtaVersion.III:
                    return new DataFile(Path.Combine(gtaPath, DAT_III), version);

                case GtaVersion.ViceCity:
                    return new DataFile(Path.Combine(gtaPath, DAT_VC), version);

                case GtaVersion.SanAndreas:
                    return new DataFile(Path.Combine(gtaPath, DAT_SA), version);

                default:
                    throw new ArgumentException("Unsopported GTA version");
            }
        }

    }
}