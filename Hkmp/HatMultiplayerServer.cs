using Hkmp.Api.Server;
using HkmpPouch;
using HkmpPouch.Multipart;
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
        private Dictionary<ushort, string> players = new Dictionary<ushort, string>();

        private MultipartContentManager multipartContentManager;

        public override void Initialize(IServerApi serverApi)
        {
            pipe = new PipeServer(Name);
            multipartContentManager = new MultipartContentManager(pipe);
            multipartContentManager.ContentRequestHandler = HatRequestHandler;
            // populate Hat list 
            PopulateHatList();
            // get hatlist
            pipe.On(GetHatListEventFactory.Instance).Do<GetHatListEvent>(HatListRequested);
            // set player hat
            pipe.On(PlayerHatEventFactory.Instance).Do<PlayerHatEvent>(PlayerHatSet);
            // get player hat 
            pipe.On(GetPlayerHatEventFactory.Instance).Do<GetPlayerHatEvent>(PlayerHatRequested);
        }

        private void HatRequestHandler(RequestedMultipartContent content)
        {
            pipe.Logger.Info($"Sending {content.ContentId} part {content.PartNumber}/{content.TotalParts}");
            pipe.SendToPlayer(content.FromPlayer, content);
            pipe.Logger.Info($"SendBytesToPlayer end!");
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
                    multipartContentManager.RegisterContent(Path.GetFileName(file), File.ReadAllBytes(file));
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
            foreach (var file in multipartContentManager.GetContentList())
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


    }
}
