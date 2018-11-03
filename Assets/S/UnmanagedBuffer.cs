using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GrandTheftAuto.New {
    public unsafe class UnmanagedBuffer : IDisposable {

        private bool disposed;
        private byte * bufferPtr;
        private GCHandle pinnedBuffer;

        public byte[] buffer;

        private Stream stream;
        private int position;
        private int remaining;
        private int length;

        public int StreamLength { get { return (int)stream.Length; } }

        public UnmanagedBuffer(string filePath) {
            var file = new FileEntry(filePath);
            this.stream = file.GetReadStream();
        }

        public UnmanagedBuffer(FileEntry file) {
            this.stream = file.GetReadStream();
        }

        public UnmanagedBuffer(Stream stream) {
            this.stream = stream;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateBuffer(int length) {
            buffer = new byte[length];
            pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            bufferPtr = (byte * )pinnedBuffer.AddrOfPinnedObject();

            position = 0;
            this.length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FreeBuffer() {
            if(pinnedBuffer.IsAllocated)
                pinnedBuffer.Free();

            buffer = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Reader GetReader(int length) {
            if(length > this.length) {
                FreeBuffer();
                CreateBuffer(length);
                ReadFromStream(length);
            }

            return new Reader(bufferPtr, length);
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public void PrewarmBuffer(int count) {
        //     EnsureLoaded(count);
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadFromStream(int count) {
            remaining = count;
            stream.Read(buffer, position, count);
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public void EnsureLoaded(int count) {
        //     if(length == 0) { // We don't have a buffer yet
        //         CreateBuffer(Math.Max(count, DEFAULT_BUFFER_SIZE));
        //         ReadFromStream(length);
        //     }

        //     if(remaining >= count) { return; } // Ok, we do have enought bytes loaded, nothing to do

        //     var needed = count - remaining;

        //     if(count > length) { // Our buffer isn't big enought to support the incoming data
        //         FreeBuffer();
        //         CreateBuffer(count); // Create a new buffer with enought space
        //         ReadFromStream(count);
        //     } else if(position + needed >= length) { // There's no space available in the buffer
        //         position = 0; // Go back to the beggining
        //         ReadFromStream(count);
        //     } else { // We have enought space, only load the needed bytes
        //         ReadFromStream(needed);
        //     }

        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() {
                FreeBuffer();
                stream.Dispose();
                disposed = true;
            }

            ~UnmanagedBuffer() { Dispose(); }

    }
}