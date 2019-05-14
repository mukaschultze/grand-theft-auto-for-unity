using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Shared;
using UnityEngine;
using UnityTexture = UnityEngine.Texture;

namespace GrandTheftAuto.Txd.Decoding {
    public class Palette : Decoder {

        public Palette() {
            if (SystemInfo.supportsComputeShaders) {
                shader = ResourcesHelper.ColorPaletteDecoder.Value;
                kernel = shader.FindKernel("Decode");
                shader.GetKernelThreadGroupSizes(kernel, out threadsX, out threadsY, out threadsZ);
            }
        }

        public override UnityTexture DecodeTextureWithProcessor(BufferReader reader, int width, int height, RasterFormat rasterFormat) {
            using(new Timing("Palette Decoding")) {

                var texture = GetTexture2D(width, height, rasterFormat);
                var pixelCount = width * height;
                var colors = new Color32[pixelCount];
                var buffer = reader.ReadBytes(1024 + 4 + pixelCount); // 1024 bytes for palette, 4 bytes for data size

                for (var x = 0; x < width; x++)
                    for (var y = 0; y < height; y++) {
                        var palIndex = buffer[1028 + x + width * y] * 4;
                        var colorIndex = x + width * (height - y - 1); // Palette textures are iverted

                        colors[colorIndex] = new Color32() {
                            r = buffer[palIndex + 0],
                            g = buffer[palIndex + 1],
                            b = buffer[palIndex + 2],
                            a = buffer[palIndex + 3]
                        };
                    };

                texture.SetPixels32(colors);
                texture.Apply(UseMipmaps, false);

                return texture;
            }
        }

        public override UnityTexture DecodeTextureWithComputeShader(BufferReader reader, int width, int height, RasterFormat rasterFormat) {
            using(new Timing("Palette Decoding (Compute Shader)"))
            using(var paletteBuffer = new ComputeBuffer(256, 4))
            using(var indicesBuffer = new ComputeBuffer(width * height / 4, 4)) {
                var texture = GetRenderTexture(width, height, rasterFormat);

                paletteBuffer.SetData(reader.ReadBytes(1024));
                reader.SkipStream(4); //Data size
                indicesBuffer.SetData(reader.ReadBytes(width * height));
                shader.SetInt("Width", width);
                shader.SetInt("Height", height);
                shader.SetTexture(kernel, "Result", texture);
                shader.SetBuffer(kernel, "Palette", paletteBuffer);
                shader.SetBuffer(kernel, "Indices", indicesBuffer);
                shader.Dispatch(kernel, width / (int)threadsX, height / (int)threadsY, (int)threadsZ);

                if (UseMipmaps)
                    texture.GenerateMips();

                return texture;
            }
        }

    }
}