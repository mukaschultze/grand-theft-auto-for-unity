using System;
using System.IO;
using System.Text;
using GrandTheftAuto.Dff;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Ide;
using GrandTheftAuto.Img;
using GrandTheftAuto.Txd;
using UnityEditor;
using UnityEngine;
using Decoder = GrandTheftAuto.Txd.Decoding.Decoder;

namespace GrandTheftAuto.Editor {
    internal static class MenuItems {

        #region Testing
        private const int TEST_COUNT = 1;
        private const int TEST_ORDER = -10000;
        private const string TEST = "Grand Theft Auto/Performance Test";

        private static void Test1() {
            var go = Selection.activeGameObject;
            var light = go.GetComponent<Light>();

            light.colorTemperature = 5500f;
            UnityEngine.Rendering.GraphicsSettings.lightsUseColorTemperature = true;
            UnityEngine.Rendering.GraphicsSettings.lightsUseLinearIntensity = true;
        }

        private static void Test2() {

        }

        private static void Test3() {

        }

        private static void Test4() {

        }

        private static void Test5() {

        }

        [MenuItem(TEST, false, TEST_ORDER)]
        private static void Test() {
            new PerformanceTest(TEST_COUNT, Test1, Test2, Test3, Test4, Test5).Run();
        }
        #endregion

        #region Preferences
        private const int PREFERENCES_ORDER = 30000;
        private const string PREFERENCES_LIGHTS = "Grand Theft Auto/Night Vertex Lights";
        private const string PREFERENCES_DEBUG = "Grand Theft Auto/Layer Debug";
        private const string PREFERENCES_SPECULAR = "Grand Theft Auto/Specular Materials";
        private const string PREFERENCES_MIPMAPS = "Grand Theft Auto/MipMaps";
        private const string PREFERENCES_GPU_DECODING = "Grand Theft Auto/GPU Decoding";

        private const string KEYWORD_LIGHTS = "_NIGHT_ILLUMINATION";
        private const string KEYWORD_DEBUG = "_LAYER_DEBUG";
        private const string KEYWORD_SPECULAR = "_SPECULAR_ON";

        [MenuItem(PREFERENCES_LIGHTS, false, PREFERENCES_ORDER)]
        private static void VertexLights() {
            if(Shader.IsKeywordEnabled(KEYWORD_LIGHTS))
                Shader.DisableKeyword(KEYWORD_LIGHTS);
            else
                Shader.EnableKeyword(KEYWORD_LIGHTS);
        }

        [MenuItem(PREFERENCES_DEBUG, false, PREFERENCES_ORDER)]
        private static void LayerDebug() {
            if(Shader.IsKeywordEnabled(KEYWORD_DEBUG))
                Shader.DisableKeyword(KEYWORD_DEBUG);
            else
                Shader.EnableKeyword(KEYWORD_DEBUG);
        }

        [MenuItem(PREFERENCES_SPECULAR, false, PREFERENCES_ORDER)]
        private static void MaterialSpecular() {
            if(Shader.IsKeywordEnabled(KEYWORD_SPECULAR))
                Shader.DisableKeyword(KEYWORD_SPECULAR);
            else
                Shader.EnableKeyword(KEYWORD_SPECULAR);
        }

        [MenuItem(PREFERENCES_GPU_DECODING, false, PREFERENCES_ORDER)]
        private static void ChangeGPUDecoding() {
            Decoder.GPUDecodingPref.Value = !Decoder.GPUDecodingPref;
        }

        [MenuItem(PREFERENCES_MIPMAPS, false, PREFERENCES_ORDER)]
        private static void ChangeMipmaps() {
            Decoder.UseMipmaps.Value = !Decoder.UseMipmaps;
        }

        [MenuItem(PREFERENCES_GPU_DECODING, true)]
        private static bool AllowGPUDecoding() {
            Menu.SetChecked(PREFERENCES_GPU_DECODING, Decoder.GPUDecoding);
            return SystemInfo.supportsComputeShaders;
        }

        [MenuItem(PREFERENCES_LIGHTS, true)]
        [MenuItem(PREFERENCES_DEBUG, true)]
        [MenuItem(PREFERENCES_SPECULAR, true)]
        private static bool KeywordsCheck() {
            Menu.SetChecked(PREFERENCES_LIGHTS, Shader.IsKeywordEnabled(KEYWORD_LIGHTS));
            Menu.SetChecked(PREFERENCES_DEBUG, Shader.IsKeywordEnabled(KEYWORD_DEBUG));
            Menu.SetChecked(PREFERENCES_SPECULAR, Shader.IsKeywordEnabled(KEYWORD_SPECULAR));
            Menu.SetChecked(PREFERENCES_MIPMAPS, Decoder.UseMipmaps);
            return true;
        }
        #endregion

        #region Load GTA Map
        private const int LOAD_GTA_ORDER = -100;
        private const string LOAD_GTA_III = "Grand Theft Auto/Load GTA III Map";
        private const string LOAD_GTA_VC = "Grand Theft Auto/Load GTA Vice City Map";
        private const string LOAD_GTA_SA = "Grand Theft Auto/Load GTA San Andreas Map";

        [MenuItem(LOAD_GTA_III, false, LOAD_GTA_ORDER)]
        private static void LoadGTAIII() {
            LoadMap(GtaVersion.III);
        }

        [MenuItem(LOAD_GTA_VC, false, LOAD_GTA_ORDER)]
        private static void LoadGTAVC() {
            LoadMap(GtaVersion.ViceCity);
        }

        [MenuItem(LOAD_GTA_SA, false, LOAD_GTA_ORDER)]
        private static void LoadGTASA() {
            LoadMap(GtaVersion.SanAndreas);
        }

        private static void LoadMap(GtaVersion version) {
            if(EditorUtility.DisplayDialog("Load " + version.GetFormatedGTAName() + " map", "Are you sure you want to load the entire map of " + version.GetFormatedGTAName(true) + "?", "Load", "Cancel"))
                using(var loader = new Loader(version))
                    loader.Load();
        }
        #endregion

        #region Exporting
        private const int EXPORT_ORDER = 10000;
        private const string EXPORT_IMG = "Grand Theft Auto/Export Img File...";
        private const string EXPORT_TXD = "Grand Theft Auto/Export Txd File...";
        private const string EXPORT_IMG_ASSETS = "Assets/Export Img File Here";
        private const string EXPORT_TXD_ASSETS = "Assets/Export Txd File Here";

        private const int LOAD_ORDER = 11000;
        private const string LOAD_DFF = "Assets/Load Dff File...";
        private const string LOAD_WATER = "Grand Theft Auto/Load Water File...";

        #region IMG
        [MenuItem(EXPORT_IMG, false, EXPORT_ORDER)]
        private static void ExportImg() {
            string imgPath, saveDirectory;

            if(SelectSourceFileAndSaveFolder(out imgPath, out saveDirectory, "img"))
                ExportImg(imgPath, saveDirectory);
        }

        [MenuItem(EXPORT_IMG_ASSETS, false, EXPORT_ORDER)]
        private static void ExportImgAssets() {
            foreach(var obj in Selection.objects) {
                var objPath = AssetDatabase.GetAssetPath(obj);
                var saveDirectory = Path.GetDirectoryName(objPath);

                if(!objPath.EndsWith(".img", StringComparison.OrdinalIgnoreCase))
                    continue;

                ExportImg(objPath, saveDirectory);
            }
        }

        [MenuItem(EXPORT_IMG_ASSETS, true)]
        private static bool ExportImgAssetsCheck() {
            foreach(var obj in Selection.objects) {
                var objPath = AssetDatabase.GetAssetPath(obj);

                if(objPath.EndsWith(".img", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static void ExportImg(string imgPath, string saveDirectory) {
            if(string.IsNullOrEmpty(imgPath) || string.IsNullOrEmpty(saveDirectory))
                return;

            using(new Timing("Exporting Img"))
            using(new MemoryCounter())
            using(new AssetEditing())
            using(var progress = new ProgressBar("Exporting Img", 0, 32)) {
                var img = new ImgFile(imgPath);

                progress.Count = img.EntriesCount;

                foreach(var entry in img)
                    try {
                        using(new Timing("Writing File"))
                            File.WriteAllBytes(Path.Combine(saveDirectory, entry.FileName), entry.GetData());

                        progress.Increment(entry.FileName);

                        if(progress.Canceled)
                            break;
                    }
                    catch {
                        Log.Error("Failed to write: {0}", entry);
                    }
            }
        }
        #endregion

        #region TXD
        [MenuItem(EXPORT_TXD, false, EXPORT_ORDER)]
        private static void ExportTxd() {
            string txdPath, saveDirectory;

            if(SelectSourceFileAndSaveFolder(out txdPath, out saveDirectory, "txd"))
                ExportImg(txdPath, saveDirectory);
        }

        [MenuItem(EXPORT_TXD_ASSETS, false, EXPORT_ORDER)]
        private static void ExportTxdAssets() {
            foreach(var obj in Selection.objects) {
                var objPath = AssetDatabase.GetAssetPath(obj);
                var saveDirectory = Path.GetDirectoryName(objPath);

                if(!objPath.EndsWith(".txd", StringComparison.OrdinalIgnoreCase))
                    continue;

                ExportTxd(objPath, saveDirectory);
            }
        }

        [MenuItem(EXPORT_TXD_ASSETS, true)]
        private static bool ExportTxdAssetsCheck() {
            foreach(var obj in Selection.objects) {
                var objPath = AssetDatabase.GetAssetPath(obj);

                if(objPath.EndsWith(".txd", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static void ExportTxd(string txdPath, string saveDirectory) {
            if(string.IsNullOrEmpty(txdPath) || string.IsNullOrEmpty(saveDirectory))
                return;

            using(new Timing("Exporting Txd"))
            using(new MemoryCounter())
            using(new AssetEditing())
            using(var progress = new ProgressBar("Exporting Txd", 0)) {
                var txd = new TxdFile(txdPath);

                progress.Count = txd.TextureCount;

                foreach(var texture in txd)
                    try {
                        using(new Timing("Asset writing"))
                            AssetDatabase.CreateAsset(texture, Path.Combine(saveDirectory, texture.Name + ".asset"));

                        progress.Increment(texture.Name);

                        if(progress.Canceled)
                            break;
                    }
                    catch(Exception e) {
                        Log.Error("Failed to write texture: {0}", e);
                    }
            }
        }
        #endregion

        #region DFF
        [MenuItem(LOAD_DFF, false, LOAD_ORDER)]
        private static void LoadDff() {
            using(new Timing("Loading DFFs into scene"))
            using(new MemoryCounter()) {
                var itemDefinitions = new DefinitionCollection();
                var modelCollection = new ModelCollection();
                var textureCollection = new TextureCollection(true);

                foreach(var obj in Selection.objects) {
                    var objPath = AssetDatabase.GetAssetPath(obj);

                    if(objPath.EndsWith(".dff", StringComparison.OrdinalIgnoreCase)) {
                        var dff = new DffFile(objPath);

                        modelCollection.Add(dff);
                        itemDefinitions.Add(new ItemDefinition(dff.FileNameWithoutExtension));
                    }
                    else if(objPath.EndsWith(".txd", StringComparison.OrdinalIgnoreCase))
                        textureCollection.Add(new TxdFile(objPath));
                }

                using(new Loader(itemDefinitions, modelCollection, textureCollection))
                    foreach(var definition in itemDefinitions)
                        definition.GetObject(true);
            }
        }
        #endregion
        #endregion

        #region Information
        private const int INFORMATION_ORDER = 40000;
        private const string INFORMATION_SUPPORTED_TEXTURE_FORMATS = "Grand Theft Auto/Log Supported Texture Formats";
        private const string INFORMATION_SUPPORTED_RENDER_TEXTURE_FORMATS = "Grand Theft Auto/Log Supported Render Texture Formats";

        [MenuItem(INFORMATION_SUPPORTED_TEXTURE_FORMATS, false, INFORMATION_ORDER)]
        private static void LogTextureFormats() {
            var str = new StringBuilder();

            for(var format = (TextureFormat)0; format < (TextureFormat)62; format++)
                try { str.AppendFormat("Format {0}: {1}\n", format, SystemInfo.SupportsTextureFormat(format) ? "Supported" : "Unsupported"); }
                catch { str.AppendFormat("Format {0}: Error\n", format); }

            Log.Message(str);
        }

        [MenuItem(INFORMATION_SUPPORTED_RENDER_TEXTURE_FORMATS, false, INFORMATION_ORDER)]
        private static void LogRenderTextureFormats() {
            var str = new StringBuilder();

            for(var format = (RenderTextureFormat)0; format < (RenderTextureFormat)24; format++)
                try { str.AppendFormat("Format {0}: {1}\n", format, SystemInfo.SupportsRenderTextureFormat(format) ? "Supported" : "Unsupported"); }
                catch { str.AppendFormat("Format {0}: Error\n", format); }

            Log.Message(str);
        }
        #endregion

        #region Utilities
        private static bool SelectSourceFile(out string sourcePath, string extension) {
            sourcePath = EditorUtility.OpenFilePanel("Select File", string.Empty, extension);
            return !string.IsNullOrEmpty(sourcePath);
        }

        private static bool SelectSourceFileAndSaveFolder(out string sourcePath, out string savePath, string extension) {
            savePath = string.Empty;
            sourcePath = EditorUtility.OpenFilePanel("Select File", string.Empty, extension);

            if(string.IsNullOrEmpty(sourcePath))
                return false;

            savePath = EditorUtility.SaveFolderPanel("Select Save Folder", "", "");

            if(string.IsNullOrEmpty(savePath))
                return false;
            else
                savePath = savePath.Replace(Application.dataPath, "Assets");

            return true;
        }
        #endregion

    }
}