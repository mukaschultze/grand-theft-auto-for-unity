using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Img;
using GrandTheftAuto.Renderwave;

namespace GrandTheftAuto.Txd {
    public class TxdFile : IEnumerable<Texture> {

        public string FileName { get { return file.FileName; } }
        public string FileNameWithoutExtension { get { return file.FileNameWithoutExtension; } }
        public short TextureCount { get { if(!loaded)Load(); return textureCount; } }
        public Texture this[int index] { get { if(!loaded)Load(); return textures.Values.ElementAt(index);; } }
        public Texture this[string name] { get { if(!loaded)Load(); return textures[name]; } }

        private bool ArchiveShouldBeIgnored { get { return FileNameWithoutExtension == "ISLANDLODCOMINDNT"; } }

        private bool loaded;
        private short textureCount;
        private short processedTextures;
        private FileEntry file;
        private Dictionary<string, Texture> textures;

        public TxdFile(string path) : this(new FileEntry(path)) { }

        public TxdFile(FileEntry file) { this.file = file; }

        private void Load() {
            using(Timing.Get("Loading TXD")) {
                loaded = true;
                textures = new Dictionary<string, Texture>(StringComparer.OrdinalIgnoreCase);

                if(ArchiveShouldBeIgnored) {
                    Log.Warning("Ignored file: {0}", file.FileName);
                    return;
                }

                textureCount = short.MaxValue;
                var reader = file.Reader;

                try { ParseSection(new SectionHeader(reader), reader); } catch(Exception e) { Log.Error("Failed to parse TXD section, on \"\"", e); }

                if(processedTextures != textureCount)
                    Log.Warning("Found {0} textures, expected {1}, on \"{2}\"", processedTextures, textureCount, file.FileName);
            }
        }

        private void ParseSection(SectionHeader parent, BufferReader reader) {
            var end = reader.Position + parent.Size;

            while(reader.Position < end && reader.Position < reader.Length && processedTextures < textureCount) {
                var header = new SectionHeader(reader);

                switch(header.Type) {
                    case SectionType.Struct:
                        switch(parent.Type) {
                            case SectionType.TextureNative:
                                var texture = new Texture(reader, header, this);

                                try {
                                    if(!string.IsNullOrEmpty(texture.Name))
                                        textures.Add(texture.Name, texture);
                                } catch { Log.Error("Failed to add texture {0} to {1}", texture.Name, FileName); }

                                try {
                                    if(!string.IsNullOrEmpty(texture.AlphaName))
                                        textures.Add(texture.AlphaName, texture);
                                } catch { Log.Error("Failed to add texture alpha {0} to {1}", texture.AlphaName, FileName); }

                                processedTextures++;
                                break;

                            case SectionType.TextureDictionary:
                                textureCount = reader.ReadInt16();
                                reader.SkipStream(2);
                                break;

                            default:
                                reader.SkipStream(header.Size);
                                Log.Message("Ignored renderwave section {0} in {1}", parent.Type, FileNameWithoutExtension);
                                break;
                        }
                        break;

                    case SectionType.Extension:
                    case SectionType.TextureDictionary:
                    case SectionType.TextureNative:
                        ParseSection(header, reader);
                        break;

                    default:
                        reader.SkipStream(header.Size);
                        Log.Message("Ignored renderwave section {0} in {1}", header.Type, FileNameWithoutExtension);
                        break;
                }

            }

        }

        public bool TryGetTexture(string name, out Texture texture) {
            if(!loaded)
                Load();
            return textures.TryGetValue(name, out texture);
        }

        IEnumerator<Texture> IEnumerable<Texture>.GetEnumerator() {
            if(!loaded)
                Load();
            return (from texture in textures where texture.Value.AlphaName != texture.Key select texture.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            if(!loaded)
                Load();
            return (from texture in textures where texture.Value.AlphaName != texture.Key select texture.Value).GetEnumerator();
        }
    }
}