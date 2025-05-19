using GrandTheftAuto.Water;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Playables;
using System;

namespace GrandTheftAuto {
    public static class Converter {
        public static GameObject CreateUnityWater(this WaterFile water) {
            using(var helper = new VertexHelper()) {
                var planes = water.Planes.ToList();

                var xMin = water.Planes.Min(p => Mathf.Min(p.P1.x, p.P2.x, p.P3.x, p.P4.x));
                var xMax = water.Planes.Max(p => Mathf.Max(p.P1.x, p.P2.x, p.P3.x, p.P4.x));
                var zMin = water.Planes.Min(p => Mathf.Min(p.P1.z, p.P2.z, p.P3.z, p.P4.z));
                var zMax = water.Planes.Max(p => Mathf.Max(p.P1.z, p.P2.z, p.P3.z, p.P4.z));

                var inf = 1000000f;

                planes.Add(new WaterPlane { P1 = new(xMin, 0, zMin), P2 = new(xMax, 0, zMin), P3 = new(xMax, 0, zMin - inf), P4 = new(xMin, 0, zMin - inf) });
                planes.Add(new WaterPlane { P1 = new(xMax, 0, zMin), P2 = new(xMax, 0, zMax), P3 = new(xMax + inf, 0, zMax), P4 = new(xMax + inf, 0, zMin) });
                planes.Add(new WaterPlane { P1 = new(xMax, 0, zMax), P2 = new(xMin, 0, zMax), P3 = new(xMin, 0, zMax + inf), P4 = new(xMax, 0, zMax + inf) });
                planes.Add(new WaterPlane { P1 = new(xMin, 0, zMax), P2 = new(xMin, 0, zMin), P3 = new(xMin - inf, 0, zMin), P4 = new(xMin - inf, 0, zMax) });

                planes.Add(new WaterPlane { P1 = new(xMin, 0, zMin), P2 = new(xMin, 0, zMin - inf), P3 = new(xMin - inf, 0, zMin - inf), P4 = new(xMin - inf, 0, zMin) });
                planes.Add(new WaterPlane { P1 = new(xMax, 0, zMin), P2 = new(xMax + inf, 0, zMin), P3 = new(xMax + inf, 0, zMin - inf), P4 = new(xMax, 0, zMin - inf) });
                planes.Add(new WaterPlane { P1 = new(xMax, 0, zMax), P2 = new(xMax, 0, zMax + inf), P3 = new(xMax + inf, 0, zMax + inf), P4 = new(xMax + inf, 0, zMax) });
                planes.Add(new WaterPlane { P1 = new(xMin, 0, zMax), P2 = new(xMin - inf, 0, zMax), P3 = new(xMin - inf, 0, zMax + inf), P4 = new(xMin, 0, zMax + inf) });

                var mapSize = 7000f;

                foreach(var plane in planes) {
                    var p1 = new UIVertex { position = plane.P1, uv0 = new Vector2(plane.P1.x / mapSize, plane.P1.z / mapSize), normal = Vector3.up, tangent = new Vector4(1, 0, 0, 1), color = Color.white };
                    var p2 = new UIVertex { position = plane.P2, uv0 = new Vector2(plane.P2.x / mapSize, plane.P2.z / mapSize), normal = Vector3.up, tangent = new Vector4(1, 0, 0, 1), color = Color.white };
                    var p3 = new UIVertex { position = plane.P3, uv0 = new Vector2(plane.P3.x / mapSize, plane.P3.z / mapSize), normal = Vector3.up, tangent = new Vector4(1, 0, 0, 1), color = Color.white };
                    var p4 = new UIVertex { position = plane.P4, uv0 = new Vector2(plane.P4.x / mapSize, plane.P4.z / mapSize), normal = Vector3.up, tangent = new Vector4(1, 0, 0, 1), color = Color.white };

                    helper.AddUIVertexQuad(new UIVertex[] { p1, p2, p3, p4 });
                }

                var go = GameObject.Instantiate<GameObject>(ResourcesHelper.WaterPrefab);
                var mesh = new Mesh();

                helper.FillMesh(mesh);

                mesh.name = "Water";
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                // go.GetComponent<MeshRenderer>().material = ResourcesHelper.WaterPrefab;
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                go.layer = LayerMask.NameToLayer("Water");
                go.transform.localScale = Vector3.one;
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;

                return go;
            }
        }

    }
}