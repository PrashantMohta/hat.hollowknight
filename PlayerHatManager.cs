using Satchel;
using System;
using UnityEngine;
using static Hat.Utils;

namespace Hat
{
    public class PlayerHatManager : MonoBehaviour
    {
        public GameObject HatGo;
        public Texture2D defaultHatTexture;
        public void Start()
        {
            defaultHatTexture = LoadTexture(Hat.Instance.CurrentDirectory, Hat.Instance.Settings.Hat);
            CreateHat();
            SetHatTexture(defaultHatTexture);
        }

        public void CreateHat()
        {
            if(HatGo == null) { 
                    HatGo = new GameObject("herohat");
                    GameObject.DontDestroyOnLoad(HatGo);
                    HatGo.SetScale(HeroController.instance.gameObject.transform.localScale.y);
                    HatGo.transform.position = HeroController.instance.gameObject.transform.position + Hat.Instance.Settings.Offset;
                    HatGo.transform.SetParent(HeroController.instance.gameObject.transform, true);
            }

        }

        public void SetHatTexture(Texture2D tex)
        {
            SpriteRenderer sr = HatGo.GetAddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 64f, 0, SpriteMeshType.FullRect);
            sr.color = new Color(1f, 1f, 1f, 1.0f);
            HatGo.SetActive(true);
        }

        public void ResetLocalHat()
        {
            SetHatTexture(defaultHatTexture);
            HatGo.SetActive(true);
        }

        internal void HideHat()
        {
            HatGo.SetActive(false);
        }
    }
}
