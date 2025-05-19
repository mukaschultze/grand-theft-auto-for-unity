using UnityEngine;
using GrandTheftAuto.Diagnostics;
using UnityEngine.UI;
using TMPro;

namespace GrandTheftAuto.Dff.Extensions {
    // https://gtamods.com/wiki/2d_Effect_(RW_Section)#Entry_Type_7_-_Street_sign
    public class StreetSign : Effect2D {

        private Vector3 position;
        private Vector2 size;
        private Vector3 rotation;

        private string text = "";
        private uint textColor = 0xFFFFFFFF;

        public StreetSign(Vector3 position, BufferReader reader) {
            this.position = position;

            var width = reader.ReadSingle();
            var height = reader.ReadSingle();
            this.size = new Vector2(width, height);

            var rotationX = reader.ReadSingle();
            var rotationZ = reader.ReadSingle();
            var rotationY = reader.ReadSingle();
            this.rotation = new Vector3(rotationX, rotationY, rotationZ);

            var flags = reader.ReadInt16();

            var flagLines = flags & 0b00000011;
            var flagMaxSymbols = flags & 0b00001100;
            var flagColor = flags & 0b00110000;

            var colors = new[] {
                0xFFFFFFFF, // White
                0xFF000000, // Black
                0xFF808080, // Gray
                0xFF0000FF, // Red
            };
            this.textColor = colors[flagColor >> 4];

            var strLength = (new[] { 16, 2, 4, 8 })[flagMaxSymbols >> 2];
            var text0 = reader.ReadString(16).Substring(0, strLength);
            var text1 = reader.ReadString(16).Substring(0, strLength);
            var text2 = reader.ReadString(16).Substring(0, strLength);
            var text3 = reader.ReadString(16).Substring(0, strLength);

            switch(flagLines) {
                case 0:
                    this.text = $"{text0}\n{text1}\n{text2}\n{text3}";
                    break;
                case 1:
                    this.text = $"{text0}";
                    break;
                case 2:
                    this.text = $"{text0}\n{text1}";
                    break;
                case 3:
                    this.text = $"{text0}\n{text1}\n{text2}";
                    break;
            }
        }

        public override void CreateEffectInUnity(GameObject owner) {
            using(new Timing("Creating effect (Street sign)")) {
                var go = Object.Instantiate<GameObject>(ResourcesHelper.StreetSignPrefab);
                var text = go.GetComponent<TextMeshPro>();
                var transform = go.GetComponent<RectTransform>();

                Log.Message("Rotation: {0}", rotation);
                transform.SetParent(owner.transform, true);
                transform.position = position;
                transform.sizeDelta = size;
                transform.localRotation = Quaternion.Euler(-90, 0, 0) * Quaternion.Euler(rotation);

                text.text = this.text;
                text.color = new Color32(
                    (byte)(textColor >> 0),
                    (byte)(textColor >> 8),
                    (byte)(textColor >> 16),
                    (byte)(textColor >> 24)
                );

            }
        }
    }
}