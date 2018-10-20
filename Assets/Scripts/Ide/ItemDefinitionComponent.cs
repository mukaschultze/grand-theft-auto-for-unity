using System;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Ide.Fx;
using UnityEngine;

namespace GrandTheftAuto.Ide {
    public class ItemDefinitionComponent : MonoBehaviour {

        [Serializable]
        private struct Property {
            public string name;
            public string value;
        }

        [SerializeField]
        private Property[] properties;

        public void RegisterDefinition(FxDefinition definition) {
            RegisterDefinition((object)definition);
        }

        public void RegisterDefinition(ItemDefinition definition) {
            RegisterDefinition((object)definition);
        }

        public void RegisterDefinition(object definition) {
            try {
                using(Timing.Get("Registering Definition Component")) {
                    var definitionProps = definition.GetType().GetProperties();

                    properties = new Property[definitionProps.Length + 1];
                    properties[0] = new Property() { name = "Type", value = definition.GetType().Name };

                    for(var i = 0; i < definitionProps.Length; i++) {
                        var ourProp = new Property();
                        var definitionProp = definitionProps[i];

                        ourProp.name = definitionProp.Name;

                        try { ourProp.value = definitionProp.GetValue(definition, null).ToString(); } catch { ourProp.value = "Ignored"; }

                        properties[i + 1] = ourProp;
                    }
                }
            } catch(Exception e) {
                Log.Error("Failed to register definition component: {0}", e);
            }
        }
    }
}