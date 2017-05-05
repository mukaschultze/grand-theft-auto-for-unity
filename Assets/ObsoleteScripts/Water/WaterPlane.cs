using UnityEngine;

namespace GrandTheftAuto.Water {
    public struct WaterPlane {
        public int Mode { get; set; }
        public Vector3 P1 { get; set; }
        public Vector3 P2 { get; set; }
        public Vector3 P3 { get; set; }
        public Vector3 P4 { get; set; }

        public WaterPlane(Vector3 p1, Vector3 p2, Vector3 p3) : this(p1, p2, p3, 1) { }

        public WaterPlane(Vector3 p1, Vector3 p2, Vector3 p3, int mode) {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            P4 = new Vector3(p3.x, (p3.y + p1.y + p2.y) / 3f, p1.z);
            Mode = mode;
        }

        public WaterPlane(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) : this(p1, p2, p3, p4, 1) { }

        public WaterPlane(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, int mode) {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            P4 = p4;
            Mode = mode;
        }
    }
}