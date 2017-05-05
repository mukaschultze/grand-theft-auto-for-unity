using UnityEngine;

namespace GrandTheftAuto {
    public class FreeCamera : MonoBehaviour {

        [SerializeField]
        private bool enableInputCapture = true;
        [SerializeField]
        private bool holdRightMouseCapture = false;

        [SerializeField]
        private float lookSpeed = 5f;
        [SerializeField]
        private float moveSpeed = 5f;
        [SerializeField]
        private float sprintSpeed = 50f;

        private bool inputCaptured;
        private float yaw;
        private float pitch;

        private void Awake() {
            enabled = enableInputCapture;
        }

        private void OnValidate() {
            if(Application.isPlaying)
                enabled = enableInputCapture;
        }

        private void CaptureInput() {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            inputCaptured = true;
            yaw = transform.eulerAngles.y;
            pitch = transform.eulerAngles.x;
        }

        private void ReleaseInput() {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            inputCaptured = false;
        }

        private void OnApplicationFocus(bool focus) {
            if(inputCaptured && !focus)
                ReleaseInput();
        }

        private void Update() {
            if(!inputCaptured)
                if(!holdRightMouseCapture && Input.GetMouseButtonDown(0))
                    CaptureInput();
                else if(holdRightMouseCapture && Input.GetMouseButtonDown(1))
                    CaptureInput();

            if(!inputCaptured)
                return;

            if(inputCaptured)
                if(!holdRightMouseCapture && Input.GetKeyDown(KeyCode.Escape))
                    ReleaseInput();
                else if(holdRightMouseCapture && Input.GetMouseButtonUp(1))
                    ReleaseInput();

            var rotStrafe = Input.GetAxis("Mouse X");
            var rotFwd = Input.GetAxis("Mouse Y");

            yaw = (yaw + lookSpeed * rotStrafe) % 360f;
            pitch = (pitch - lookSpeed * rotFwd) % 360f;
            transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);

            var speed = Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed);
            var forward = speed * Input.GetAxis("Vertical");
            var right = speed * Input.GetAxis("Horizontal");
            var up = speed * ((Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f));

            transform.position += transform.forward * forward + transform.right * right + Vector3.up * up;
        }
    }
}