using Modding;
using UnityEngine;
using Satchel.BetterMenus;
using static Hat.Utils;
using Satchel;

namespace Hat
{
    public static class HatMenu {
        public static Menu MenuRef;

        static GameObject preview,previewHat;
        public static GameObject getPreviwGo(){
            if(preview != null) { 
                //GameObject.Destroy(preview); 
                //GameObject.Destroy(previewHat); 
            }
            preview = new GameObject("HatPreviewGo");
            var bc = preview.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(0.5f, 1.3f);
            bc.offset = new Vector2(0.0f, -0.8f);
            SpriteRenderer sr = preview.AddComponent<SpriteRenderer>();
            Texture2D tex =  AssemblyUtils.GetTextureFromResources("knight.png");
            sr.sprite = Sprite.Create(tex,new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f),128f,0,SpriteMeshType.FullRect);
            sr.color = new Color(1f, 1f, 1f, 1.0f);
            previewHat = Hat.Instance.preCreateHat();
            updateHatPos();
            return preview;
        }


        public static void updateHatPos(){
            Vector3 center = getParentColliderCenter(preview);
            float scale = preview.transform.localScale.x;
            preview.SetActive(true);
            previewHat.SetActive(true);
            previewHat.SetScale(0.5f);
            previewHat.transform.position = center + new Vector3(Hat.settings.offsetX,Hat.settings.offsetY,Hat.settings.offsetZ);
            previewHat.transform.SetParent(preview.transform);
        }
        public static Menu PrepareMenu(){
                return new Menu("Hat Mod", new Element[]{
                    new TextPanel("Add Hats to your knight",800f),
                    new CustomSlider(
                        "",
                        (f)=>{},
                        () => Hat.settings.offsetX,Id:"OffetX"){minValue = -5,maxValue = 5},
                    new StaticPanel("preview", (parent) => {
                        var previewGo = getPreviwGo();
                        previewGo.transform.SetParent(parent.transform);            
                        previewGo.transform.localPosition = ZeroVector;
                    }),
                    new CustomSlider(
                        "Offset X",
                        (f)=>{
                            Hat.settings.offsetX = f;
                            updateHatPos();
                        },
                        () => Hat.settings.offsetX,Id:"OffsetX"){minValue = -5,maxValue = 5},
                    new CustomSlider(
                        "Offset Y",
                        (f)=>{
                            Hat.settings.offsetY = f;
                            updateHatPos();
                        },
                        () => Hat.settings.offsetY,Id:"OffsetY"){minValue = -5,maxValue = 5},
                    new CustomSlider(
                        "Offset Z",
                        (f)=>{
                            Hat.settings.offsetZ = f;
                            updateHatPos();
                        },
                        () => Hat.settings.offsetZ,Id:"OffsetZ"){minValue = -5,maxValue = 5},
                });
            }
        

    }

}