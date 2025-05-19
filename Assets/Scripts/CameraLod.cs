using UnityEngine;

namespace GrandTheftAuto {
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class CameraLod : MonoBehaviour {

        [HideInInspector]
        [SerializeField]
        private Camera dummyCam;
        private Camera cam;

        public bool useDummyCam = true;

        private void OnEnable() {
            cam = GetComponent<Camera>();
        }

        private void OnPreCull() {
            if(!dummyCam)
                Reset();

            cam.clearFlags = useDummyCam ? CameraClearFlags.Depth : CameraClearFlags.Skybox;
            cam.cullingMask = useDummyCam ? ~(Layer.LOD.Mask | Layer.IslandLOD.Mask) : -1;
            cam.farClipPlane = 10000f;

            dummyCam.enabled = useDummyCam;
            dummyCam.CopyFrom(cam);
            dummyCam.clearFlags = CameraClearFlags.Skybox;
            dummyCam.depth = cam.depth - 1;
            dummyCam.fieldOfView = cam.fieldOfView;
            dummyCam.cullingMask = ~cam.cullingMask;
            dummyCam.nearClipPlane = 100f;
            dummyCam.farClipPlane = 30000f;
        }

        private void Reset() {
            dummyCam = new GameObject("Dummy Camera").AddComponent<Camera>();
            dummyCam.transform.SetParent(transform);
            dummyCam.transform.localPosition = Vector3.zero;
            dummyCam.transform.localRotation = Quaternion.identity;
        }

    }
}
