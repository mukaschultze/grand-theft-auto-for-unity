using System;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Renderwave;
using GrandTheftAuto.Shared;
using GrandTheftAuto.Txd.Decoding;
using UnityEngine;
using UnityTexture = UnityEngine.Texture;

namespace GrandTheftAuto.Txd {
    public class Texture {
        private const string FULLNAME_FORMAT = "{0}/{1}";

        public int Width { get; private set; }
        public int Height { get; private set; }

        public string Name { get; private set; }
        public string AlphaName { get; private set; }
        public string FullName { get; private set; }
        public string FullALphaName { get; private set; }

        public RasterFormat RasterFormat { get { return (RasterFormat)((int)rawRasterFormat % 0x1000); } }
        public RasterFormat RasterFormatEx { get { return (rawRasterFormat - (int)rawRasterFormat % 0x1000); } }
        public UnityTexture Texture2D { get { if(!loadedTexture)Load(); return loadedTexture; } }

        private int offset;
        private RasterFormat rawRasterFormat;
        private UnityTexture loadedTexture;
        private BufferReader reader;
        private static Texture missing;

        public Texture(BufferReader reader, SectionHeader header, TxdFile parent) {
            this.reader = reader;

            reader.SkipStream(8);
            reader.PrewarmBuffer(64);

            Name = reader.ReadBytes(32).GetNullTerminatedString();
            AlphaName = reader.ReadBytes(32).GetNullTerminatedString();

            FullName = string.Format(FULLNAME_FORMAT, parent.FileNameWithoutExtension, Name);
            FullALphaName = string.Format(FULLNAME_FORMAT, parent.FileNameWithoutExtension, AlphaName);

            reader.PrewarmBuffer(12);
            rawRasterFormat = (RasterFormat)reader.ReadInt32();
            reader.Skip(4); //Alpha or four CC

            Width = reader.ReadInt16();
            Height = reader.ReadInt16();

            reader.SkipStream(4); //BPP, Mipmaps, RasterType and DXTnumber

            offset = (int)reader.Position;
            reader.SkipStream(header.Size - 88);
        }

        public Texture(UnityTexture texture, string name, string alphaName, string parentName) {
            if(!texture)
                throw new ArgumentNullException("texture");

            Name = name;
            AlphaName = alphaName;
            FullName = parentName + name;
            FullALphaName = parentName + alphaName;
            loadedTexture = texture;
        }

        public static Texture GetMissingTexture() {
            if(missing == null)
                missing = new Texture(ResourcesHelper.MissingTexture, "Missing", "MissingAlpha", string.Empty);
            return missing;
        }

        private void Load() {
            using(Timing.Get("Loading Texture")) {
                var position = reader.Position;
                reader.Position = offset;

                try {
                    if(RasterFormatEx == RasterFormat.Extension_Palette8)
                        loadedTexture = Decoder.Palette.DecodeTexture(reader, Width, Height, RasterFormat);
                    else {
                        reader.SkipStream(4); //Data size

                        switch(RasterFormat) {
                            case RasterFormat.Format_565:
                            case RasterFormat.Format_1555:
                                loadedTexture = Decoder.DXT1.DecodeTexture(reader, Width, Height, RasterFormat);
                                break;

                            case RasterFormat.Format_4444:
                                loadedTexture = Decoder.DXT3.DecodeTexture(reader, Width, Height, RasterFormat);
                                break;

                            case RasterFormat.Format_888:
                            case RasterFormat.Format_8888:
                                loadedTexture = Decoder.ColorBlock.DecodeTexture(reader, Width, Height, RasterFormat);
                                break;

                            default:
                                Log.Error("Invalid raster format \"{0}\" on {1}", RasterFormat, FullName);
                                loadedTexture = missing;
                                return;
                        }
                    }

                    loadedTexture.name = Name;

                    // We do a lot of calculation to decode DXT
                    // And then we encode it back as DXT
                    // What a waste of time
                    // But if we don't do this it's problaby the application will run out of memory
                    // And fucking freeze the entire computer...
                    if(loadedTexture is Texture2D && Settings.Instance.compressTextures)
                        using(Timing.Get("Compressing Texture")) {
                            (loadedTexture as Texture2D).Compress(false);
                            (loadedTexture as Texture2D).Apply(false, true);
                        }
                } catch(Exception e) {
                    Log.Warning("Failed to load texture \"{0}\": {1}", FullName, e);
                    loadedTexture = missing;
                } finally {
                    reader.Position = position;
                    reader = null;
                }
            }
        }

        public override string ToString() {
            return string.Format("Name: {0}\nAlphaMask: {1}\nRasterFormat: {2}, extension: {3}\nSize: {4}x{5}", Name, AlphaName, RasterFormat, RasterFormatEx, Width, Height);
        }

        public static implicit operator UnityTexture(Texture tex) {
            return tex.Texture2D;
        }

    }
}