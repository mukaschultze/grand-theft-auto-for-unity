using System;
using System.Collections;
using System.Collections.Generic;
using GrandTheftAuto.Data;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Img;

namespace GrandTheftAuto.Dff {
    public class ModelCollection : IEnumerable<DffFile> {

        private Dictionary<string, DffFile> models = new Dictionary<string, DffFile>(StringComparer.OrdinalIgnoreCase);

        public DffFile this[string dffName] { get { return models[dffName]; } }

        public void Add(DataFile data) {
            using(Timing.Get("Adding Models (data)")) {
                //foreach(var dff in data.DFFs)
                //    Add(dff);
                foreach(var img in data.IMGs)
                    Add(img);
            }
        }

        public void Add(ImgFile img) {
            using(Timing.Get("Adding Models (img)"))
            foreach(var entry in img)
                if(entry.FileName.EndsWith(".dff", StringComparison.OrdinalIgnoreCase))
                    Add(new DffFile(entry));
        }

        public void Add(DffFile dff) {
            using(Timing.Get("Adding Models (dff)"))
            models.Add(dff.FileNameWithoutExtension, dff);
        }

        public bool TryGetValue(string dffName, out DffFile dff) {
            return models.TryGetValue(dffName, out dff);
        }

        IEnumerator<DffFile> IEnumerable<DffFile>.GetEnumerator() {
            return models.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return models.Values.GetEnumerator();
        }
    }
}