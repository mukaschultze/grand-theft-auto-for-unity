using GrandTheftAuto.Diagnostics;
using UnityEngine;

namespace GrandTheftAuto {
    public class Resource<T> where T : Object {
        private T loadedValue;

        public string Path { get; private set; }
        public T Value { get { return loadedValue ?? (loadedValue = Resources.Load<T>(Path)); } }

        public Resource(string path) {
            Path = path;
        }

        public static implicit operator Resource<T>(string path) {
            return new Resource<T>(path);
        }

        public static implicit operator T(Resource<T> resource) {
            return resource.Value;
        }
    }

    public static class ResourcesHelper {
        public static readonly Resource<LayerDebugColors> LayerDebugColors = "DebugColors";
        public static readonly Resource<Texture2D> MissingTexture = "Missing";
        public static readonly Resource<Material> WaterMaterial = "WaterSimple";

        public static readonly Resource<Shader> DiffuseShader = "Shaders/Diffuse";
        public static readonly Resource<Shader> StandardShader = "Shaders/Standard";

        public static readonly Resource<Material> BaseMaterial = "BaseMaterial";

        public static readonly Resource<ComputeShader> ColorPaletteDecoder = "ComputeShaders/PaletteDecoder";
        public static readonly Resource<ComputeShader> ColorBlockDecoder = "ComputeShaders/ColorBlockDecoder";
        public static readonly Resource<ComputeShader> DXT1Decoder = "ComputeShaders/DXT1Decoder";
        public static readonly Resource<ComputeShader> DXT3Decoder = "ComputeShaders/DXT3Decoder";
    }
}
