using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using GlobalEnums;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

using Newtonsoft.Json;
using Satchel;
using static Hat.Utils;
using Hat.Hkmp;
using Hkmp.Api.Server;

namespace Hat
{
    public class HatState : MonoBehaviour{
        public bool hasHat = false;
    }
    public class Hat : Mod
    {
        internal static Hat Instance;
        string currentDirectory = Path.Combine(AssemblyUtils.getCurrentDirectory(),"Hats");
        public Setting settings = new Setting();
        private GameObject hat;
        
        private List<Texture2D> hats = new List<Texture2D>();
        private Dictionary<int,Sprite> hatSprite = new Dictionary<int,Sprite>();

        internal HatMultiplayerClient MpClientInstance;
        internal HatMultiplayerServer MpServerInstance;

        public override string GetVersion()
        {
            return AssemblyUtils.GetAssemblyVersionHash(Constants.VERSION);
        }

        public override void Initialize()
        {
            Instance = this;
            IoUtils.EnsureDirectory(currentDirectory);
            
            var settingsPath = Path.Combine(currentDirectory,"settings.json");
            if(File.Exists(settingsPath)){
                settings = deSerialiseSetting(settingsPath);
            }
            serialiseSetting(settingsPath,settings);


            var hatPath = Path.Combine(currentDirectory,"hat.png");
            if(!File.Exists(hatPath)) {
                ExtractHatPng(hatPath);
            }

            loadhats(); // load for world mode
            preCreateHat(); // pre-create hat
            On.HeroController.Start += HeroControllerStart;
            if(ModHooks.GetMod("HkmpPouch") is Mod)
            {
                if (MpClientInstance == null)
                {
                    MpClientInstance = new HatMultiplayerClient();
                }
                if (MpServerInstance == null)
                {
                    MpServerInstance = new HatMultiplayerServer();
                    ServerAddon.RegisterAddon(MpServerInstance);
                }
            }
        }


        private void createHatOnParent(GameObject Parent,bool force){
            try{
                var HatState = Parent.GetAddComponent<HatState>();
                if(!force && !HatState.hasHat && Parent.GetComponent<HealthManager>() == null && Parent.LocateMyFSM("npc_control") == null ){
                    HatState.hasHat = false;
                    return;
                }
                if(HatState.hasHat || Parent.FindGameObjectInChildren(Parent.name + " hat") != null) {
                    return;
                }
                HatState.hasHat = true;
                Vector3 center = getParentColliderCenter(Parent);
                if(center == ZeroVector) {return;}
                if(settings.verbose){
                    Log("Adding Hat to" + Parent.name);
                }
                var hat = new GameObject(Parent.name + " hat");
                SpriteRenderer sr = hat.AddComponent<SpriteRenderer>();
                float scale = Parent.transform.localScale.x;
                var index = rand.Next(hats.Count);
                Sprite spr;
                if(hatSprite.TryGetValue(index,out spr)){
                    sr.sprite = spr;
                } else {
                    Texture2D tex = hats[index];
                    sr.sprite = Sprite.Create(tex,new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f),64f,0,SpriteMeshType.FullRect);
                    hatSprite[index] = sr.sprite;
                }
                sr.color = new Color(1f, 1f, 1f, 1.0f);
                hat.SetActive(true);
                hat.SetScale(Parent.transform.localScale.y);
                hat.transform.position = Parent.transform.position + new Vector3(0,Parent.transform.localScale.y*1.1f,-0.0001f); //center + new Vector3(settings.offsetX,settings.offsetY,settings.offsetZ);
                hat.transform.SetParent(Parent.transform,true);
                
            } catch (Exception e){
                Log(e.ToString());
            }
        }
        public void changeLocalHat(Texture2D tex)
        {
            SpriteRenderer sr = hat.GetAddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 64f, 0, SpriteMeshType.FullRect);
        }

        public void resetLocalHat()
        {
            Texture2D tex = LoadTexture(currentDirectory, settings.hat);
            changeLocalHat(tex);
        }


        private void preCreateHat(){
            hat = new GameObject("herohat");
            GameObject.DontDestroyOnLoad(hat);
            SpriteRenderer sr = hat.AddComponent<SpriteRenderer>();
            Texture2D tex =  LoadTexture(currentDirectory,settings.hat);
            sr.sprite = Sprite.Create(tex,new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f),64f,0,SpriteMeshType.FullRect);
            sr.color = new Color(1f, 1f, 1f, 1.0f);
            hat.SetActive(false);
        }
        private void attachHat(){
            hat.SetActive(true);
            hat.SetScale(HeroController.instance.gameObject.transform.localScale.y);
            hat.transform.position = HeroController.instance.gameObject.transform.position + new Vector3(settings.offsetX,settings.offsetY,settings.offsetZ);
            hat.transform.SetParent(HeroController.instance.gameObject.transform,true);
        }

        DateTime lastTime = DateTime.Now;
        bool pending = false;
        public void loadhats(){
            if(!settings.enableWorld){ return;}
            if(settings.randomWorldHats){
                foreach(string hatpng in Directory.GetFiles(currentDirectory,"*.png")){
                    hats.Add(LoadTexture(currentDirectory,hatpng));
                }
            } else {
               hats.Add(LoadTexture(currentDirectory,settings.hat));
            }
        }

        internal void createHatConditionally(GameObject gameObj){
            if(gameObj.GetComponent<HatState>() != null) { return; }
            if(settings.reduceChaos){
                var goName = gameObj.name.ToLower();
                foreach(var name in settings.limitTo){
                    if(goName.Contains(name.ToLower())){
                        createHatOnParent(gameObj,true);
                        break;
                    }
                }
                createHatOnParent(gameObj,false);
            } else {
                createHatOnParent(gameObj,true);
            }
        }
        internal IEnumerator addHats( float delay = 0){
            if(!settings.enableWorld){
                pending = false;
                yield break; 
            }
            var currentTime = DateTime.Now;
            if ((currentTime - lastTime).TotalMilliseconds <= 1000) {
                pending = false;
                yield break; 
            }
            if(delay > 0){
                yield return new WaitForSeconds(delay);
            }
            var GOList = GameObject.FindObjectsOfType<GameObject>();
            foreach(var gameObj in GOList){
                if(gameObj == null) { continue; }
                createHatConditionally(gameObj);
            }

            lastTime = DateTime.Now;
            pending = false;
        }

        public void HeroControllerStart(On.HeroController.orig_Start orig,HeroController self){
            orig(self);
            attachHat();
            ModHooks.HeroUpdateHook += update;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += activeSceneChanged;
            On.HutongGames.PlayMaker.Actions.ActivateGameObject.DoActivateGameObject += objectActivated;
        }

        public void ManageHatCoroutine(bool restart = false){
            if(restart){
                if(hatCoro != null){
                    GameManager.instance.StopCoroutine(hatCoro);
                }
                lastTime = DateTime.Now.AddSeconds(-20000); //force next update to trigger
                pending = false;
            }
            if(!pending){
                pending = true;
                hatCoro = GameManager.instance.StartCoroutine(addHats(restart ? 0.01f : 0f));
            }
        }
        public void activeSceneChanged(Scene from, Scene to){
            if(!GameManager.instance.IsGameplayScene()) { return; } 
            ManageHatCoroutine(true);
        }
        internal void objectActivated(On.HutongGames.PlayMaker.Actions.ActivateGameObject.orig_DoActivateGameObject orig, HutongGames.PlayMaker.Actions.ActivateGameObject self){
            orig(self);
            if(self.gameObject.GameObject.Value != null){
                if(!GameManager.instance.IsGameplayScene()) { return; } 
                createHatConditionally(self.gameObject.GameObject.Value);
            }
        }
        public Coroutine hatCoro;
        public void update()
        {
            if(!GameManager.instance.IsGameplayScene()) { return; } 
            ManageHatCoroutine();
        }

    }

}
