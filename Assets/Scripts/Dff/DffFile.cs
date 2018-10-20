using System;
using System.Collections.Generic;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Img;
using GrandTheftAuto.Renderwave;
using UnityEngine;

namespace GrandTheftAuto.Dff {
    ///http://www.gtamodding.com/wiki/RenderWare_binary_stream_file"
    ///http://www.chronetal.co.uk/gta/index.php?page=dff"
    public class DffFile {

        public string FileName { get { return file.FileName; } }
        public string FileNameWithoutExtension { get { return file.FileNameWithoutExtension; } }

        public Frame RootFrame {
            get {
                if(!loaded)
                    Load();

                try {
                    return frames[rootFrameIndex];
                } catch(Exception e) {
                    Log.Error("Failed to get root frame on \"{0}\": {1}", FileName, e);
                    return new Frame() { Name = "Error" };
                }
            }
        }
        public Frame[] Frames { get { if(!loaded)Load(); return frames; } }

        private bool loaded;
        private bool isAlphaMaskString;
        private int rootFrameIndex = -1;
        private int currentFrameName;
        private Frame[] frames;
        private BufferReader reader;
        private FileEntry file;
        private List<Geometry> geometries;

        public DffFile(string filePath) : this(new FileEntry(filePath)) { }

        public DffFile(FileEntry file) { this.file = file; }

        private void Load() {
            using(Timing.Get("Loading DFF")) {
                try {
                    reader = file.Reader;
                    geometries = new List<Geometry>();
                    ProcessSection(new SectionHeader(reader));
                } catch(Exception e) {
                    Log.Warning("Error loading {0}", FileName);
                    Log.Exception(e);
                } finally {
                    loaded = true;
                    geometries = null;
                    reader = null;
                }
            }
        }

        private void ProcessSection(SectionHeader parent) {
            var end = reader.Position + parent.Size;

            while(reader.Position < end) {
                var header = new SectionHeader(reader);

                switch(header.Type) {
                    case SectionType.Extension:
                    case SectionType.Texture:
                    case SectionType.Material:
                    case SectionType.MaterialList:
                    case SectionType.FrameList:
                    case SectionType.Geometry:
                    case SectionType.Clump:
                    case SectionType.Atomic:
                    case SectionType.GeometryList:
                        ProcessSection(header);
                        break;

                    case SectionType.BinMesh:
                        ParseBinaryMesh(header.Size);
                        break;

                    case SectionType.Frame:
                        ParseFrame(header.Size);
                        break;

                    case SectionType.Struct:
                        ParseStruct(header, parent.Type);
                        break;

                    case SectionType.String:
                        ParseStringSection(header.Size, parent.Type);
                        break;

                    case SectionType.MaterialSpecular:
                        ParseMaterialSpecular();
                        break;

                    case SectionType.MaterialReflection:
                        ParseMaterialReflection();
                        break;

                    default:
                        reader.SkipStream(header.Size);
                        break;
                }
            }
        }

        private void ParseStruct(SectionHeader header, SectionType parent) {
            switch(parent) {
                case SectionType.Geometry:
                    ParseGeometry(header);
                    break;

                case SectionType.Material:
                    ParseMaterial();
                    break;

                case SectionType.FrameList:
                    ParseFramesList();
                    break;

                case SectionType.Atomic:
                    ParseAtomic();
                    break;

                default:
                    reader.SkipStream(header.Size);
                    break;
            }
        }

        private void ParseAtomic() {
            reader.PrewarmBuffer(8);
            var frameIndex = reader.ReadInt32();
            var geometryIndex = reader.ReadInt32();
            frames[frameIndex].Geometry = geometries[geometryIndex];
            reader.SkipStream(8);
        }

        private void ParseGeometry(SectionHeader header) {
            var geometry = new Geometry(reader, header.Version);
            geometries.Add(geometry);
            reader.SkipStream(header.Size);
        }

        private void ParseFrame(int size) {
            var frame = frames[currentFrameName++];
            var bytes = reader.ReadBytes(size);
            var name = bytes.GetNullTerminatedString();
            frame.Name = name;
        }

        private void ParseBinaryMesh(int size) {
            var geometry = geometries.GetLastValue();
            geometry.SetMaterialSplitData(reader);
            reader.SkipStream(size);
        }

        private void ParseFramesList() {
            var framesCount = reader.ReadInt32();
            var matrix = Matrix4x4.identity;
            var position = new Vector3();
            var frameParentIndex = -1;

            frames = new Frame[framesCount];
            reader.PrewarmBuffer(framesCount * 56);

            for(var i = 0; i < framesCount; i++) {
                var frame = new Frame();

                //Doing this multiple times is faster than creating a new matrix and looping through all its values
                matrix.m00 = reader.ReadSingle();
                matrix.m10 = reader.ReadSingle();
                matrix.m20 = reader.ReadSingle();
                matrix.m01 = reader.ReadSingle();
                matrix.m11 = reader.ReadSingle();
                matrix.m21 = reader.ReadSingle();
                matrix.m02 = reader.ReadSingle();
                matrix.m12 = reader.ReadSingle();
                matrix.m22 = reader.ReadSingle();

                position.x = reader.ReadSingle();
                position.z = reader.ReadSingle();
                position.y = reader.ReadSingle();

                frame.Rotation = matrix;
                frame.Position = position;

                frameParentIndex = reader.ReadInt32();

                if(frameParentIndex != -1) {
                    frame.Parent = frames[frameParentIndex];
                    frame.Parent.Childs.Add(frame);
                } else if(rootFrameIndex == -1)
                    rootFrameIndex = i;
                else
                    Log.Error("More than one root frame in {0}", FileName);

                frames[i] = frame;
                reader.Skip(4);
            }
        }

        private void ParseMaterial() {
            var material = new Material();

            reader.SkipStream(4);
            reader.PrewarmBuffer(24);

            var color = reader.ReadBytes(4);

            material.Color = new Color32(color[0], color[1], color[2], color[3]);
            reader.Skip(4);
            material.TextureName = string.Empty;
            material.MaskName = string.Empty;
            material.Textured = reader.ReadInt32() != 0;
            material.Ambient = reader.ReadSingle();
            material.Specular = reader.ReadSingle();
            material.Diffuse = reader.ReadSingle();

            geometries.GetLastValue().Materials.Add(material);
        }

        private void ParseMaterialReflection() {
            reader.SkipStream(16);

            var mesh = geometries.GetLastValue();
            var material = mesh.Materials.GetLastValue();

            material.Diffuse = reader.ReadSingle();
            mesh.Materials.SetLastValue(material);

            reader.Skip(4);
        }

        private void ParseMaterialSpecular() {
            var mesh = geometries.GetLastValue();
            var material = mesh.Materials.GetLastValue();

            material.Specular = reader.ReadSingle();
            mesh.Materials.SetLastValue(material);

            reader.SkipStream(24);
        }

        private void ParseStringSection(int sectionSize, SectionType parentType) {
            if(parentType != SectionType.Texture) {
                reader.SkipStream(sectionSize);
                return;
            }

            var data = reader.ReadBytes(sectionSize);
            var mesh = geometries.GetLastValue();
            var material = mesh.Materials.GetLastValue();

            if(isAlphaMaskString)
                material.MaskName = data.GetNullTerminatedString();
            else
                material.TextureName = data.GetNullTerminatedString();

            isAlphaMaskString = !isAlphaMaskString;
            mesh.Materials.SetLastValue(material);
        }
    }
}