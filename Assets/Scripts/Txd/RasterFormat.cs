using System;

namespace GrandTheftAuto.Txd {
    [Flags]
    public enum RasterFormat {
        /// <summary>
        /// 5 bits RGB, 1 bit alpha.
        /// </summary>
        Format_1555 = 0x0100,
        /// <summary>
        /// 5 bits R, 6 bits G, 5 bits B.
        /// </summary>
        Format_565 = 0x0200,
        /// <summary>
        /// 4 bits RGBA.
        /// </summary>
        Format_4444 = 0x0300,
        /// <summary>
        /// 8 bits alpha.
        /// </summary>
        Format_LUM8 = 0x0400,
        /// <summary>
        /// 8 bits BGRA.
        /// </summary>
        Format_8888 = 0x0500,
        /// <summary>
        /// 8 bits BGR, no alpha.
        /// </summary>
        Format_888 = 0x0600,
        /// <summary>
        /// 5 bits RGB.
        /// </summary>
        Format_555 = 0x0A00,

        /// <summary>
        /// The mipmaps should be automatically generated.
        /// </summary>
        Extension_AutoMipmap = 0x1000,
        /// <summary>
        /// <see cref="Texture"/> contains a 8 bit palette of 256 colors.
        /// </summary>
        Extension_Palette8 = 0x2000,
        /// <summary>
        /// <see cref="Texture"/> contains a 4 bit palette of 256 colors.
        /// </summary>
        Extension_Palette4 = 0x4000,
        /// <summary>
        /// <see cref="Texture"/> contains mipmaps.
        /// </summary>
        Extension_Mipmap = 0x8000
    }
}