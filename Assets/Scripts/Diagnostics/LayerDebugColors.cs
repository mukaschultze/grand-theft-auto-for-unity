using System.Linq;
using UnityEngine;

namespace GrandTheftAuto.Diagnostics {
    [CreateAssetMenu]
    public class LayerDebugColors : ScriptableObject {

        [SerializeField]
        private Color[] colors = new Color[32];

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod]
#endif
        private static void Init() {
            ResourcesHelper.LayerDebugColors.Value.OnValidate();
        }

        private void Reset() {
            for(var i = 0; i < colors.Length; i++)
                colors[i] = Color.white;
            OnValidate();
        }

        private void OnValidate() {
            //Linq cast doesn't work here, I don't know why
            Shader.SetGlobalVectorArray("_DebugColors", (from color in colors select (Vector4)color).ToArray());
        }

    }
}