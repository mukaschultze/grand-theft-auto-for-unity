using System.Collections;
using System.Collections.Generic;
using GrandTheftAuto.Data;
using GrandTheftAuto.Diagnostics;

namespace GrandTheftAuto.Ide {
    public class DefinitionCollection : IEnumerable<ItemDefinition> {

        public Dictionary<int, ItemDefinition> definitionsIDS = new Dictionary<int, ItemDefinition>();
        public Dictionary<string, ItemDefinition> definitionsNames = new Dictionary<string, ItemDefinition>();

        public ItemDefinition this[int ID] { get { return definitionsIDS[ID]; } }
        public ItemDefinition this[string ID] { get { return definitionsNames[ID]; } }

        public void Add(DataFile data) {
            using(new Timing("Adding Definitions (data)"))
                foreach(var ide in data.IDEs)
                    Add(ide);
        }

        public void Add(IdeFile ide) {
            using(new Timing("Adding Definitions (ide)"))
                foreach(var def in ide)
                    Add(def);
        }

        public void Add(ItemDefinition def) {
            using(new Timing("Adding Definitions (definition)")) {
                definitionsIDS.Add(def.ID, def);
                definitionsNames.Add(def.DffName, def);
            }
        }

        public IEnumerator<ItemDefinition> GetEnumerator() {
            return definitionsIDS.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return definitionsIDS.Values.GetEnumerator();
        }
    }
}
