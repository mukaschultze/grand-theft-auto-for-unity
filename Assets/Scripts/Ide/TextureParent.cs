namespace GrandTheftAuto.Ide {
    public class TextureParent {
        public string TextureName { get; set; }
        public string ParentName { get; set; }

        public TextureParent(string[] tokens) {
            TextureName = tokens[0];
            ParentName = tokens[1];
        }
    }
}