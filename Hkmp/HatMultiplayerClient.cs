using Hkmp.Api.Client;
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
    public class HatMultiplayerClient
    {
        internal static PipeClient pipe;
        internal static bool isConnected = false;
        internal static HatManager hatManager;
        public HatMultiplayerClient()
        {
            pipe = new PipeClient(Constants.NAME);
            pipe.OnReady += Pipe_OnReady;
        }

        private void ClientManager_PlayerEnterSceneEvent(IClientPlayer player)
        {
            if (player.IsInLocalScene)
            {
                var playerHat = player.PlayerContainer.FindGameObjectInChildren("Player Prefab").GetAddComponent<PlayerHatBehaviour>();
                playerHat.SetPlayer(player.Id);
            }
        }

        private void ClientManager_ConnectEvent()
        {
            pipe.ServerCounterPartAvailable(whenServerCounterPartAvailable);
        }

        private void Pipe_OnReady(object sender, EventArgs e)
        {
            pipe.ClientApi.ClientManager.ConnectEvent += ClientManager_ConnectEvent;
            pipe.ClientApi.ClientManager.DisconnectEvent += ClientManager_DisconnectEvent;
            pipe.ClientApi.ClientManager.PlayerEnterSceneEvent += ClientManager_PlayerEnterSceneEvent;
            pipe.ClientApi.CommandManager.RegisterCommand(new HatsCommand());
            pipe.ServerCounterPartAvailable(whenServerCounterPartAvailable);
        }


        private void ClientManager_DisconnectEvent()
        {
            hatManager = null;
            Hat.Instance.resetLocalHat();
        }

        private void whenServerCounterPartAvailable(bool isAvailable)
        {
            if (isAvailable && hatManager == null) {
                hatManager = new HatManager(pipe);
                //pipe.SendToServer(new RequestFileEvent { fileHash = "hat.png" });
            }
        }

    }
}
