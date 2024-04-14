using Hkmp.Api.Server;
using HkmpPouch;
using Satchel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hat.Hkmp
{
    public class HatMultiplayerServer : ServerAddon
    {
        public override bool NeedsNetwork => false;

        protected override string Name => Constants.NAME;

        protected override string Version => Constants.VERSION;

        private PipeServer pipe;
        private Dictionary<string, byte[]> cachedFiles = new Dictionary<string, byte[]>();
        private Dictionary<ushort, string> players = new Dictionary<ushort, string>();

        public override void Initialize(IServerApi serverApi)
        {
            pipe = new PipeServer(Name);
            // populate Hat list 
            PopulateHatList();
            // get hatlist
            pipe.On(GetHatListEventFactory.Instance).Do<GetHatListEvent>(HatListRequested);
            // set player hat
            pipe.On(PlayerHatEventFactory.Instance).Do<PlayerHatEvent>(PlayerHatSet);
            // get player hat 
            pipe.On(GetPlayerHatEventFactory.Instance).Do<GetPlayerHatEvent>(PlayerHatRequested);
            // get hat
            pipe.On(RequestFileEventFactory.Instance).Do<RequestFileEvent>(FileRequested);
        }


        private void PlayerHatSet(PlayerHatEvent obj)
        {
            players[obj.FromPlayer] = obj.hat;
            pipe.Broadcast(new PlayerHatEvent { playerId = obj.FromPlayer, hat = obj.hat });
        }

        private void PlayerHatRequested(GetPlayerHatEvent obj)
        {
            if(players.TryGetValue(obj.playerId, out var hatName))
            {
                pipe.SendToPlayer(obj.FromPlayer, new PlayerHatEvent { playerId = obj.playerId, hat = hatName });
            }
        }

        private void PopulateHatList()
        {
            var path = AssemblyUtils.getCurrentDirectory();
            var cacheDirectory = Path.Combine(path, Constants.HKMP_HATS_FOLDER_NAME);
            IoUtils.EnsureDirectory(cacheDirectory);
            foreach (var file in Directory.GetFiles(cacheDirectory))
            {
                if (Path.GetExtension(file).ToLower() != Constants.FILE_EXTENSION)
                {
                    continue;
                }
                try
                {
                    cachedFiles[Path.GetFileName(file)] = File.ReadAllBytes(file);
                }
                catch (Exception ex)
                {
                    pipe.Logger.Error(ex.ToString());
                }
            }
        }
        private string[] GetHatList()
        {
            List<string> hats = new List<string>();
            foreach (var file in cachedFiles.Keys)
            {
                if (Path.GetExtension(file).ToLower() == Constants.FILE_EXTENSION)
                {
                    hats.Add(file);
                }
            }
            return hats.ToArray();
        }
        private void HatListRequested(GetHatListEvent obj)
        {
            pipe.SendToPlayer(obj.FromPlayer, new GotHatListEvent { Hatlist = GetHatList() });

            foreach (var player in pipe.ServerApi.ServerManager.Players)
            {
                if (players.TryGetValue(player.Id, out var hat))
                {
                    pipe.SendToPlayer(obj.FromPlayer, new PlayerHatEvent { playerId = player.Id, hat = hat });
                }
            }
        }

        private void SendBytesToPlayer(ushort playerId,string filehash,ushort partNumber, byte[] data)
        {
            ushort partSize = 100;
            ushort totalParts = (ushort)Math.Ceiling((float)data.Length/partSize);
            pipe.Logger.Info($"Sending {filehash} part {partNumber}/{totalParts}");
            var currentIndex = partNumber * partSize;
            List<byte> currentBuffer = new List<byte>();
            var readPos = 0;
            while(currentIndex + readPos < data.Length && readPos < partSize)
            {
                currentBuffer.Add(data[currentIndex + readPos]);
                readPos++;
            }
            pipe.SendToPlayer(playerId, new RequestedFileEvent { fileHash = filehash, partNumber = partNumber, totalParts = totalParts, ExtraBytes = currentBuffer.ToArray() });
            pipe.Logger.Info($"SendBytesToPlayer end!");
        }

        private void FileRequested(RequestFileEvent e)
        {
            pipe.Logger.Info($"File requested server : {e.fileHash}");
            if (e.fileHash.Length > 0)
            {
                if(cachedFiles.TryGetValue(e.fileHash, out var data))
                {
                    pipe.Logger.Info($"File found in cache : {e.fileHash}");
                    SendBytesToPlayer(e.FromPlayer, e.fileHash,e.partNumber, data);
                } else
                {
                    pipe.Logger.Info($"File not found in cache : {e.fileHash}");
                }
            } else
            {
                pipe.Logger.Info($"File not found : {e.fileHash}");
            }
        }
    }
}
