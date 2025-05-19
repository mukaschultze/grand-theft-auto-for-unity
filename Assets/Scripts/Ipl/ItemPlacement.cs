using UnityEngine;

namespace GrandTheftAuto.Ipl {
    public struct ItemPlacement {
        public int DefinitionID { get; set; }
        public int LodDefinitionID { get; set; }
        public string ItemName { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
    }
}
