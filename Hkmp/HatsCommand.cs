using Hkmp.Api.Command.Client;
using System;

namespace Hat.Hkmp
{
    internal class HatsCommand : IClientCommand
    {
        public string Trigger => "/hats";

        public string[] Aliases => new string[] { "/list-hats" };

        public void Execute(string[] arguments)
        {
            if (arguments.Length == 1)
            {
                HatMultiplayerClient.pipe.ClientApi.UiManager.ChatBox.AddMessage("Available Hats:");
                foreach (string hat in HatMultiplayerClient.hatManager.hats)
                {
                    HatMultiplayerClient.pipe.ClientApi.UiManager.ChatBox.AddMessage(hat);
                }
            }
            else if (Array.IndexOf(HatMultiplayerClient.hatManager.hats, arguments[1]) >= 0)
            {
                HatMultiplayerClient.pipe.SendToServer(new PlayerHatEvent { hat = arguments[1] });
                HatMultiplayerClient.hatManager.GetTextureByHash(arguments[1], Hat.LocalHatManager.SetHatTexture);
            }
            else {
                HatMultiplayerClient.pipe.ClientApi.UiManager.ChatBox.AddMessage("Invalid!");
            }

        }
    }
}