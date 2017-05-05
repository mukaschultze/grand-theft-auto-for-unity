using UnityEngine;

namespace GrandTheftAuto {
    public static class DistanceLod {

        private static float[] distances;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod]
#endif
        private static void Init() {
            distances = new float[32];
            Camera.onPreCull += OnPreCull;
        }

        private static void OnPreCull(Camera cam) {
            switch(cam.cameraType) {
                case CameraType.Game:
                case CameraType.SceneView:
                    distances[Layer.LOD] = 1000f * QualitySettings.lodBias;
                    distances[Layer.IslandLOD] = 0f;
                    distances[Layer.CullOff] = 0f;

                    distances[Layer.LowDistance] = 50f * QualitySettings.lodBias;
                    distances[Layer.MediumDistance] = 100f * QualitySettings.lodBias;
                    distances[Layer.HighDistance] = 200f * QualitySettings.lodBias;
                    distances[Layer.VeryHighDistance] = 300f * QualitySettings.lodBias;

                    cam.layerCullSpherical = true;
                    cam.layerCullDistances = distances;
                    break;
            }
        }
    }
}