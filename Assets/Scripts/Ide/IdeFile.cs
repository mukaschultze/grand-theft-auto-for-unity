using System;
using System.Collections;
using System.Collections.Generic;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Ide.Fx;

///http://gta.wikia.com/wiki/IDE
///http://www.gtamodding.com/wiki/Item_Definition
namespace GrandTheftAuto.Ide {
    public sealed class IdeFile : TextFileParser, IEnumerable<ItemDefinition> {

        protected override char CommentChar { get { return '#'; } }
        protected override char EofChar { get { return '#'; } }

        public List<TextureParent> TextureParents { get; private set; }
        public Dictionary<int, ItemDefinition> Objects { get; private set; }

        private IdeSection currentSection = IdeSection.END;

        public IdeFile(string filePath) {
            using(new Timing("Loading IDE")) {
                FilePath = filePath;
                Objects = new Dictionary<int, ItemDefinition>();
                TextureParents = new List<TextureParent>();
                Load();
            }
        }

        protected override void ParseLine(string line) {
            if(line == "end") {
                currentSection = IdeSection.END;
                return;
            }

            var tokens = line.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            try {
                switch(currentSection) {
                    case IdeSection.END:
                        ProcessNewSectionStart(tokens[0]);
                        break;

                    case IdeSection.OBJS:
                        AddObject(new MapObject(tokens));
                        break;

                    case IdeSection.TOBJ:
                        AddObject(new TimeObject(tokens));
                        break;

                    case IdeSection.ANIM:
                        AddObject(new AnimatedObject(tokens));
                        break;

                    case IdeSection.CARS:
                        AddObject(new CarDefinition(tokens));
                        break;

                    case IdeSection._2DFX:
                        switch(int.Parse(tokens[8])) {
                            case 0:
                                AddEffectToTargetObject(new FxLight(tokens));
                                break;

                            case 1:
                                AddEffectToTargetObject(new FxParticleDefinition(tokens));
                                break;

                            case 2: //Ped investigate, unused
                            case 3: //Ped Behaviours
                            case 4: //Sunflares (Vice City only)
                                break;

                            default:
                                Log.Error("Invalid 2DFX entry: {0}", line);
                                break;
                        }
                        break;

                    case IdeSection.TXDP:
                        TextureParents.Add(new TextureParent(tokens));
                        break;
                }
            }
            catch(Exception e) {
                Log.Error("Failed to parse IDE line at \"{0}\", line \"{1}\", section {2}: {3}", FilePath, line, currentSection, e);
            }
        }

        private void AddObject(ItemDefinition obj) {
            Objects.Add(obj.ID, obj);
        }

        private void AddEffectToTargetObject(FxDefinition effect) {
            try {
                if(Objects[effect.TargetObjectID].Effects == null)
                    Objects[effect.TargetObjectID].Effects = new List<FxDefinition>();
                Objects[effect.TargetObjectID].Effects.Add(effect);
            }
            catch {
                Log.Error("Couldn't find target object ({0}) for 2DFX entry", effect.TargetObjectID);
            }
        }

        private void ProcessNewSectionStart(string line) {
            try {
                if(line == "2dfx")
                    currentSection = IdeSection._2DFX;
                else
                    currentSection = (IdeSection)Enum.Parse(typeof(IdeSection), line, true);
            }
            catch(Exception e) {
                Log.Warning("Unknow IDE section in \"{0}\", line \"{1}\": {2}", FilePath, line, e);
                currentSection = IdeSection.Unknow;
            }
        }

        IEnumerator<ItemDefinition> IEnumerable<ItemDefinition>.GetEnumerator() {
            return ((IEnumerable<ItemDefinition>)Objects.Values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return Objects.Values.GetEnumerator();
        }
    }
}
