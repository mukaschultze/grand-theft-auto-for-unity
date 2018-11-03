using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public unsafe struct Reader {

    private byte* bufferPtr;
    private int position;
    private int remaining;
    private int length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Reader(byte* bufferPtr, int length) {
        this.bufferPtr = bufferPtr;
        position = 0;
        remaining = length;
        this.length = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Reader Split(int index, int count) {
        return new Reader(bufferPtr + (length / count) * index, length / count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte* Read(int count) {
        remaining -= count;
        position += count;
        return bufferPtr + position - count;
    }

    #region Reading
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Skip(int count) {
        Read(count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte() {
        return *(byte*)Read(sizeof(byte));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char ReadChar() {
        return *(char*)Read(sizeof(char));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble() {
        return *(double*)Read(sizeof(double));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadInt16() {
        return *(short*)Read(sizeof(short));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt32() {
        return *(int*)Read(sizeof(int));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadInt64() {
        return *(long*)Read(sizeof(long));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadSingle() {
        return *(float*)Read(sizeof(float));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString() {
        var ptr = Read(0);
        var length = 0;

        for(length = 0; ; length++)
            if(*(ptr + length) == '\0')
                break;

        return Encoding.ASCII.GetString(Read(length), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString(int length) {
        var ptr = Read(length);

        for(int i = 0; i < length; i++)
            if(*(ptr + i) == '\0') {
                length = i;
                break;
            }

        return Encoding.ASCII.GetString(ptr, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadUInt16() {
        return *(ushort*)Read(sizeof(ushort));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt32() {
        return *(uint*)Read(sizeof(uint));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ReadUInt64() {
        return *(ulong*)Read(sizeof(ulong));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ReadStruct<T>() where T : struct {
        return Marshal.PtrToStructure<T>((IntPtr)Read(Marshal.SizeOf<T>()));
    }
    #endregion

}