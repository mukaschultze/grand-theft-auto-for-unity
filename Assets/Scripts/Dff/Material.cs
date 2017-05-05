using GrandTheftAuto.Diagnostics;
using UnityEngine;
using Texture = GrandTheftAuto.Txd.Texture;
using UnityMaterial = UnityEngine.Material;

namespace GrandTheftAuto.Dff {
    public struct Material {
        public bool Textured { get; set; }
        public string TextureName { get; set; }
        public string MaskName { get; set; }
        public float Ambient { get; set; }
        public float Diffuse { get; set; }
        public float Specular { get; set; }
        public Color Color { get; set; }

        private UnityMaterial loadedMaterial;

        private static int colorProp = Shader.PropertyToID("_Color");
        private static int ambientProp = Shader.PropertyToID("_Ambient");
        private static int diffuseProp = Shader.PropertyToID("_Diffuse");
        private static int specularProp = Shader.PropertyToID("_Specular");
        private static int mainTexProp = Shader.PropertyToID("_MainTex");
        private static int maskTexProp = Shader.PropertyToID("_MaskTex");

        public UnityMaterial GetUnityMaterial(string txdName) {
            if(loadedMaterial)
                return loadedMaterial;

            using(new Timing("Creating Unity Material")) {
                loadedMaterial = Object.Instantiate(ResourcesHelper.BaseMaterial.Value);
                loadedMaterial.name = string.Format("{0}/{1}", txdName, TextureName);

                if(Color != Color.white)
                    loadedMaterial.SetColor(colorProp, Color);
                if(Ambient < 1f)
                    loadedMaterial.SetFloat(ambientProp, Ambient);
                if(Diffuse < 1f)
                    loadedMaterial.SetFloat(diffuseProp, Diffuse);
                if(Specular < 1f)
                    loadedMaterial.SetFloat(specularProp, Specular);

                var texture = (Texture)null;
                var mask = (Texture)null;

                if(Textured) {
                    var txds = Loader.TxdCollection;

                    if(txds.TextureNameOnly) {
                        if(!string.IsNullOrEmpty(TextureName))
                            texture = txds[TextureName];
                        if(!string.IsNullOrEmpty(MaskName))
                            mask = txds[MaskName];
                    }
                    else {
                        if(!string.IsNullOrEmpty(TextureName))
                            texture = txds[txdName, TextureName];
                        if(!string.IsNullOrEmpty(MaskName))
                            mask = txds[txdName, MaskName];
                    }

                    if(mask == Texture.GetMissingTexture())
                        mask = texture;

                    loadedMaterial.SetTexture(mainTexProp, texture);
                    loadedMaterial.SetTexture(maskTexProp, mask ?? texture);
                }

                return loadedMaterial;
            }
        }
    }
}