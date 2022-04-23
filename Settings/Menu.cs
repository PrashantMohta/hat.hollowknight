using Modding;
using UnityEngine;
using Satchel.BetterMenus;
using static Hat.Utils;
using Satchel;

namespace Hat
{
    public static class HatMenu {
        public static Menu MenuRef;

        public static Menu PrepareMenu(){
                return new Menu("Hat Mod", new Element[]{
                    new TextPanel("Add Hats to your knight",800f),
                    new HorizontalOption(
                        "World Hats", "Add Hat to more than the knight",
                        new string[] { "Disabled", "Enabled" },
                        (setting) => { Hat.settings.enableWorld = (setting == 1); },
                        () => Hat.settings.enableWorld ? 1 : 0,
                        Id:"enableWorld"),
                    new HorizontalOption(
                        "Random World Hats", "",
                        new string[] { "Disabled", "Enabled" },
                        (setting) => { Hat.settings.randomWorldHats = (setting == 1); },
                        () => Hat.settings.randomWorldHats ? 1 : 0,
                        Id:"randomWorldHats"),
                    new HorizontalOption(
                        "reduceChaos", "you like less chaos",
                        new string[] { "Disabled", "Enabled" },
                        (setting) => { Hat.settings.reduceChaos = (setting == 1); },
                        () => Hat.settings.reduceChaos ? 1 : 0,
                        Id:"reduceChaos"),
                    new CustomSlider(
                        "Offset X",
                        (f)=>{
                            Hat.settings.offsetX = f;
                        },
                        () => Hat.settings.offsetX,Id:"OffsetX"){minValue = -5,maxValue = 5},
                    new CustomSlider(
                        "Offset Y",
                        (f)=>{
                            Hat.settings.offsetY = f;
                        },
                        () => Hat.settings.offsetY,Id:"OffsetY"){minValue = -5,maxValue = 5},
                    new CustomSlider(
                        "Offset Z",
                        (f)=>{
                            Hat.settings.offsetZ = f;
                        },
                        () => Hat.settings.offsetZ,Id:"OffsetZ"){minValue = -5,maxValue = 5}
                });
            }
        

    }

}