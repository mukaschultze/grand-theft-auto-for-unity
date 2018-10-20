//using System;
//using System.Collections.Generic;
//using System.IO;
//using GrandTheftAuto.Data;
//using GrandTheftAuto.Dff;
//using GrandTheftAuto.Diagnostics;
//using GrandTheftAuto.Img;
//using UnityEngine;
//using Material = GrandTheftAuto.Dff.Material;

//namespace GrandTheftAuto.Caches {
//    public class FrameCache : Cache {

//        private Dictionary<string, Frame> rootFrames = new Dictionary<string, Frame>();

//        public FrameCache() : this(new DffFile[0]) { }

//        public FrameCache(params DffFile[] dffs) {
//            AddDff(dffs);
//        }

//        public void AddRootFrame(string modelName, Frame frame) {
//            using(Timing.Get("Adding frame"))
//                try {
//                    rootFrames.Add(modelName, frame);
//                }
//                catch {
//                    Log.Warning("The frame for the dff file is already present in the cache: {0}.dff", modelName);
//                }
//        }

//        public void AddDff(DffFile dff) {
//            using(Timing.Get("Adding Dff"))
//                AddRootFrame(dff.FileName, dff.RootFrame);
//        }

//        public void AddDff(DffFile[] dffs) {
//            using(Timing.Get("Adding Dff"))
//                for(int i = 0; i < dffs.Length; i++)
//                    AddDff(dffs[i]);
//        }

//        public void AddDff(List<DffFile> dffs) {
//            using(Timing.Get("Adding Dff"))
//                for(int i = 0; i < dffs.Count; i++)
//                    AddDff(dffs[i]);
//        }

//        public Frame GetRootFrame(string modelName) {
//            using(Timing.Get("Retrieving Frame"))
//                try {
//                    return rootFrames[modelName];
//                }
//                catch {
//                    Log.Warning("Could not find frame \"{0}\" in cache", modelName);
//                    return null;
//                }
//        }

//        public override void Load(BufferReader reader) {
//            using(Timing.Get("Reading Cache")) {
//                var modelCount = reader.ReadInt32();
//                for(var i = 0; i < modelCount; i++)
//                    AddRootFrame(reader.ReadString(), ReadFrame(reader));
//            }
//        }

//        public override void Save(BinaryWriter writer) {
//            using(Timing.Get("Writing Cache")) {
//                writer.Write(rootFrames.Count);
//                foreach(var kvp in rootFrames) {
//                    writer.Write(kvp.Key);
//                    WriteFrame(kvp.Value, writer);
//                }
//            }
//        }

//        public override void Create(GtaVersion version) {
//            using(new TempWorkingFolder(Directories.GetPathFromVersion(version)))
//            using(Timing.Get("Exporting frame cache"))
//            using(new GCPass())
//                try {
//                    var imgs = new List<ImgFile> { ImgFile.GetMainImg(version) };

//                    imgs.AddRange(DataFile.GetMainData(version).IMGs);
//                    imgs.AddRange(DataFile.GetVersionSpecificData(version).IMGs);

//                    foreach(var img in imgs)
//                        foreach(var entry in img.Entries)
//                            if(entry.FileName.EndsWith("dff", StringComparison.InvariantCultureIgnoreCase))
//                                AddDff(new DffFile(entry));

//                    Save(version);
//                }
//                catch(Exception e) {
//                    Log.Exception(e);
//                }
//        }

//        private Frame ReadFrame(BufferReader reader) {
//            var result = new Frame();

//            result.Name = reader.ReadString();
//            reader.PrewarmBuffer(49);

//            result.Position = new Vector3() {
//                x = reader.ReadSingle(),
//                y = reader.ReadSingle(),
//                z = reader.ReadSingle(),
//            };
//            result.Rotation = new Matrix4x4() {
//                m00 = reader.ReadSingle(),
//                m01 = reader.ReadSingle(),
//                m02 = reader.ReadSingle(),
//                m10 = reader.ReadSingle(),
//                m11 = reader.ReadSingle(),
//                m12 = reader.ReadSingle(),
//                m20 = reader.ReadSingle(),
//                m21 = reader.ReadSingle(),
//                m22 = reader.ReadSingle()
//            };

//            if(reader.ReadBoolean()) {
//                reader.PrewarmBuffer(11);

//                var mesh = new Mesh();
//                var materials = new List<Material>();
//                var vertexCount = reader.ReadInt32();
//                var submeshCount = reader.ReadInt32();
//                var hasUv = reader.ReadBoolean();
//                var hasUv2 = reader.ReadBoolean();
//                var hasColors = reader.ReadBoolean();

//                var vertices = new Vector3[vertexCount];
//                var normals = new Vector3[vertexCount];
//                var uv = new Vector2[hasUv ? vertexCount : 0];
//                var uv2 = new Vector2[hasUv2 ? vertexCount : 0];
//                var colors = new Color[hasColors ? vertexCount : 0];

//                reader.PrewarmBuffer(vertexCount * 24);

//                for(int i = 0, v = 0; i < vertexCount; i++, v += 24) {
//                    vertices[i] = new Vector3() {
//                        x = reader.ReadSingle(),
//                        y = reader.ReadSingle(),
//                        z = reader.ReadSingle()
//                    };
//                    normals[i] = new Vector3() {
//                        x = reader.ReadSingle(),
//                        y = reader.ReadSingle(),
//                        z = reader.ReadSingle()
//                    };
//                }

//                if(hasUv) {
//                    reader.PrewarmBuffer(vertexCount * 8);

//                    for(int i = 0, v = 0; i < vertexCount; i++, v += 8)
//                        uv[i] = new Vector2() {
//                            x = reader.ReadSingle(),
//                            y = reader.ReadSingle(),
//                        };
//                }
//                if(hasUv2) {
//                    reader.PrewarmBuffer(vertexCount * 8);

//                    for(int i = 0, v = 0; i < vertexCount; i++, v += 8)
//                        uv2[i] = new Vector2() {
//                            x = reader.ReadSingle(),
//                            y = reader.ReadSingle(),
//                        };
//                }
//                if(hasColors) {
//                    reader.PrewarmBuffer(vertexCount * 16);

//                    for(int i = 0, v = 0; i < vertexCount; i++, v += 16)
//                        colors[i] = new Color() {
//                            r = reader.ReadSingle(),
//                            g = reader.ReadSingle(),
//                            b = reader.ReadSingle(),
//                            a = reader.ReadSingle()
//                        };
//                }

//                mesh.vertices = vertices;
//                mesh.normals = normals;
//                mesh.uv = uv;
//                mesh.uv2 = uv2;
//                mesh.colors = colors;
//                mesh.subMeshCount = submeshCount;

//                for(int i = 0; i < submeshCount; i++) {
//                    var triCount = reader.ReadInt32() * 3;
//                    var tris = new int[triCount];
//                    var material = new Material();

//                    reader.PrewarmBuffer(triCount * 4 + 1);

//                    for(int j = 0, v = 0; j < triCount; j += 3, v += 12) {
//                        tris[j + 0] = reader.ReadInt32();
//                        tris[j + 1] = reader.ReadInt32();
//                        tris[j + 2] = reader.ReadInt32();
//                    }

//                    mesh.SetTriangles(tris, i);

//                    material.Textured = reader.ReadBoolean();

//                    material.TextureName = reader.ReadString();
//                    material.MaskName = reader.ReadString();

//                    reader.PrewarmBuffer(28);

//                    material.Ambient = reader.ReadSingle();
//                    material.Diffuse = reader.ReadSingle();
//                    material.Specular = reader.ReadSingle();
//                    material.Color = new Color() {
//                        r = reader.ReadSingle(),
//                        g = reader.ReadSingle(),
//                        b = reader.ReadSingle(),
//                        a = reader.ReadSingle()
//                    };

//                    materials.Add(material);
//                }

//                mesh.name = result.Name;
//                result.Geometry = new Geometry(mesh, materials);
//            }

//            var childCount = reader.ReadInt32();

//            for(int i = 0; i < childCount; i++) {
//                result.Childs.Add(ReadFrame(reader));
//                result.Childs.GetLastValue().Parent = result;
//            }

//            return result;
//        }

//        private void WriteFrame(Frame frame, BinaryWriter writer) {
//            writer.Write(frame.Name);
//            writer.Write(frame.Position.x);
//            writer.Write(frame.Position.y);
//            writer.Write(frame.Position.z);
//            writer.Write(frame.Rotation.m00);
//            writer.Write(frame.Rotation.m01);
//            writer.Write(frame.Rotation.m02);
//            writer.Write(frame.Rotation.m10);
//            writer.Write(frame.Rotation.m11);
//            writer.Write(frame.Rotation.m12);
//            writer.Write(frame.Rotation.m20);
//            writer.Write(frame.Rotation.m21);
//            writer.Write(frame.Rotation.m22);
//            writer.Write(frame.Geometry != null);

//            if(frame.Geometry != null) {
//                var geometry = frame.Geometry.Mesh;
//                var vertices = geometry.vertices;
//                var normals = geometry.normals;
//                var uv = geometry.uv;
//                var uv2 = geometry.uv2;
//                var colors = geometry.colors;

//                writer.Write(vertices.Length);
//                writer.Write(geometry.subMeshCount);
//                writer.Write(uv.Length > 0);
//                writer.Write(uv2.Length > 0);
//                writer.Write(colors.Length > 0);

//                for(int i = 0; i < vertices.Length; i++) {
//                    writer.Write(vertices[i].x);
//                    writer.Write(vertices[i].y);
//                    writer.Write(vertices[i].z);
//                    writer.Write(normals[i].x);
//                    writer.Write(normals[i].y);
//                    writer.Write(normals[i].z);
//                }

//                if(uv.Length > 0)
//                    for(int i = 0; i < vertices.Length; i++) {
//                        writer.Write(uv[i].x);
//                        writer.Write(uv[i].y);
//                    }

//                if(uv2.Length > 0)
//                    for(int i = 0; i < vertices.Length; i++) {
//                        writer.Write(uv2[i].x);
//                        writer.Write(uv2[i].y);
//                    }

//                if(colors.Length > 0)
//                    for(int i = 0; i < vertices.Length; i++) {
//                        writer.Write(colors[i].r);
//                        writer.Write(colors[i].g);
//                        writer.Write(colors[i].b);
//                        writer.Write(colors[i].a);
//                    }

//                for(int j = 0; j < geometry.subMeshCount; j++) {
//                    var tris = geometry.GetTriangles(j);
//                    var material = frame.Geometry.Materials[j];

//                    writer.Write(tris.Length / 3);

//                    for(int i = 0; i < tris.Length; i++)
//                        writer.Write(tris[i]);

//                    writer.Write(material.Textured);
//                    writer.Write(material.TextureName);
//                    writer.Write(material.MaskName);
//                    writer.Write(material.Ambient);
//                    writer.Write(material.Diffuse);
//                    writer.Write(material.Specular);
//                    writer.Write(material.Color.r);
//                    writer.Write(material.Color.g);
//                    writer.Write(material.Color.b);
//                    writer.Write(material.Color.a);
//                }
//            }

//            writer.Write(frame.Childs.Count);

//            for(int i = 0; i < frame.Childs.Count; i++)
//                WriteFrame(frame.Childs[i], writer);
//        }

//        /* File format:
//        * 
//        * int32 - modelCount
//        * array[modelCount] - frames {
//        *   string - modelName
//        *   frame {
//        *     string - frameName
//        *     float x, y, z - position
//        *     float m00, m01, m02, m10, m11, m12, m20, 21, 22 - rotation
//        *     bool - hasGeometry
//        *     if (hasGeometry)
//        *       geometry {
//        *         int32 - vertexCount
//        *         int32 - submeshCount
//        *         bool - hasUv
//        *         bool - hasUv2
//        *         bool - hasColors
//        *         array [vertexCount] - positions {
//        *           float x, y, x - vertice 
//        *           float x, y, x - normal
//        *         }
//        *         if (hasUV)
//        *           array [vertexCount] - uv {
//        *             float x, y - uv
//        *           }
//        *         if (hasUV2)
//        *           array [vertexCount] - uv2 {
//        *             float x, y - uv2
//        *           }
//        *         if (hasColors)
//        *           array [vertexCount] - colors {
//        *             float r, g, b, a - color
//        *           }
//        *         array [submeshCount] - submeshTris {
//        *           int32 - triCount
//        *           array [triCount * 3] int32 - triangles
//        *         }
//        *         array[subMeshCount] - materials {
//        *           bool - textured
//        *           string - textureName
//        *           string - maskName
//        *           float - ambient
//        *           float - diffuse
//        *           float - specular
//        *           float r, g, b, a - color
//        *       }
//        *     }
//        *     int32 - childCount
//        *     array frame [childCount] - childs
//        *   }
//        * }
//        */
//    }
//}