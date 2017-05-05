namespace GrandTheftAuto.Ide {
    public class TimeObject : MapObject {
        public int TimeOn { get; protected set; }
        public int TimeOff { get; protected set; }

        public TimeObject(string[] tokens) : base(tokens, false) {
            if(tokens.Length == 7) {
                ObjectCount = 1;
                DrawDistance = float.Parse(tokens[3]);
                Flags = (DefinitionFlags)int.Parse(tokens[4]);
            }
            else {
                ObjectCount = int.Parse(tokens[3]);
                DrawDistance = float.Parse(tokens[4]);
                Flags = (DefinitionFlags)int.Parse(tokens[3 + ObjectCount]);
            }

            TimeOn = int.Parse(tokens[4 + ObjectCount]);
            TimeOff = int.Parse(tokens[5 + ObjectCount]);
        }
    }
}