using GrandTheftAuto.Diagnostics;
using UnityEngine;

namespace GrandTheftAuto.Ide.Fx {
    public abstract class FxDefinition {
        public int TargetObjectID { get; protected set; }
        public Vector3 Position { get; protected set; }
        public Color Color { get; protected set; }

        public FxDefinition(string[] tokens) {
            TargetObjectID = int.Parse(tokens[0]);
            Position = new Vector3(float.Parse(tokens[1]), float.Parse(tokens[3]), float.Parse(tokens[2]));
            Color = new Color32(byte.Parse(tokens[4]), byte.Parse(tokens[5]), byte.Parse(tokens[6]), byte.MaxValue);
        }

        static FxDefinition() {
            ItemDefinition.GameObjectModifiers += (go, dff, definition) => {
                if(definition.Effects == null)
                    return;

                using(Timing.Get("Creating Effects"))
                for(var i = 0; i < definition.Effects.Count; i++)
                    //TODO: Implement other effects
                    if(definition.Effects[i] is FxLight)
                        (definition.Effects[i] as FxLight).GetLight(go.transform);
            };
        }
    }
}