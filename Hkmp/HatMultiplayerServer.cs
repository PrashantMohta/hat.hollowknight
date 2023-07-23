using Hkmp.Api.Server;
using HkmpPouch;
using Satchel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var cacheDirectory = Path.Combine(path, "HkmpHats");
            IoUtils.EnsureDirectory(cacheDirectory);
            foreach (var file in Directory.GetFiles(cacheDirectory))
            {
                if (Path.GetExtension(file).ToLower() != ".png")
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

        private void HatListRequested(GetHatListEvent obj)
        {
            pipe.SendToPlayer(obj.FromPlayer, new GotHatListEvent { Hatlist = cachedFiles.Keys.ToArray() });

            foreach (var player in pipe.ServerApi.ServerManager.Players)
            {
                if (players.TryGetValue(player.Id, out var hat))
                {
                    pipe.SendToPlayer(obj.FromPlayer, new PlayerHatEvent { playerId = player.Id, hat = hat });
                }
            }
        }

        private void SendBytesToPlayer(ushort playerId,string filehash, byte[] data)
        {
            pipe.SendToPlayer(playerId, new RequestedFileEvent { fileHash = filehash, ExtraBytes = data});
        }

        private void FileRequested(RequestFileEvent e)
        {
            pipe.Logger.Info($"File requested server : {e.fileHash}");
            if (e.fileHash.Length > 0)
            {
                if(cachedFiles.TryGetValue(e.fileHash, out var data))
                {
                    SendBytesToPlayer(e.FromPlayer, e.fileHash, data);
                }
            } else
            {
                pipe.Logger.Info($"File not found : {e.fileHash}");
            }
        }
    }
}
