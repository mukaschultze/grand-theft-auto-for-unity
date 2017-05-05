using UnityEngine;

namespace GrandTheftAuto {
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class CameraLod : MonoBehaviour {

        [HideInInspector]
        [SerializeField]
        private Camera dummyCam;
        private Camera cam;

        private void OnEnable() {
            cam = GetComponent<Camera>();
        }

        private void OnPreCull() {
            if(!dummyCam)
                Reset();

            cam.clearFlags = CameraClearFlags.Depth;
            cam.cullingMask = ~(Layer.LOD.Mask | Layer.IslandLOD.Mask);
            cam.farClipPlane = 300f * QualitySettings.lodBias;

            dummyCam.CopyFrom(cam);
            dummyCam.clearFlags = CameraClearFlags.Skybox;
            dummyCam.depth = cam.depth - 1;
            dummyCam.fieldOfView = cam.fieldOfView;
            dummyCam.cullingMask = ~cam.cullingMask;
            dummyCam.nearClipPlane = 10f * QualitySettings.lodBias;
            dummyCam.farClipPlane = 3000f * QualitySettings.lodBias;
        }

        private void Reset() {
            dummyCam = new GameObject("Dummy Camera").AddComponent<Camera>();
            dummyCam.transform.SetParent(transform);
            dummyCam.transform.localPosition = Vector3.zero;
            dummyCam.transform.localRotation = Quaternion.identity;
            dummyCam.gameObject.hideFlags = HideFlags.NotEditable;
        }

    }
}
