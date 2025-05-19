using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace GrandTheftAuto.Ide {
    public class MapObject : ItemDefinition {
        public int ObjectCount { get; protected set; }
        public float DrawDistance { get; protected set; }
        public DefinitionFlags Flags { get; protected set; }

        protected MapObject(string[] tokens, bool _) : base(tokens) { }

        public MapObject(string[] tokens) : base(tokens) {
            if(tokens.Length == 5) {
                ObjectCount = 1;
                DrawDistance = float.Parse(tokens[3]);
            } else {
                ObjectCount = int.Parse(tokens[3]);
                DrawDistance = float.Parse(tokens[4]);
            }

            Flags = (DefinitionFlags)int.Parse(tokens[tokens.Length - 1]);
        }

        static MapObject() {
            GameObjectModifiers += (go, dff, definition) => {
                var mapObject = definition as MapObject;

                if(mapObject == null)
                    return;

                mapObject.MarkAsShadow(go);
            };
            TransformModifiers += (transform, frame, definition) => {
                var mapObject = definition as MapObject;

                if(mapObject == null)
                    return;

                if(frame.Name.EndsWith("_l1", StringComparison.OrdinalIgnoreCase))
                    transform.gameObject.SetActive(false);

                mapObject.SetLayer(transform.gameObject);
            };
            RendererModifiers += (renderer, geometry, txdName, definition) => {
                var mapObject = definition as MapObject;

                if(mapObject == null)
                    return;

                mapObject.SetupMaterialsFlags(renderer);
            };
        }

        private void MarkAsShadow(GameObject go) {
            if((Flags & DefinitionFlags.Shadows) != 0)
                go.SetActive(false);
        }

        private void SetLayer(GameObject go) {
            if(DrawDistance > 300f) {
                if(go.name.StartsWith("IslandLOD", StringComparison.Ordinal)) {
                    go.layer = Layer.IslandLOD;
                    go.SetActive(false); // temp disable
                } else
                    go.layer = Layer.LOD;
            } else if((Flags | DefinitionFlags.DrawDistanceOff) == Flags)
                go.layer = Layer.CullOff;
            else if(DrawDistance <= 50f)
                go.layer = Layer.LowDistance;
            else if(DrawDistance <= 100f)
                go.layer = Layer.MediumDistance;
            else if(DrawDistance <= 200f)
                go.layer = Layer.HighDistance;
            else
                go.layer = Layer.VeryHighDistance;
        }

        private void SetupMaterialsFlags(Renderer renderer) {
            foreach(var material in renderer.sharedMaterials) {
                var twoSided = (Flags & DefinitionFlags.FaceCullingOff) == 0 && Loader.Current.Version >= GtaVersion.SanAndreas;

                renderer.shadowCastingMode = twoSided ? ShadowCastingMode.TwoSided : ShadowCastingMode.On;

                Utilities.SetMaterialCulling(material, twoSided);
                Utilities.SetMaterialTransparency(material, (Flags & DefinitionFlags.DrawLast) != 0);

                material.SetInt("_Layer", renderer.gameObject.layer);
            }
        }

    }
}