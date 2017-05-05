namespace GrandTheftAuto.Ide.Fx {
    public class FxParticleDefinition : FxDefinition {
        public float Size { get; private set; }
        public ParticleType Particle { get; private set; }

        public FxParticleDefinition(string[] tokens) : base(tokens) {
            Particle = (ParticleType)int.Parse(tokens[9]);
            Size = float.Parse(tokens[13]);
        }
    }
}