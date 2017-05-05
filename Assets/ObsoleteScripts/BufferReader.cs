using System;
using System.IO;
using System.Text;

namespace GrandTheftAuto {
    public class BufferReader : IDisposable {

        public Stream BaseStream { get { return reader.BaseStream; } }
        public long Position {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
        }
        public long Length { get { return BaseStream.Length; } }

        private BinaryReader reader;

        public BufferReader(Stream stream) {
            reader = new BinaryReader(stream);
        }

        public void PrewarmBuffer(int count) {

        }

        public void Skip(int count) {
            Position += count;
        }

        public void Skip(long count) {
            Position += count;
        }

        public void SkipStream(int count) {
            Position += count;
        }

        public void SkipStream(long count) {
            Position += count;
        }

        public bool ReadBoolean() {
            return reader.ReadBoolean();
        }

        public byte ReadByte() {
            return reader.ReadByte();
        }

        public byte[] ReadBytes(int count) {
            return reader.ReadBytes(count);
        }

        public char ReadChar() {
            return reader.ReadChar();
        }

        public double ReadDouble() {
            return reader.ReadDouble();
        }

        public short ReadInt16() {
            return reader.ReadInt16();
        }

        public int ReadInt32() {
            return reader.ReadInt32();
        }

        public long ReadInt64() {
            return reader.ReadInt64();
        }

        public float ReadSingle() {
            return reader.ReadSingle();
        }

        public string ReadString() {
            return reader.ReadString();
        }

        public string ReadString(int size) {
            if(size <= 0)
                return string.Empty;

            return Encoding.ASCII.GetString(ReadBytes(size), 0, size);
        }

        public ushort ReadUInt16() {
            return reader.ReadUInt16();
        }

        public uint ReadUInt32() {
            return reader.ReadUInt32();
        }

        public ulong ReadUInt64() {
            return reader.ReadUInt64();
        }

        public void Dispose() {
            reader.Close();
        }
    }
}
