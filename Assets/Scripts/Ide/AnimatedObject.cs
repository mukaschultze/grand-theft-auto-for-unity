namespace GrandTheftAuto.Ide {
    public class AnimatedObject : MapObject {
        public string AnimationName { get; protected set; }

        public AnimatedObject(string[] tokens) : base(tokens, false) {
            AnimationName = tokens[3];
            DrawDistance = float.Parse(tokens[4]);
            Flags = (DefinitionFlags)int.Parse(tokens[5]);
        }
    }
}