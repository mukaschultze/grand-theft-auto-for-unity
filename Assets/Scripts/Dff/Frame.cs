using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrandTheftAuto.Dff {
    [Serializable]
    public class Frame {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Matrix4x4 Rotation { get; set; }
        public Geometry Geometry { get; set; }
        public Frame Parent { get; set; }
        public List<Frame> Children { get; set; }

        public Frame() {
            Name = string.Empty;
            Children = new List<Frame>();
        }
    }
}