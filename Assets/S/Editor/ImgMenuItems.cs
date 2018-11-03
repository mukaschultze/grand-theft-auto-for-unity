using System;
using System.IO;
using System.Threading.Tasks;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Img;
using GrandTheftAuto.Shared;
using UnityEditor;
using UnityEngine;

namespace GrandTheftAuto.Editor {
    public class ImgMenuItems {

        private const int EXPORT_ORDER = 10000;
        private const string EXPORT_IMG = "Grand Theft Auto/Export/Img...";

        [MenuItem("LOADER/TESTE")]
        private static void Load() {
            new New.Loader(GtaVersion.SanAndreas);
        }

        // [MenuItem(EXPORT_IMG + "", false, EXPORT_ORDER)]
        // private static void ExportImg() {
        //     string src;
        //     string dst;

        //     if(SelectSourceFileAndSaveFolder(out src, out dst, "img"))
        //         ExportImgN(src, dst);
        // }

        private static void ExportImg(string imgPath, string saveDirectory) {
            if(string.IsNullOrEmpty(imgPath) || string.IsNullOrEmpty(saveDirectory))
                return;

            using(Timing.Get("Exporting Img"))
            using(new MemoryCounter())
            using(new AssetEditing())
            using(var progress = new ProgressBar("Exporting Img", 0, 32)) {
                var img = new New.ImgFile(imgPath);

                img.Load();
                progress.Count = img.files.Length;

                using(Timing.Get("Writing Files"))
                ThreadUtility.Foreach(img.files, (entry) => {
                    try {
                        entry.CopyTo(Path.Combine(saveDirectory, entry.virtualName));
                        //progress.Increment(entry.virtualName);

                        // if(progress.Canceled)
                        //     break;
                    } catch(Exception e) {
                        Log.Error("Failed to write: {0}", entry);
                        Log.Exception(e);
                    }
                });
            }
        }

    }
}