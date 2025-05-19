using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace GrandTheftAuto {
    [RequireComponent(typeof(Camera))]
    public class DynamicLod : MonoBehaviour {

        public float frameRateTime = 0.2f;

        public float adjustmentSpeed = 0.1f;
        public float targetFrameRate = 60f;

        public float currentBias = 0f;

        private int lastFrameCount = 0;
        private double lastTime = 0d;
        private float frameRate = 0f;

        private void Update() {
            if(Time.realtimeSinceStartupAsDouble - lastTime > frameRateTime) {
                frameRate = (float)((Time.frameCount - lastFrameCount) / (Time.realtimeSinceStartupAsDouble - lastTime));
                lastTime = Time.realtimeSinceStartupAsDouble;
                lastFrameCount = Time.frameCount;

                // Ignore false positives like when the game is not focused
                if(frameRate > 1f) {
                    var delta = (frameRate - targetFrameRate) * adjustmentSpeed;

                    QualitySettings.lodBias = Math.Clamp(QualitySettings.lodBias + delta, 0.5f, 50f);
                    currentBias = QualitySettings.lodBias;

                }
            }
        }
    }
}
