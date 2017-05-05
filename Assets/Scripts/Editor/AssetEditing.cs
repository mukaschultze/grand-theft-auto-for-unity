using System;
using UnityEditor;

namespace GrandTheftAuto.Editor {
    public class AssetEditing : IDisposable {
        public AssetEditing() {
            AssetDatabase.StartAssetEditing();
        }

        public void Dispose() {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }
    }
}
