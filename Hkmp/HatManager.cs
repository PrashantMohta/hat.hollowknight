using HkmpPouch;
using HkmpPouch.Multipart;
using Satchel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Hat.Hkmp
{
    internal class HatManager
    {
        private PipeClient pipe;

        internal Dictionary<ushort, string> players = new Dictionary<ushort, string>();

        private Dictionary<string, Texture2D> cachedFiles = new Dictionary<string, Texture2D>();

        private Dictionary<string, List<Action<Texture2D>>> pendingCallback = new Dictionary<string, List<Action<Texture2D>>>();

        public EventHandler<EventArgs> playerHatUpdated;
        private MultipartContentManager multipartContentManager;

        public string[] hats = new string[0];
        public int currentlyLoading = 0;
        public HatManager(PipeClient pipe)
        {
            this.pipe = pipe;
            multipartContentManager = new MultipartContentManager(pipe);
            multipartContentManager.ContentReceivedHandler = HatReceivedHandler;
            multipartContentManager.SendContentRequest = HatRequestSender;
            pipe.On(GotHatListEventFactory.Instance).Do<GotHatListEvent>(GotHatList);
            pipe.On(PlayerHatEventFactory.Instance).Do<PlayerHatEvent>(PlayerHatUpdated);

            LoadCachedHats();
            PopulateTextureList();
        }

        private void HatRequestSender(RequestMultipartContent content)
        {
            pipe.SendToServer(content);
        }

        private void HatReceivedHandler(RequestedMultipartContent content)
        {
            var path = AssemblyUtils.getCurrentDirectory();
            var cacheDirectory = Path.Combine(path, Constants.HKMP_CACHE_FOLDER_NAME);
            var filePath = Path.Combine(cacheDirectory, content.ContentId);
            IoUtils.EnsureDirectory(cacheDirectory);
            Modding.Logger.Log($"{content.ContentId} byte size {content.ExtraBytes.Length}");
            File.WriteAllBytes(filePath, content.ExtraBytes);
            var tex = TextureUtils.LoadTextureFromFile(filePath);
            cachedFiles[content.ContentId] = tex;
            if (pendingCallback.ContainsKey(content.ContentId))
            {
                foreach (var cb in pendingCallback[content.ContentId])
                {
                    cb(cachedFiles[content.ContentId]);
                }
                pendingCallback[content.ContentId].Clear();
            }
        }

        private void PlayerHatUpdated(PlayerHatEvent obj)
        {
            players[obj.playerId] = obj.hat;
            playerHatUpdated?.Invoke(obj, EventArgs.Empty);
        }

        private void LoadCachedHats()
        {
            var path = AssemblyUtils.getCurrentDirectory();
            var cacheDirectory = Path.Combine(path, Constants.HKMP_CACHE_FOLDER_NAME);
            IoUtils.EnsureDirectory(cacheDirectory);
            foreach (var file in Directory.GetFiles(cacheDirectory))
            {
                if(Path.GetExtension(file).ToLower() != Constants.FILE_EXTENSION)
                {
                    continue;
                }
                try
                {
                    cachedFiles[Path.GetFileName(file)] = TextureUtils.LoadTextureFromFile(file);
                }
                catch (Exception ex)
                {
                    pipe.Logger.Error(ex.ToString());
                }
            }
        }

        private void GotHatList(GotHatListEvent obj)
        {
            hats = obj.Hatlist;
            //attempt to preload hats 1 by 1
            LoadNextHat();
        }

        private void LoadNextHat()
        {
            if(currentlyLoading >= hats.Length)
            {
                return;
            }
            this.GetTextureByHash(hats[currentlyLoading], onLoadedHat);
        }

        private void onLoadedHat(Texture2D obj)
        {
            currentlyLoading++;
            LoadNextHat();
        }
        private Dictionary<string, Dictionary<ushort, byte[]>> multipart = new();

        public void GetTextureByHash(string fileHash, Action<Texture2D> callback)
        {
            Texture2D texture;
            if (cachedFiles.TryGetValue(fileHash, out texture) && texture != null)
            {
                pipe.Logger.Info("cachedFiles.TryGetValue");
                callback(texture);
            }
            else
            {
                pipe.Logger.Info("RequestFileEvent");
                multipartContentManager.RequestContent(fileHash);
                if (!pendingCallback.ContainsKey(fileHash))
                {
                    pendingCallback.Add(fileHash, new List<Action<Texture2D>> { callback });
                }
                else
                {
                    pendingCallback[fileHash].Add(callback);
                }
            }
        }
        public void PopulateTextureList()
        {
            pipe.SendToServer(new GetHatListEvent());
        }
    }
}
