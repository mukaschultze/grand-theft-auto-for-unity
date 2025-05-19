namespace GrandTheftAuto.RenderWare {
    public struct SectionHeader {
        public int Size { get; private set; }
        public SectionType Type { get; private set; }
        public RenderWareVersion Version { get; private set; }

        public SectionHeader(BufferReader reader) {
            reader.PrewarmBuffer(12);
            Type = (SectionType)reader.ReadInt32();
            Size = reader.ReadInt32();
            reader.Skip(2);
            Version = (RenderWareVersion)reader.ReadInt16();
        }
    }
}