namespace GrandTheftAuto.RenderWare {
    ///http://www.gtamodding.com/wiki/List_of_RW_section_IDs
    public enum SectionType {
        Undefined = 0x0000,
        Struct = 0x0001,
        /// <summary>
        /// Used to store an array of characters. It includes a trailing zero, and is padded with zeros to the next 4 byte boundary.
        /// </summary>
        String = 0x0002,
        /// <summary>
        /// Extension chunks are containers for plugin data. 
        /// The contents of this chunk are one chunk per plugin where the chunk ID is the same as the plugin ID.
        /// </summary>
        Extension = 0x0003,
        /// <summary>
        /// Section used in <see cref="Dff.DffFile"/> files as child of a <see cref="Material"/> or an <see cref="MaterialEffects"/> . 
        /// Does not store any data at all, all additional information get stored inside a struct section which directly follows this one as a child. 
        /// A texture section is used to store identifying information about a <see cref="Txd.Texture"/> and it's alpha layer.
        /// </summary>
        Texture = 0x0006,
        /// <summary>
        /// Structure that stores geometric data for <see cref="Geometry"/>.
        /// </summary>
        Material = 0x0007,
        /// <summary>
        /// Section used in <see cref="Dff.DffFile"/> as child of a <see cref="Geometry"/>. 
        /// The section itself does not store any data at all, all additional information get stored inside a struct section which directly follows this one as a child. 
        /// Material List stores the different materials used by the <see cref="Geometry"/> it is appended to.
        /// </summary>
        MaterialList = 0x0008,
        /// <summary>
        /// Section used in <see cref="Dff.DffFile"/> as child of a <see cref="Clump"/>. 
        /// Just like the <see cref="Clump"/> it does only store child sections and no data, all additional informations are hold by a <see cref="Struct"/>.
        /// </summary>
        FrameList = 0x000E,
        /// <summary>
        /// Structure that stores geometric data use by <see cref="Dff.Geometry"/> 
        /// </summary>
        Geometry = 0x000F,
        /// <summary>
        /// Container for a <see cref="Frame"/> hierarchy to which <see cref="Atomic"/> are attached.
        /// </summary>
        Clump = 0x0010,
        /// <summary>
        /// Container section used in <see cref="Dff.DffFile"/> as child of a <see cref="Clump"/>. 
        /// It is normally accompanied by a <see cref="Struct"/>. 
        /// An <see cref="Atomic"/> can associate a <see cref="Frame"/> with a <see cref="Geometry"/>.
        /// </summary>
        Atomic = 0x0014,
        /// <summary>
        /// Container section used in <see cref="Txd.TxdFile"/> as child of a <see cref="TextureDictionary"/> section. 
        /// It is normally accompanied by a <see cref="Struct"/>.
        /// </summary>
        TextureNative = 0x0015,
        /// <summary>
        /// Texture Dictionary is usually the root section of TXD files, thus it only contains child sections, no data. 
        /// It is normally accompanied by a <see cref="Struct"/>.
        /// </summary>
        TextureDictionary = 0x0016,
        /// <summary>
        /// Section used in <see cref="Dff.DffFile"/> as child of a <see cref="Clump"/>. 
        /// Just like the <see cref="Clump"/> it does only store child sections and no data, additional informations are hold by a <see cref="Struct"/>.
        /// </summary>
        GeometryList = 0x001A,
        /// <summary>
        /// Holds an optimized representation of the <see cref="Dff.Geometry"/> topology.
        /// </summary>
        BinMesh = 0x050E,
        /// <summary>
        /// Extension to the <see cref="Material"/>. Used to attach certain effects to a material, such as bump mapping, reflections and UV-Animations.
        /// </summary>
        MaterialEffects = 0x0120,
        /// <summary>
        /// Rockstar's custom section. In <see cref="GtaVersion.SanAndreas"/> it is used to store <see cref="Dff.Material"/> information for specular lighting.
        /// </summary>
        MaterialSpecular = 0x0253F2F6,
        /// <summary>
        /// Rockstar's custom section. In <see cref="GtaVersion.SanAndreas"/> it is used to override vehicle reflection maps.
        /// </summary>
        MaterialReflection = 0x0253F2FC,
        /// <summary>
        /// Rockstar's custom section. Is a child section of the <see cref="Extension"/> of the <see cref="FrameList"/> inside a <see cref="Dff.DffFile"/>.
        /// </summary>
        Frame = 0x0253F2FE,
        /// <summary>
        /// 2d Effect, formerly 2dfx, is one of Rockstar's custom sections. In GTA San Andreas it is used to store 2D effects, which were located in ide files in previous versions. There can be multiple effects per section, their types are defined by an ID.
        /// </summary>
        Effect2D = 0x0253F2F8,
    }
}