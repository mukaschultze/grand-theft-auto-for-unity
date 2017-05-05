using GrandTheftAuto.Diagnostics;
using UnityEngine;
using UnityTexture = UnityEngine.Texture;

namespace GrandTheftAuto.Txd.Decoding {
    public abstract class Decoder {

        protected int kernel;
        protected uint threadsX;
        protected uint threadsY;
        protected uint threadsZ;
        protected ComputeShader shader;

        public static PrefItem<bool> UseMipmaps = new PrefItem<bool>("GrandTheftAuto.Txd.Decoding.UseMipmaps", true);
        public static PrefItem<bool> GPUDecodingPref = new PrefItem<bool>("GrandTheftAuto.Txd.Decoding.GPUDecodingPref", SystemInfo.supportsComputeShaders);

        public static bool GPUDecoding {
            get { return GPUDecodingPref && SystemInfo.supportsComputeShaders; }
            set { GPUDecodingPref.Value = value; }
        }

        public static DXT3 DXT3 = new DXT3();
        public static DXT1 DXT1 = new DXT1();
        public static ColorBlock ColorBlock = new ColorBlock();
        public static Palette Palette = new Palette();

        public UnityTexture DecodeTexture(BufferReader reader, int width, int height, RasterFormat rasterFormat) {
            if(GPUDecoding)
                return DecodeTextureWithComputeShader(reader, width, height, rasterFormat);
            else
                return DecodeTextureWithProcessor(reader, width, height, rasterFormat);
        }

        public abstract UnityTexture DecodeTextureWithProcessor(BufferReader reader, int width, int height, RasterFormat rasterFormat);

        public abstract UnityTexture DecodeTextureWithComputeShader(BufferReader reader, int width, int height, RasterFormat rasterFormat);

        protected static TextureFormat GetFormat(RasterFormat format) {
            var result = TextureFormat.RGBA32;

            switch(format) {
                case RasterFormat.Format_555:
                case RasterFormat.Format_565:
                    result = TextureFormat.RGB565;
                    break;

                case RasterFormat.Format_1555:
                case RasterFormat.Format_8888:
                    result = TextureFormat.RGBA32;
                    break;

                case RasterFormat.Format_4444:
                    result = TextureFormat.RGBA4444;
                    break;

                case RasterFormat.Format_888:
                    result = TextureFormat.RGB24;
                    break;

                case RasterFormat.Format_LUM8:
                    result = TextureFormat.Alpha8;
                    break;

                default:
                    Log.Warning("Could not find a matching format for raster {0}", format);
                    break;
            }

            if(!SystemInfo.SupportsTextureFormat(result))
                Log.Warning("Texture format {0} is unsupported on current platform, using {1} instead", result, result = TextureFormat.ARGB32);

            return result;
        }

        protected static RenderTextureFormat GetRenderTextureFormat(RasterFormat format) {
            var result = RenderTextureFormat.Default;

            switch(format) {
                case RasterFormat.Format_555:
                case RasterFormat.Format_565:
                //result = RenderTextureFormat.RGB565;
                //break;

                case RasterFormat.Format_1555:
                //result = RenderTextureFormat.ARGB1555;
                //break;

                case RasterFormat.Format_4444:
                //result = RenderTextureFormat.ARGB4444;
                //break;

                case RasterFormat.Format_888:
                case RasterFormat.Format_8888:
                    result = RenderTextureFormat.ARGB32;
                    break;

                case RasterFormat.Format_LUM8:
                    result = RenderTextureFormat.R8;
                    break;

                default:
                    Log.Warning("Could not find a matching render format for raster {0}", format);
                    break;
            }

            if(!SystemInfo.SupportsRenderTextureFormat(result))
                Log.Warning("Render texture format {0} is unsupported on current platform, using {1} instead", result, result = RenderTextureFormat.Default);

            return result;
        }

        protected static Color32 SumColor(Color32 left, Color32 right) {
            left.r += right.r;
            left.g += right.g;
            left.b += right.b;
            left.a += right.a;
            return left;
        }

        protected static Color32 SubtractColor(Color32 left, Color32 right) {
            left.r -= right.r;
            left.g -= right.g;
            left.b -= right.b;
            left.a -= right.a;
            return left;
        }

        protected static Color32 MultiplyColor(Color32 right, float left) {
            right.r = (byte)(right.r * left);
            right.g = (byte)(right.g * left);
            right.b = (byte)(right.b * left);
            right.a = (byte)(right.a * left);
            return right;
        }

        protected static Texture2D GetTexture2D(int width, int height, RasterFormat rasterFormat) {
            return new Texture2D(width, height, GetFormat(rasterFormat), UseMipmaps);
        }

        protected static RenderTexture GetRenderTexture(int width, int height, RasterFormat rasterFormat) {
            var texture = new RenderTexture(width, height, 0, GetRenderTextureFormat(rasterFormat)) {
                enableRandomWrite = true,
                useMipMap = UseMipmaps,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 16,
                antiAliasing = 2,
                wrapMode = TextureWrapMode.Repeat
            };

            texture.Create();
            return texture;
        }

    }
}
