using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Shared;
using UnityEngine;

namespace GrandTheftAuto.New {
    public class Loader {

        public Loader(GtaVersion version) {
            var dir = (Directory)Application.streamingAssetsPath;

            new Thread(() => {
                try {
                    ExportImg(ImgFile.GetMainImg(), dir + "imgs");
                } catch(Exception e) {
                    Log.Exception(e);
                }
            }).Start();
        }

        private static void ExportImg(ImgFile img, Directory dst) {

            dst.EnsureExists();
            dst.ClearContents();

            using(Timing.Get("Exporting Img"))
            using(new MemoryCounter())
            using(var progress = new ProgressBar("Exporting Img", 0, 32)) {

                img.Load();
                progress.Count = img.files.Length;

                using(Timing.Get("Writing Files"))
                ThreadUtility.Foreach(img.files, (entry) => {
                    try {
                        entry.CopyTo(dst + entry.virtualName);
                    } catch(Exception e) {
                        Log.Error("Failed to write: {0}", entry);
                        Log.Exception(e);
                    }
                });
            }
        }

    }
}