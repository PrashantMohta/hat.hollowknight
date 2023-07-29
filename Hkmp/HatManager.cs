using HkmpPouch;
using Satchel;
using System;
using System.Collections.Generic;
using System.IO;
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

        public string[] hats = new string[0];
        public int currentlyLoading = 0;
        public HatManager(PipeClient pipe)
        {
            this.pipe = pipe;
            pipe.On(RequestedFileEventFactory.Instance).Do<RequestedFileEvent>(RecievedFile);
            pipe.On(GotHatListEventFactory.Instance).Do<GotHatListEvent>(GotHatList);
            pipe.On(PlayerHatEventFactory.Instance).Do<PlayerHatEvent>(PlayerHatUpdated);

            LoadCachedHats();
            PopulateTextureList();
        }

        private void PlayerHatUpdated(PlayerHatEvent obj)
        {
            players[obj.playerId] = obj.hat;
            playerHatUpdated?.Invoke(obj, EventArgs.Empty);
        }

        private void LoadCachedHats()
        {
            var path = AssemblyUtils.getCurrentDirectory();
            var cacheDirectory = Path.Combine(path, "HkmpHatsCache");
            IoUtils.EnsureDirectory(cacheDirectory);
            foreach (var file in Directory.GetFiles(cacheDirectory))
            {
                if(Path.GetExtension(file).ToLower() != ".png")
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

        private void RecievedFile(RequestedFileEvent e)
        {
            var path = AssemblyUtils.getCurrentDirectory();
            var cacheDirectory = Path.Combine(path, "HkmpHatsCache");
            var filePath = Path.Combine(cacheDirectory, e.fileHash);
            IoUtils.EnsureDirectory(cacheDirectory);
            File.WriteAllBytes(filePath, e.ExtraBytes);
            var tex = TextureUtils.LoadTextureFromFile(filePath);
            cachedFiles[e.fileHash] = tex;
            if (pendingCallback.ContainsKey(e.fileHash))
            {
                foreach(var cb in pendingCallback[e.fileHash])
                {
                    cb(cachedFiles[e.fileHash]);
                }
                pendingCallback[e.fileHash].Clear();
            }
        }
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
                pipe.SendToServer(new RequestFileEvent { fileHash = fileHash });
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
