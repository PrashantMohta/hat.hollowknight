using Satchel;
using System;
using UnityEngine;

namespace Hat.Hkmp
{
    internal class PlayerHatBehaviour : MonoBehaviour
    {
        ushort playerId;
        string hatFile;
        Texture2D hatTexture;
        GameObject hat;
        public void Start()
        {
            HatMultiplayerClient.hatManager.playerHatUpdated += PlayerhatUpdated;
        }
        private void PlayerhatUpdated()
        {
            if (HatMultiplayerClient.hatManager != null && HatMultiplayerClient.hatManager.players != null && HatMultiplayerClient.hatManager.players.TryGetValue(playerId, out var playerHat))
            {
                if (hatFile != playerHat)
                {
                    Modding.Logger.Log($"{playerHat}");

                    hatFile = playerHat;
                    HatMultiplayerClient.hatManager.GetTextureByHash(playerHat, TextureLoaded);
                }
            }
        }
        private void PlayerhatUpdated(object sender, EventArgs e)
        {
            Modding.Logger.Log("  updated  ");
            PlayerhatUpdated();
        }

        internal void SetPlayer(ushort id)
        {
            playerId = id;
            PlayerhatUpdated();
        }

        private void TextureLoaded(Texture2D tex)
        {
            hatTexture = tex;
            createOrUpdateHat();
        }

        private void createOrUpdateHat()
        {
            if(gameObject == null || transform == null || hatTexture == null)
            {
                Modding.Logger.Log("  null?  ");
                return;
            }
            if(hat == null)
            {
                hat = gameObject.FindGameObjectInChildren("hat");
            }
            if (hat == null) { 
                hat = new GameObject(" hat");
                hat.SetActive(true);
                hat.SetScale(transform.localScale.y);
                hat.transform.position = transform.position + new Vector3(Hat.Instance.settings.offsetX, Hat.Instance.settings.offsetY, Hat.Instance.settings.offsetZ);
                hat.transform.SetParent(transform, true);

            }
            SpriteRenderer sr = hat.GetAddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(hatTexture, new Rect(0f, 0f, hatTexture.width, hatTexture.height), new Vector2(0.5f, 0.5f), 64f, 0, SpriteMeshType.FullRect);
            sr.color = new Color(1f, 1f, 1f, 1.0f);
        }


        public void OnDisable()
        {
            playerId = 0;
            Destroy(hat);
        }
    }
}
