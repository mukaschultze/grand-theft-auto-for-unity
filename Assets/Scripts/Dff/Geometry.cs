using System;
using System.Collections.Generic;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.RenderWare;
using UnityEngine;

namespace GrandTheftAuto.Dff {
    public class Geometry {
        [Flags]
        private enum GeometrySectionFlags : short {
            None = 0x00000000,
            TrianglesStrip = 0x00000001,
            HasVertexPositions = 0x00000002,
            HasUV = 0x00000004,
            HasColorInfo = 0x00000008,
            HasNormalsInfo = 0x00000010,
            GeometryLight = 0x00000020,
            ModulateMaterialColor = 0x00000040,
            HasUV2 = 0x00000080,
            //NativeGeometry = 0x01000000 //This can't be a short, so where will it be used?
        }

        public List<Material> Materials { get; private set; }
        public Mesh Mesh { get { if(!loadedMesh) Load(); return loadedMesh; } }

        private RenderWareVersion version;
        private Mesh loadedMesh;
        private BufferReader reader;
        private long offset;
        private long nativeOffset;

        public Geometry(BufferReader reader, RenderWareVersion version) {
            this.version = version;
            this.reader = reader;
            offset = reader.Position;
            Materials = new List<Material>();
        }

        public Geometry(Mesh mesh, List<Material> materials) {
            loadedMesh = mesh;
            Materials = materials;
        }

        public void Load() {
            using(new Timing("Loading Geometry")) {
                var position = reader.Position;
                reader.Position = offset;

                try {
                    loadedMesh = new Mesh();
                    reader.PrewarmBuffer(12);

                    var uv = new Vector2[0];
                    var uv2 = new Vector2[0];
                    var colors = new Color32[0];
                    var tris = new int[0][];
                    var vertices = new Vector3[0];
                    var subMeshCount = 0;
                    var flags = (GeometrySectionFlags)reader.ReadInt16();

                    reader.Skip(2);
                    var triCount = reader.ReadInt32();
                    var vertexCount = reader.ReadInt32();
                    reader.SkipStream(4); //Number of morph targets 

                    if(version < RenderWareVersion.ViceCity_2) {
                        reader.PrewarmBuffer(12);

                        var ambient = reader.ReadSingle();
                        var diffuse = reader.ReadSingle();
                        var specular = reader.ReadSingle();

                        for(var i = 0; i < Materials.Count; i++) {
                            var mat = Materials[i];
                            mat.Ambient = ambient;
                            mat.Diffuse = diffuse;
                            mat.Specular = specular;
                            Materials[i] = mat;
                        }
                    }

                    if((flags | GeometrySectionFlags.HasColorInfo) == flags)
                        colors = ReadColorArray(vertexCount);

                    if((flags | GeometrySectionFlags.HasUV2) == flags) {
                        uv = ReadVector2Array(vertexCount);
                        uv2 = ReadVector2Array(vertexCount);
                    } else if((flags | GeometrySectionFlags.HasUV) == flags)
                        uv = ReadVector2Array(vertexCount);

                    if(nativeOffset != 0) {
                        tris = ReadTrisFromNative(out subMeshCount);
                        reader.SkipStream(triCount * 8);
                    } else
                        tris = ReadTris(triCount);

                    reader.SkipStream(16); //Bounding sphere
                    reader.PrewarmBuffer(8);

                    var hasPositions = reader.ReadInt32() == 1;
                    var hasNormals = reader.ReadInt32() == 1;

                    if(hasPositions)
                        vertices = ReadVector3Array(vertexCount);
                    else
                        Log.Error("Geometry doesn't contains vertex info");

                    loadedMesh.vertices = vertices;
                    loadedMesh.uv = uv;
                    loadedMesh.uv2 = uv2;
                    loadedMesh.colors32 = colors;
                    loadedMesh.subMeshCount = subMeshCount;

                    for(var i = 0; i < subMeshCount; i++)
                        loadedMesh.SetTriangles(tris[i], i, false);

                    if(hasNormals || (flags | GeometrySectionFlags.HasNormalsInfo) == flags)
                        loadedMesh.normals = ReadVector3Array(vertexCount);
                    else
                        loadedMesh.normals = CalculateNormals(vertices, loadedMesh.triangles);

#if UNITY_EDITOR
                    using(new Timing("Optimizing mesh")) {
                        // compression causes misalignment for some large meshes
                        UnityEditor.MeshUtility.SetMeshCompression(loadedMesh, UnityEditor.ModelImporterMeshCompression.Off);
                        UnityEditor.MeshUtility.Optimize(loadedMesh);
                    }
#endif

                    loadedMesh.RecalculateBounds();
                    loadedMesh.UploadMeshData(true);
                } catch(Exception e) {
                    Log.Exception(e);
                } finally {
                    reader.Position = position;
                }
            }
        }

        private int[][] ReadTris(int count) {
            var tris = new int[count * 3];

            reader.PrewarmBuffer(count * 8);

            for(var i = 0; i < count; i++) {
                tris[i * 3 + 0] = reader.ReadInt16();
                tris[i * 3 + 1] = reader.ReadInt16();
                reader.Skip(2); //Material (?)
                tris[i * 3 + 2] = reader.ReadInt16();
            }

            return new int[][] { tris };
        }

        private int[][] ReadTrisFromNative(out int subMeshCount) {
            var position = reader.Position;
            reader.Position = nativeOffset;

            try {
                reader.PrewarmBuffer(8);

                var isStrip = reader.ReadInt32() == 1;
                subMeshCount = reader.ReadInt32();
                var tris = new int[subMeshCount][];

                reader.SkipStream(4); //Face Count

                for(var j = 0; j < subMeshCount; j++) {
                    reader.PrewarmBuffer(8);

                    var indicesCount = reader.ReadInt32();
                    var materialIndex = reader.ReadInt32();
                    var indices = new int[indicesCount];

                    reader.PrewarmBuffer(indicesCount * 4);

                    for(var i = 0; i < indicesCount; i++)
                        indices[i] = reader.ReadInt32();

                    tris[materialIndex] = isStrip ? GetTrianglesFromStrip(indices) : GetTrianglesList(indices);
                }

                return tris;
            } catch(Exception e) {
                Log.Exception(e);
                subMeshCount = 0;
                return null;
            } finally {
                reader.Position = position;
            }
        }

        private Vector3[] CalculateNormals(Vector3[] vertices, int[] tris) {
            var normals = new Vector3[vertices.Length];

            for(var i = 0; i < tris.Length; i += 3) {
                var v0 = vertices[tris[i + 2]];
                var v1 = vertices[tris[i + 1]];
                var v2 = vertices[tris[i + 0]];

                var normal = Vector3.Cross(v0 - v1, v2 - v1);

                normals[tris[i + 0]] += normal;
                normals[tris[i + 1]] += normal;
                normals[tris[i + 2]] += normal;
            }

            //This will "fix" most of normals where the vertices share opposite faces
            //GTA VC vegetation will be shitty as hell with this fix, without it the mesh would be black
            for(var i = 0; i < normals.Length; i++)
                if(normals[i] == Vector3.zero)
                    normals[i] = Vector3.up;

            return normals;
        }

        private Vector2[] ReadVector2Array(int count) {
            var array = new Vector2[count];

            reader.PrewarmBuffer(count * 8);

            for(var i = 0; i < count; i++)
                array[i] = new Vector2() {
                    x = reader.ReadSingle(),
                    y = 1f - reader.ReadSingle()
                };

            return array;
        }

        private Vector3[] ReadVector3Array(int count) {
            var array = new Vector3[count];

            reader.PrewarmBuffer(count * 12);

            for(var i = 0; i < count; i++)
                array[i] = new Vector3() {
                    x = reader.ReadSingle(),
                    z = reader.ReadSingle(),
                    y = reader.ReadSingle()
                };

            return array;
        }

        private Color32[] ReadColorArray(int count) {
            var colors = new Color32[count];
            var buffer = reader.ReadBytes(count * 4);

            for(var i = 0; i < count; i++)
                colors[i] = new Color32() {
                    r = buffer[i * 4 + 0],
                    g = buffer[i * 4 + 1],
                    b = buffer[i * 4 + 2],
                    a = buffer[i * 4 + 3]
                };

            return colors;
        }

        private static int[] GetTrianglesFromStrip(int[] strip) {
            //TRIANGLE STRIPS SUCKS!!!!!
            var result = new int[(strip.Length - 2) * 3];

            for(var i = 0; i < strip.Length - 2; i++) {
                var v0 = strip[i + 0];
                var v1 = strip[i + 1 + (i & 1)];
                var v2 = strip[i + 2 - (i & 1)];

                if(v0 == v1 || v1 == v2 || v2 == v0)
                    continue;

                result[i * 3 + 2] = v0;
                result[i * 3 + 1] = v1;
                result[i * 3 + 0] = v2;
            }

            return result;
        }

        private static int[] GetTrianglesList(int[] tris) {
            var result = new int[tris.Length];
            var triCount = result.Length / 3;

            for(var i = 0; i < triCount; i++) {
                result[i * 3 + 0] = tris[i * 3 + 2];
                result[i * 3 + 1] = tris[i * 3 + 1];
                result[i * 3 + 2] = tris[i * 3 + 0];
            }

            return result;
        }

        public void SetMaterialSplitData(BufferReader reader) {
            nativeOffset = reader.Position;
        }

        public static implicit operator Mesh(Geometry geometry) {
            return geometry.Mesh;
        }
    }
}