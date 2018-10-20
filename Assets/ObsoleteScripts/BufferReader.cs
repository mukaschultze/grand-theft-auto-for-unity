using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using GrandTheftAuto.Diagnostics;

namespace GrandTheftAuto {
    public class BufferReader : IDisposable {

        public Stream BaseStream { get { return reader.BaseStream; } }

        public long Position {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return BaseStream.Position; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { BaseStream.Position = value; }
        }

        public long Length {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return BaseStream.Length; }
        }

        private BinaryReader reader;

        public BufferReader(Stream stream) {
            reader = new BinaryReader(stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrewarmBuffer(int count) {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Skip(int count) {
            Position += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Skip(long count) {
            Position += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SkipStream(int count) {
            Position += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SkipStream(long count) {
            Position += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBoolean() {
            return reader.ReadBoolean();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte() {
            return reader.ReadByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadBytes(int count) {
            return reader.ReadBytes(count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ReadChar() {
            return reader.ReadChar();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble() {
            return reader.ReadDouble();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16() {
            return reader.ReadInt16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32() {
            return reader.ReadInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64() {
            return reader.ReadInt64();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadSingle() {
            return reader.ReadSingle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString() {
            return reader.ReadString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString(int size) {
            if(size <= 0)
                return string.Empty;

            return Encoding.ASCII.GetString(ReadBytes(size), 0, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16() {
            return reader.ReadUInt16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32() {
            return reader.ReadUInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64() {
            return reader.ReadUInt64();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() {
            reader.Close();
        }

    }
}