using GrandTheftAuto.Diagnostics;
using UnityEngine;

namespace GrandTheftAuto.Ide.Fx {
    public class FxLight : FxDefinition {
        public bool Wet { get; private set; }
        public bool Flares { get; private set; }
        public string Corona { get; private set; }
        public string Shadow { get; private set; }
        public float Distance { get; private set; }
        public float OuterRange { get; private set; }
        public float InnerRange { get; private set; }
        public float Size { get; private set; }
        public float ShadowIntensity { get; private set; }
        public DefinitionFlags Flags { get; private set; }
        public FlashType Flash { get; private set; }

        private Light loadedLight;

        public FxLight(string[] tokens) : base(tokens) {
            Corona = tokens[9].Replace("\"", "");
            Shadow = tokens[10].Replace("\"", "");

            Distance = float.Parse(tokens[11]);
            OuterRange = float.Parse(tokens[12]);
            Size = float.Parse(tokens[13]);
            InnerRange = float.Parse(tokens[14]);

            ShadowIntensity = byte.Parse(tokens[15]) / 255f;
            Flash = (FlashType)int.Parse(tokens[16]);
            Wet = int.Parse(tokens[17]) == 1;
            Flares = int.Parse(tokens[18]) == 1;
        }

        //TODO: Implement Halo
        //TODO: Implement Flickering
        public Light GetLight(Transform parent) {
            using(new Timing("Creating light")) {
                if(loadedLight)
                    return Object.Instantiate(loadedLight);

                loadedLight = new GameObject("FxLight").AddComponent<Light>();
                loadedLight.gameObject.AddComponent<ItemDefinitionComponent>().RegisterDefinition(this);
                loadedLight.cullingMask = ~(Layer.LOD.Mask | Layer.IslandLOD.Mask | Layer.CullOff.Mask | Layer.LODGroup.Mask);
                loadedLight.transform.SetParent(parent);
                loadedLight.type = LightType.Point;
                loadedLight.shadows = LightShadows.Hard;
                loadedLight.transform.localPosition = Position;
                loadedLight.color = Color;
                loadedLight.range = OuterRange;
                loadedLight.intensity = Size;
                loadedLight.bounceIntensity = 0f;

                switch(Flash) {
                    case FlashType.TrafficLights:
                        loadedLight.enabled = false;
                        break;
                }

                return loadedLight;
            }
        }
    }
}