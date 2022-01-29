using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using GlobalEnums;
using Modding;
using UnityEngine;
using Newtonsoft.Json;

namespace Hat
{
    public class Setting{
        public string hat = "hat.png";
        public float offsetX  = 0f;
        public float offsetY = 1.1f;
        public float offsetZ = -0.5f;
        public bool enableWorld = true;
        public bool reduceChaos = true;
        public List<string> limitTo = new List<string>{
                                                        "npc",
                                                        "boss",
                                                        "shop",
                                                        "door_sly",
                                                        "Weaverling",
                                                        "Grimmchild"
                                                    };
        public bool verbose = false;
        public bool randomWorldHats = true;
        public Setting(){}
        public Setting(string hat,float offsetX,float offsetY,float offsetZ,bool enableWorld,bool randomWorldHats,bool reduceChaos,List<string> limitTo,bool verbose){
            this.hat = hat;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.offsetZ = offsetZ;

            this.enableWorld = enableWorld;
            this.randomWorldHats = randomWorldHats;
            this.reduceChaos = reduceChaos;
            this.limitTo = limitTo;
            this.verbose = verbose;
        }
    }
    
}