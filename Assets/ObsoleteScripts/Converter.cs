using GrandTheftAuto.Water;
using UnityEngine;
using UnityEngine.UI;

namespace GrandTheftAuto {
    //TODO: Organize this old crap
    public static class Converter {
        public static GameObject CreateUnityWater(this WaterFile water) {
            using(var helper = new VertexHelper()) {
                foreach(var plane in water.Planes) {
                    var p1 = new UIVertex { position = plane.P1 };
                    var p2 = new UIVertex { position = plane.P2 };
                    var p3 = new UIVertex { position = plane.P3 };
                    var p4 = new UIVertex { position = plane.P4 };

                    helper.AddUIVertexQuad(new UIVertex[] { p1, p2, p3, p4 });
                }

                var go = new GameObject("Water");
                var mesh = new Mesh();

                helper.FillMesh(mesh);

                mesh.name = "Water";
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                go.AddComponent<MeshRenderer>().material = ResourcesHelper.WaterMaterial;
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.layer = LayerMask.NameToLayer("Water");

                return go;
            }
        }

    }
}