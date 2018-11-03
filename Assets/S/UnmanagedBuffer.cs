using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GrandTheftAuto {
    public unsafe class UnmanagedBuffer : IDisposable {

        private bool disposed;
        private byte[] buffer;
        private byte* dataPtr;
        private GCHandle pinnedBuffer;

        public UnmanagedBuffer(byte[] data) {
            buffer = data;
            pinnedBuffer = GCHandle.Alloc(data, GCHandleType.Pinned);
            dataPtr = (byte*)pinnedBuffer.AddrOfPinnedObject();
        }

        public UnmanagedBuffer(Stream stream) : this(stream, stream.Length) { }

        public UnmanagedBuffer(Stream stream, long length) {
            buffer = new byte[length];
            stream.Read(buffer, 0, (int)length);
            pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            dataPtr = (byte*)pinnedBuffer.AddrOfPinnedObject();
            buffer = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Skip(int count) {
            dataPtr += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte ReadByte() {
            dataPtr += sizeof(byte);
            return *(dataPtr - sizeof(byte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe char ReadChar() {
            dataPtr += sizeof(char);
            return (char)*(dataPtr - sizeof(char));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe double ReadDouble() {
            dataPtr += sizeof(double);
            return *(dataPtr - sizeof(double));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe short ReadInt16() {
            dataPtr += sizeof(short);
            return *(dataPtr - sizeof(short));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int ReadInt32() {
            dataPtr += sizeof(int);
            return *(dataPtr - sizeof(int));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe long ReadInt64() {
            dataPtr += sizeof(long);
            return *(dataPtr - sizeof(long));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe float ReadSingle() {
            dataPtr += sizeof(float);
            return *(dataPtr - sizeof(float));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe string ReadString() {
            var str = new string((char*)dataPtr);
            dataPtr += str.Length + 1;
            return str;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe string ReadString(int length) {
            var str = new string((char*)dataPtr, 0, length);
            dataPtr += length;
            return str;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ushort ReadUInt16() {
            dataPtr += sizeof(ushort);
            return *(dataPtr - sizeof(ushort));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe uint ReadUInt32() {
            dataPtr += sizeof(uint);
            return *(dataPtr - sizeof(uint));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ulong ReadUInt64() {
            dataPtr += sizeof(ulong);
            return *(dataPtr - sizeof(ulong));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T ReadStruct<T>() {
            var s = Marshal.PtrToStructure<T>((IntPtr)dataPtr);
            dataPtr += Marshal.SizeOf<T>();
            return s;
        }

        public void Dispose() {
            if(!disposed && pinnedBuffer.IsAllocated)
                pinnedBuffer.Free();

            buffer = null;
            disposed = true;
        }

        ~UnmanagedBuffer() { Dispose(); }

    }
}