using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker.Actions;
using Satchel;
using static Hat.Utils;

namespace Hat
{
    public class OtherHatManager : MonoBehaviour
    {

        public Coroutine HatCoro;

        public void Start()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ActiveSceneChanged;
            On.HutongGames.PlayMaker.Actions.ActivateGameObject.DoActivateGameObject += ObjectActivated;
            StartCoroutine();
        }

        public void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= ActiveSceneChanged;
            On.HutongGames.PlayMaker.Actions.ActivateGameObject.DoActivateGameObject -= ObjectActivated;
            if (HatCoro != null)
            {
                CoroutineHelper.GetRunner().StopCoroutine(HatCoro);
            }
        }
        public void StartCoroutine()
        {
            if (HatCoro != null)
            {
                CoroutineHelper.GetRunner().StopCoroutine(HatCoro);
            }
            HatCoro = CoroutineHelper.WaitForFramesBeforeInvoke(1, CheckForHatCandidates);

        }

        private void ObjectActivated(On.HutongGames.PlayMaker.Actions.ActivateGameObject.orig_DoActivateGameObject orig, ActivateGameObject self)
        {
            orig(self);
            if (self.gameObject.GameObject.Value != null)
            {
                if (!GameManager.instance.IsGameplayScene()) { return; }
                CreateHatConditionally(self.gameObject.GameObject.Value);
            }
        }

        private void ActiveSceneChanged(Scene arg0, Scene arg1)
        {
            if (!GameManager.instance.IsGameplayScene()) { return; }
            StartCoroutine();
        }


        private void CheckForHatCandidates()
        {
            var GOList = GameObject.FindObjectsOfType<GameObject>();
            foreach (var gameObj in GOList)
            {
                if (gameObj == null) { continue; }
                CreateHatConditionally(gameObj);
            }
            HatCoro = Satchel.CoroutineHelper.WaitForSecondsBeforeInvoke(1f, CheckForHatCandidates);
        }

        private void CreateHatConditionally(GameObject gameObj)
        {
            if(Hat.Instance.HatTextures.Count == 0) { return; }
            if (gameObj.GetComponent<HatState>() != null) { return; }
            if (Hat.Instance.Settings.ReduceChaos)
            {
                var goName = gameObj.name.ToLower();
                foreach (var name in Hat.Instance.Settings.LimitTo)
                {
                    if (goName.Contains(name.ToLower()))
                    {
                        CreateHatOnParent(gameObj, true);
                        break;
                    }
                }
                CreateHatOnParent(gameObj, false);
            }
            else
            {
                CreateHatOnParent(gameObj, true);
            }
        }


        private void CreateHatOnParent(GameObject Parent, bool force)
        {
            try
            {
                var HatState = Parent.GetAddComponent<HatState>();
                if (!force && !HatState.HasHat && Parent.GetComponent<HealthManager>() == null && Parent.LocateMyFSM("npc_control") == null)
                {
                    HatState.HasHat = false;
                    return;
                }
                if (HatState.HasHat || Parent.FindGameObjectInChildren(Parent.name + " hat") != null)
                {
                    return;
                }
                HatState.HasHat = true;
                Vector3 center = GetParentColliderCenter(Parent);
                if (center == ZeroVector) { return; }
                if (Hat.Instance.Settings.Verbose)
                {
                    Hat.Instance.Log("Adding Hat to" + Parent.name);
                }
                var hat = new GameObject(Parent.name + " hat");
                SpriteRenderer sr = hat.AddComponent<SpriteRenderer>();
                float scale = Parent.transform.localScale.x;
                var index = rand.Next(Hat.Instance.HatTextures.Count);
                Sprite spr;
                if (Hat.Instance.HatSprites.TryGetValue(index, out spr))
                {
                    sr.sprite = spr;
                }
                else
                {
                    Texture2D tex = Hat.Instance.HatTextures[index];
                    sr.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 64f, 0, SpriteMeshType.FullRect);
                    Hat.Instance.HatSprites[index] = sr.sprite;
                }
                sr.color = new Color(1f, 1f, 1f, 1.0f);
                hat.SetActive(true);
                hat.SetScale(Parent.transform.localScale.y);
                hat.transform.position = Parent.transform.position + new Vector3(0, Parent.transform.localScale.y * 1.1f, -0.0001f);
                hat.transform.SetParent(Parent.transform, true);

            }
            catch (Exception e)
            {
                Hat.Instance.Log(e);
            }
        }

    }
}
