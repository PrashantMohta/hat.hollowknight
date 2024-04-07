using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using Modding.Converters;

namespace Hat
{
    public class Setting{
        public string Hat = Constants.DEFAULT_HAT;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Offset = new Vector3(0f, 0.5f, -0.5f); //new Vector3(0f, 1.1f, -0.5f);

        public bool EnableWorld = false;
        public bool ReduceChaos = true;
        public List<string> LimitTo = new List<string>{
                                                        "npc",
                                                        "boss",
                                                        "shop",
                                                        "door_sly",
                                                        "Weaverling",
                                                        "Grimmchild"
                                                    };
        public bool Verbose = false;
        public bool RandomWorldHats = true;
        public Setting(){}
    }
    
}