using System;
using System.Collections;
using System.Collections.Generic;
using GrandTheftAuto.Data;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Ide;
using GrandTheftAuto.Img;

namespace GrandTheftAuto.Txd {
    public class TextureCollection : IEnumerable<Texture> {

        private Dictionary<string, TxdFile> txds = new Dictionary<string, TxdFile>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, Texture> textures = new Dictionary<string, Texture>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<TxdFile, TxdFile> texturesParents = new Dictionary<TxdFile, TxdFile>();

        public bool TextureNameOnly { get; private set; }

        public Texture this[string textureName] {
            get {
                using(new Timing("Retrieving Texture")) {
                    var names = textureName.Split('/');

                    switch(names.Length) {
                        case 1:
                            Texture result;

                            if(textures.TryGetValue(textureName, out result))
                                return result;

                            Log.Warning("Texture not found \"{0}\"", textureName);
                            break;

                        case 2:
                            return this[names[0], names[1]];

                        default:
                            Log.Error("Invalid texture name: {0}", textureName);
                            break;
                    }

                    return Texture.GetMissingTexture();
                }
            }
        }

        public Texture this[string txdName, string textureName] {
            get {
                using(new Timing("Retrieving Texture")) {
                    TxdFile txd;
                    Texture result;

                    if(txds.TryGetValue(txdName, out txd))
                        if(txd.TryGetTexture(textureName, out result))
                            return result;
                        else if(texturesParents.TryGetValue(txd, out txd))
                            if(txd.TryGetTexture(textureName, out result))
                                return result;
                            else
                                Log.Warning("Texture \"{0}\" not found on \"{1}\" or any of its parents", textureName, txdName);
                        else
                            Log.Warning("Texture \"{0}\" not found on \"{1}\"", textureName, txdName);
                    else
                        Log.Warning("Txd not found \"{0}\"", txdName);

                    return Texture.GetMissingTexture();
                }
            }
        }

        public TextureCollection() { }

        public TextureCollection(bool textureNameOnly) { TextureNameOnly = textureNameOnly; }

        public void Add(DataFile data) {
            using(new Timing("Adding Textures (data)")) {
                foreach(var txd in data.TXDs)
                    Add(txd);
                foreach(var img in data.IMGs)
                    Add(img);
            }
        }

        public void Add(ImgFile img) {
            using(new Timing("Adding Textures (img)"))
                foreach(var entry in img)
                    if(entry.FileName.EndsWith(".txd", StringComparison.Ordinal))
                        Add(new TxdFile(entry));
        }

        public void Add(TxdFile txd) {
            using(new Timing("Adding Textures (txd)")) {
                try { txds.Add(txd.FileNameWithoutExtension, txd); }
                catch { Log.Error("A Txd with the same name already exist in this collection: {0}", txd.FileName); };

                if(TextureNameOnly)
                    foreach(var texture in txd) {
                        try { textures.Add(texture.Name, texture); }
                        catch { Log.Warning("A texture with the same name already exist in this collection: {0}", texture.FullName); }
                        try { textures.Add(texture.AlphaName, texture); }
                        catch { Log.Warning("A texture with the same alpha name already exist in this collection: {0}", texture.FullALphaName); }
                    }
            }
        }

        public void AddTextureParent(DataFile data) {
            foreach(var ide in data.IDEs)
                foreach(var textureParent in ide.TextureParents)
                    AddTextureParent(textureParent);
        }

        public void AddTextureParent(TextureParent textureParenting) {
            using(new Timing("Resolving Texture Parent")) {
                TxdFile texture;
                TxdFile parent;

                var hasTexture = txds.TryGetValue(textureParenting.TextureName, out texture);
                var hasParent = txds.TryGetValue(textureParenting.ParentName, out parent);

                if(hasTexture && hasParent)
                    try { texturesParents.Add(texture, parent); }
                    catch { Log.Warning("Couldn't resolve \"{0}.txd\" parenting, tried to set \"{1}.txd\" as parent but \"{2}.txd\" is already registered", textureParenting.TextureName, textureParenting.ParentName, texturesParents[texture].FileNameWithoutExtension); }
                else if(!hasTexture && !hasParent)
                    Log.Warning("Invalid texture parenting, texture and parent not found: \"{0}.txd\", parent \"{1}.txd\"", textureParenting.TextureName, textureParenting.ParentName);
                else if(!hasTexture)
                    Log.Warning("Invalid texture parenting, texture not found: \"{0}.txd\"", textureParenting.TextureName);
                else if(!hasParent)
                    Log.Warning("Invalid texture parenting, parent not found: \"{0}.txd\"", textureParenting.ParentName);

            }
        }

        IEnumerator<Texture> IEnumerable<Texture>.GetEnumerator() {
            return textures.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return textures.Values.GetEnumerator();
        }
    }
}
