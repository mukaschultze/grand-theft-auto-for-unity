//using System;
//using System.IO;
//using System.Text;

//namespace GrandTheftAuto {
//    public class BufferReader : IDisposable {

//        private const int DEFAULT_BUFFER_SIZE = 15 * 1024;

//        public int BufferSize { get; }
//        private Stream BaseStream { get; }

//        public long Position {
//            get { return BaseStream.Position; }
//            set {
//                BaseStream.Position = value;

//                if(value < lastReadPosition || value >= lastReadPosition + lastBufferedSize)
//                    ReadBuffer();
//            }
//        }
//        public long Length {
//            get { return BaseStream.Length; }
//        }

//        private int bufferPosition {
//            get {
//                var result = (int)(BaseStream.Position - lastReadPosition);

//                if(result > BufferSize - 16) {
//                    ReadBuffer();
//                    return 0;
//                }

//                return result;
//            }
//        }
//        private int lastBufferedSize;
//        private long lastReadPosition;
//        private readonly byte[] buffer;

//        public BufferReader(Stream stream) : this(stream, DEFAULT_BUFFER_SIZE) { }

//        public BufferReader(Stream stream, int bufferSize) {
//            BufferSize = bufferSize;
//            BaseStream = stream;
//            buffer = new byte[bufferSize];
//            ReadBuffer();
//        }

//        public void ReadBuffer() {
//            lastBufferedSize = BufferSize;

//            if(BaseStream.Length - BaseStream.Position < BufferSize)
//                lastBufferedSize = (int)(BaseStream.Length - BaseStream.Position);

//            lastReadPosition = BaseStream.Position;
//            BaseStream.Read(buffer, 0, lastBufferedSize);
//            BaseStream.Position = lastReadPosition;
//        }

//        private void PrewarmBuffer(int count) {
//            lastBufferedSize = count;
//            BaseStream.Read(buffer, 0, count);
//        }

//        public void Skip(int count) {
//            BaseStream.Seek(count, SeekOrigin.Current);
//        }

//        public bool ReadBoolean() {
//            try {
//                return BitConverter.ToBoolean(buffer, bufferPosition);
//            }
//            finally {
//                Skip(1);
//            }
//        }

//        public byte ReadByte() {
//            try {
//                return buffer[bufferPosition];
//            }
//            finally {
//                Skip(1);
//            }
//        }

//        public byte[] ReadBytes(int count) {
//            var result = new byte[count];

//            if(count < lastBufferedSize - bufferPosition) {
//                Buffer.BlockCopy(buffer, bufferPosition, result, 0, count);
//                Skip(count);
//            }
//            else
//                BaseStream.Read(result, 0, count);

//            return result;
//        }

//        public char ReadChar() {
//            try {
//                return BitConverter.ToChar(buffer, bufferPosition);
//            }
//            finally {
//                Skip(1);
//            }
//        }

//        public double ReadDouble() {
//            try {
//                return BitConverter.ToDouble(buffer, bufferPosition);
//            }
//            finally {
//                Skip(8);
//            }
//        }

//        public short ReadInt16() {
//            try {
//                return BitConverter.ToInt16(buffer, bufferPosition);
//            }
//            finally {
//                Skip(2);
//            }
//        }

//        public int ReadInt32() {
//            try {
//                return BitConverter.ToInt32(buffer, bufferPosition);
//            }
//            finally {
//                Skip(4);
//            }
//        }

//        public long ReadInt64() {
//            try {
//                return BitConverter.ToInt64(buffer, bufferPosition);
//            }
//            finally {
//                Skip(8);
//            }
//        }

//        public float ReadSingle() {
//            try {
//                return BitConverter.ToSingle(buffer, bufferPosition);
//            }
//            finally {
//                Skip(4);
//            }
//        }

//        public string ReadString() {
//            var size = ReadByte();

//            if(size > 0)
//                try {
//                    ReadBuffer();
//                    return Encoding.ASCII.GetString(buffer, bufferPosition, size);
//                }
//                finally {
//                    Skip(size);
//                }
//            else
//                return string.Empty;
//        }

//        public ushort ReadUInt16() {
//            try {
//                return BitConverter.ToUInt16(buffer, bufferPosition);
//            }
//            finally {
//                Skip(2);
//            }
//        }

//        public uint ReadUInt32() {
//            try {
//                return BitConverter.ToUInt32(buffer, bufferPosition);
//            }
//            finally {
//                Skip(4);
//            }
//        }

//        public ulong ReadUInt64() {
//            try {
//                return BitConverter.ToUInt64(buffer, bufferPosition);
//            }
//            finally {
//                Skip(8);
//            }
//        }

//        public void Dispose() {
//            BaseStream.Dispose();
//        }
//    }
//}
