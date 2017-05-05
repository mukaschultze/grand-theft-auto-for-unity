//using System;
//using System.IO;
//using System.Text;

//namespace GrandTheftAuto {
//    public class BufferReader : UnmanagedMemoryStream IDisposable {

//        private const int DEFAULT_BUFFER_SIZE = 64 * 1024;

//        public Stream BaseStream { get; private set; }
//        public long Position {
//            get { return BaseStream.Position - bufferLength + bufferPosition; }
//            set { BaseStream.Position = value; bufferPosition = bufferLength = 0; }
//        }
//        public long Length { get { return BaseStream.Length; } }

//        private byte[] buffer;
//        private int bufferPosition;
//        private int bufferLength;

//        public BufferReader(Stream stream) : this(stream, DEFAULT_BUFFER_SIZE) { }

//        public BufferReader(Stream stream, int initialBufferSize) {
//            BaseStream = stream;
//            buffer = new byte[initialBufferSize];
//        }

//        public void PrewarmBuffer(int count) {
//            //PB(count);
//        }

//        public void PB(int count) {
//            //Diagnostics.Log.Message("{0}, {1}, {2}, {3}", BaseStream.Position, Position, bufferPosition, bufferLength);
//            BaseStream.Position = Position;

//            //count = DEFAULT_BUFFER_SIZE;

//            if(count > buffer.Length)
//                buffer = new byte[count];

//            bufferPosition = 0;
//            bufferLength = BaseStream.Read(buffer, 0, count);
//        }

//        public void Skip(int count) {
//            bufferPosition += count;

//            if(bufferPosition < 0 || bufferPosition >= bufferLength)
//                bufferLength = 0;
//        }

//        public void SkipStream(int count) {
//            BaseStream.Seek(count, SeekOrigin.Current);
//            bufferPosition = 0;
//            bufferLength = 0;
//        }

//        public bool ReadBoolean() {
//            return ReadByte() != 0;
//        }

//        public byte ReadByte() {
//            if(bufferPosition + 1 >= bufferLength)
//                PB(1);

//            return buffer[bufferPosition++];
//        }

//        public byte[] ReadBytes(int count) {
//            if(count <= 0)
//                return new byte[0];

//            if(bufferPosition + count >= bufferLength)
//                PB(count);

//            var result = new byte[count];
//            Buffer.BlockCopy(buffer, bufferPosition, result, 0, count);
//            bufferPosition += count;
//            return result;
//        }

//        public char ReadChar() {
//            return (char)ReadByte();
//        }

//        public double ReadDouble() {
//            if(bufferPosition + 8 >= bufferLength)
//                PB(8);

//            var result = BitConverter.ToDouble(buffer, bufferPosition);
//            bufferPosition += 8;
//            return result;
//        }

//        public short ReadInt16() {
//            if(bufferPosition + 2 >= bufferLength)
//                PB(2);

//            var result = BitConverter.ToInt16(buffer, bufferPosition);
//            bufferPosition += 2;
//            return result;
//        }

//        public int ReadInt32() {
//            if(bufferPosition + 4 >= bufferLength)
//                PB(4);

//            var result = BitConverter.ToInt32(buffer, bufferPosition);
//            bufferPosition += 4;
//            return result;
//        }

//        public long ReadInt64() {
//            if(bufferPosition + 8 >= bufferLength)
//                PB(8);

//            var result = BitConverter.ToInt64(buffer, bufferPosition);
//            bufferPosition += 8;
//            return result;
//        }

//        public float ReadSingle() {
//            if(bufferPosition + 4 >= bufferLength)
//                PB(4);

//            var result = BitConverter.ToSingle(buffer, bufferPosition);
//            bufferPosition += 4;
//            return result;
//        }

//        public string ReadString() {
//            return ReadString(ReadByte());
//        }

//        public string ReadString(int size) {
//            if(size <= 0)
//                return string.Empty;

//            if(bufferPosition + size >= bufferLength)
//                PB(size);

//            var result = Encoding.ASCII.GetString(buffer, bufferPosition, size);
//            bufferPosition += size;
//            return result;
//        }

//        public ushort ReadUInt16() {
//            if(bufferPosition + 2 >= bufferLength)
//                PB(2);

//            var result = BitConverter.ToUInt16(buffer, bufferPosition);
//            bufferPosition += 2;
//            return result;
//        }

//        public uint ReadUInt32() {
//            if(bufferPosition + 4 >= bufferLength)
//                PB(4);

//            var result = BitConverter.ToUInt32(buffer, bufferPosition);
//            bufferPosition += 4;
//            return result;
//        }

//        public ulong ReadUInt64() {
//            if(bufferPosition + 8 >= bufferLength)
//                PB(8);

//            var result = BitConverter.ToUInt64(buffer, bufferPosition);
//            bufferPosition += 8;
//            return result;
//        }

//        public void Dispose() {
//            BaseStream.Dispose();
//        }
//    }
//}
