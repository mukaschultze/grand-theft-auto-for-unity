using System;
using System.IO;
using GrandTheftAuto.Diagnostics;

namespace GrandTheftAuto {
    public abstract class TextFileParser {

        public string FilePath { get; protected set; }
        protected abstract char CommentChar { get; }
        protected abstract char EofChar { get; }

        protected void Load() {
            string line;

            using(var reader = new StreamReader(FilePath))
                while((line = reader.ReadLine()) != null) {
                    line = line.Trim();

                    if(string.IsNullOrEmpty(line) || line[0] == CommentChar || line[0] == EofChar)
                        continue;

                    try { ParseLine(line); }
                    catch(Exception e) { Log.Error("Error parsing line \"{0}\" in \"{1}\": {2}", line, FilePath, e); }
                }
        }

        protected abstract void ParseLine(string line);
    }
}