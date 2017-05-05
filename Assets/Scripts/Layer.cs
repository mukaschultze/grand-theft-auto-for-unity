using UnityEngine;

namespace GrandTheftAuto {
    public struct Layer {
        public static readonly Layer Default = 0;
        public static readonly Layer TransparentFX = 1;
        public static readonly Layer IgnoreRaycast = 2;

        public static readonly Layer Water = 4;
        public static readonly Layer UI = 5;

        public static readonly Layer LODGroup = 12;
        public static readonly Layer LOD = 13;
        public static readonly Layer IslandLOD = 14;
        public static readonly Layer LowDistance = 15;
        public static readonly Layer MediumDistance = 16;
        public static readonly Layer HighDistance = 17;
        public static readonly Layer VeryHighDistance = 18;
        public static readonly Layer CullOff = 19;

        public int Value { get; private set; }
        public int Mask { get { return (int)Mathf.Pow(2f, Value); } }
        public string Name { get { return LayerMask.LayerToName(Value); } }

        public Layer(int layer) { Value = layer; }
        public Layer(string name) { Value = LayerMask.NameToLayer(name); }

        public static implicit operator int(Layer layer) { return layer.Value; }
        public static implicit operator string(Layer layer) { return layer.Name; }
        public static implicit operator LayerMask(Layer layer) { return layer.Mask; }

        public static implicit operator Layer(int value) { return new Layer(value); }
        public static implicit operator Layer(string name) { return new Layer(name); }
        public static implicit operator Layer(LayerMask mask) { return (int)Mathf.Sqrt(mask.value); }
    }
}