using HkmpPouch;
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
        private void RecievedFile(RequestedFileEvent e)
        {
            var path = AssemblyUtils.getCurrentDirectory();
            var cacheDirectory = Path.Combine(path, Constants.HKMP_CACHE_FOLDER_NAME);
            var filePath = Path.Combine(cacheDirectory, e.fileHash);
            IoUtils.EnsureDirectory(cacheDirectory);
            Modding.Logger.Log($"byte length {e.ExtraBytes.Length} data : {String.Join(",", Array.ConvertAll<byte, String>(e.ExtraBytes, Convert.ToString))}");
            if (!multipart.ContainsKey(e.fileHash))
            {
                multipart[e.fileHash] = new ();
            }
            multipart[e.fileHash][e.partNumber] = e.ExtraBytes;
            if (multipart[e.fileHash].Count == e.totalParts)
            {
                var totalData = new List<byte>();
                foreach ( var part in multipart[e.fileHash])
                {
                    totalData.AddRange(part.Value);
                }
                File.WriteAllBytes(filePath, totalData.ToArray());
                var tex = TextureUtils.LoadTextureFromFile(filePath);
                cachedFiles[e.fileHash] = tex;
                if (pendingCallback.ContainsKey(e.fileHash))
                {
                    foreach (var cb in pendingCallback[e.fileHash])
                    {
                        cb(cachedFiles[e.fileHash]);
                    }
                    pendingCallback[e.fileHash].Clear();
                }
            } else
            {
                //request the next pending part
                ushort nextPart = (ushort)(e.partNumber + 1);
                Modding.Logger.Log($"request next part {nextPart},{nextPart < e.totalParts},{multipart[e.fileHash].ContainsKey(nextPart)}");
                if (nextPart < e.totalParts && !multipart[e.fileHash].ContainsKey(nextPart))
                {
                    pipe.SendToServer(new RequestFileEvent { fileHash = e.fileHash , partNumber = nextPart});
                }
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
