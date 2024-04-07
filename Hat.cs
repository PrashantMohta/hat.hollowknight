using Hat.Hkmp;
using Hkmp.Api.Server;
using Modding;
using Satchel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Hat.Utils;

namespace Hat
{
    public class Hat : Mod
    {
        internal static Hat Instance;
        internal static PlayerHatManager LocalHatManager;
        internal static OtherHatManager OtherHatManager;
        internal static HatMultiplayerClient MpClientInstance;
        internal static HatMultiplayerServer MpServerInstance;

        internal string CurrentDirectory = Path.Combine(AssemblyUtils.getCurrentDirectory(), Constants.HAT_FOLDER_NAME);

        internal Setting Settings = new Setting();

        internal List<Texture2D> HatTextures = new List<Texture2D>();
        internal Dictionary<int,Sprite> HatSprites = new Dictionary<int,Sprite>();


        public override string GetVersion()
        {
            return AssemblyUtils.GetAssemblyVersionHash(Constants.VERSION);
        }

        public void Loadhats()
        {
            if (!Settings.EnableWorld) { return; }
            if (Settings.RandomWorldHats)
            {
                foreach (string hatpng in Directory.GetFiles(CurrentDirectory, $"*{Constants.FILE_EXTENSION}"))
                {
                    HatTextures.Add(LoadTexture(CurrentDirectory, hatpng));
                }
            }
            else
            {
                HatTextures.Add(LoadTexture(CurrentDirectory, Settings.Hat));
            }
        }

        public override void Initialize()
        {
            Instance = this;
            IoUtils.EnsureDirectory(CurrentDirectory);
            
            var settingsPath = Path.Combine(CurrentDirectory, Constants.SETTINGS_FILE);
            if(File.Exists(settingsPath)){
                try { 
                    Settings = DeSerialiseSetting(settingsPath);
                } catch(Exception e){
                    Log(e);
                }
            }
            SerialiseSetting(settingsPath,Settings);


            var hatPath = Path.Combine(CurrentDirectory, Constants.DEFAULT_HAT);
            if(!File.Exists(hatPath)) {
                ExtractFile(hatPath);
            }

            Loadhats(); // load for world mode
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




        public void HeroControllerStart(On.HeroController.orig_Start orig,HeroController self){
            orig(self);
            LocalHatManager = self.gameObject.GetAddComponent<PlayerHatManager>();
            OtherHatManager = self.gameObject.GetAddComponent<OtherHatManager>();
        }


    }

}
