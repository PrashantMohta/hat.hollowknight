using System;
using System.IO;
using System.Reflection;
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
    public static class Utils {

        public static System.Random rand = new System.Random();
        public static Vector3 ZeroVector = new Vector3(0,0,0);
        public static void serialiseSetting(string path,Setting setting){
            var Json = JsonConvert.SerializeObject(setting, Formatting.Indented);
            File.WriteAllText(path,Json);
        }

        public static Setting deSerialiseSetting(string path){
            var Json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Setting>(Json, new JsonSerializerSettings() { ObjectCreationHandling = ObjectCreationHandling.Replace });
        }
        public static void SetScale(this GameObject gameObject,float scale){
            var localScale = gameObject.transform.localScale;
            localScale.x = Math.Abs(scale);
            localScale.y = Math.Abs(scale);
            gameObject.transform.localScale = localScale;
        }
        public static GameObject FindGameObjectInChildren( this GameObject gameObject, string name )
        {
            if( gameObject == null )
                return null;

            foreach( var t in gameObject.GetComponentsInChildren<Transform>( true ) )
            {
                if( t.name == name )
                    return t.gameObject;
            }
            return null;
        }

        public static void ExtractHatPng(string path){
            
            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (string res in asm.GetManifestResourceNames())
            {   
                if(!res.EndsWith("hat.png")) {
                    continue;
                } 
                using (Stream s = asm.GetManifestResourceStream(res))
                {
                        if (s == null) continue;
                        var buffer = new byte[s.Length];
                        s.Read(buffer, 0, buffer.Length);
                        File.WriteAllBytes(path,buffer);
                        s.Dispose();
                }
            }
        }
    }
    public class Hat : Mod
    {

        internal static Hat Instance;

        string currentDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),"Hats");

        Setting settings = new Setting();
        private BoxCollider2D heroCollider;
        private GameObject hat;

        private List<Texture2D> hats = new List<Texture2D>();



        
        public override string GetVersion()
        {
            return "1.0";
        }

        public override void Initialize()
        {
            Instance = this;
            if(!Directory.Exists(currentDirectory)){
                Directory.CreateDirectory(currentDirectory);
            }
            
            var settingsPath = Path.Combine(currentDirectory,"settings.json");
            if(File.Exists(settingsPath)){
                settings = Utils.deSerialiseSetting(settingsPath);
            }
            Utils.serialiseSetting(settingsPath,settings);


            var hatPath = Path.Combine(currentDirectory,"hat.png");
            if(!File.Exists(hatPath)) {
                Utils.ExtractHatPng(hatPath);
            }

            ModHooks.Instance.HeroUpdateHook += update;
        }


        public Texture2D LoadTexture(string name){
            Texture2D tex = new Texture2D(2, 2);
            try{
                byte[] texBytes = File.ReadAllBytes(Path.Combine(currentDirectory,name));            
                tex.LoadImage(texBytes);
                tex.Apply();
            } catch (Exception e){
                
            }
            return tex;
        }

        private void createHatOnParent(GameObject Parent,bool force){
            if(Parent.FindGameObjectInChildren(Parent.name + " hat") != null) {return;}
            Vector3 center = getParentColliderCenter(Parent);
            if(center == Utils.ZeroVector) {return;}
            if(Parent.GetComponent<HealthManager>() == null && Parent.LocateMyFSM("npc_control") == null && !force){return;}
            if(settings.verbose){
                Log("Adding Hat to" + Parent.name);
            }
            var hat = new GameObject(Parent.name + " hat");
            SpriteRenderer sr = hat.AddComponent<SpriteRenderer>();
            float scale = Parent.transform.localScale.x;
            Texture2D tex = hats[Utils.rand.Next(hats.Count)];
            sr.sprite = Sprite.Create(tex,new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f),64f);
            sr.color = new Color(1f, 1f, 1f, 1.0f);
            hat.SetActive(true);
            hat.SetScale(Parent.transform.localScale.y);
            hat.transform.position = Parent.transform.position + new Vector3(0,Parent.transform.localScale.y*1.1f,-0.0001f); //center + new Vector3(settings.offsetX,settings.offsetY,settings.offsetZ);
            hat.transform.SetParent(Parent.transform,true);
        }
        private void createHat(){
            hat = new GameObject("herohat");
            SpriteRenderer sr = hat.AddComponent<SpriteRenderer>();
            Texture2D tex =  LoadTexture(settings.hat);
            Vector3 center = getHeroColliderCenter();
            float scale = HeroController.instance.gameObject.transform.localScale.x;
            sr.sprite = Sprite.Create(tex,new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f),64f);
            sr.color = new Color(1f, 1f, 1f, 1.0f);
            hat.SetActive(true);
            hat.SetScale(HeroController.instance.gameObject.transform.localScale.y);
            hat.transform.position = center + new Vector3(settings.offsetX,settings.offsetY,settings.offsetZ);
            hat.transform.SetParent(HeroController.instance.gameObject.transform,true);
        }

        private Vector3 getParentColliderCenter(GameObject Parent){
            var collider = Parent.GetComponent<BoxCollider2D>();
            if(collider != null){
                return collider.bounds.center;
            } 
            return Utils.ZeroVector;
        }
        private Vector3 getHeroColliderCenter(){
            if(heroCollider == null){
                heroCollider = HeroController.instance.gameObject.GetComponent<BoxCollider2D>();
            }
            return heroCollider.bounds.center;
        }

        DateTime lastTime = DateTime.Now;

        public void loadhats(){
            if(!settings.enableWorld){ return;}
            if(settings.randomWorldHats){
                foreach(string hatpng in Directory.GetFiles(currentDirectory,"*.png")){
                    hats.Add(LoadTexture(hatpng));
                }
            } else {
               hats.Add(LoadTexture(settings.hat));
            }
        }
        public void updateHatPos(){
            if(settings.enableWorld){
                var currentTime = DateTime.Now;
                if ((currentTime - lastTime).TotalMilliseconds > 1000) {
                    foreach(GameObject gameObj in GameObject.FindObjectsOfType<GameObject>())
                    {   
                        if(settings.reduceChaos){
                            foreach(var name in settings.limitTo){
                                if(gameObj.name.ToLower().Contains(name.ToLower())){
                                    createHatOnParent(gameObj,true);
                                    continue;
                                }
                            }
                            createHatOnParent(gameObj,false);
                        } else {
                            createHatOnParent(gameObj,true);
                        }
                    }
                    
                }
            }
        }
        public void update()
        {
            if(HeroController.instance != null && HeroController.instance.gameObject!= null){
                if(hat == null) {
                    createHat();
                    loadhats(); // load for world mode
                }
                updateHatPos();
            }
        }

    }

}
