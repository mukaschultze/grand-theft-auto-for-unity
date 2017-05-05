using GrandTheftAuto.Diagnostics;
using UnityEngine;
using UnityTexture = UnityEngine.Texture;

namespace GrandTheftAuto.Txd.Decoding {
    public class Palette : Decoder {

        public Palette() {
            if(SystemInfo.supportsComputeShaders) {
                shader = ResourcesHelper.ColorPaletteDecoder.Value;
                kernel = shader.FindKernel("Decode");
                shader.GetKernelThreadGroupSizes(kernel, out threadsX, out threadsY, out threadsZ);
            }
        }

        public override UnityTexture DecodeTextureWithProcessor(BufferReader reader, int width, int height, RasterFormat rasterFormat) {
            using(new Timing("Palette Decoding")) {
                var texture = GetTexture2D(width, height, rasterFormat);
                var pixelCount = width * height;
                var palette = new Color32[256];
                var colors = new Color32[pixelCount];
                var buffer = reader.ReadBytes(1024);

                for(var i = 0; i < 256; i++)
                    palette[i] = new Color32() {
                        r = buffer[i * 4 + 0],
                        g = buffer[i * 4 + 1],
                        b = buffer[i * 4 + 2],
                        a = buffer[i * 4 + 3],
                    };

                reader.SkipStream(4); //Data size
                buffer = reader.ReadBytes(pixelCount);

                for(var x = 0; x < width; x++)
                    for(var y = 0; y < height; y++)
                        colors[x + width * (height - y - 1)] = palette[buffer[x + width * y]];

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

                if(UseMipmaps)
                    texture.GenerateMips();

                return texture;
            }
        }

    }
}
