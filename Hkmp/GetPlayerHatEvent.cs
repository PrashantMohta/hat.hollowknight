using HkmpPouch;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hat.Hkmp
{
    public class PlayerHatEvent : PipeEvent
    {
        public static string Name = "PlayerHatEvent";

        public ushort playerId;

        public string hat;

        public override string GetName() => Name;

        public override string ToString()
        {
            return $"{playerId.ToString(CultureInfo.InvariantCulture)},{hat}";
        }
    }
    public class PlayerHatEventFactory : IEventFactory
    {
        public static PlayerHatEventFactory Instance { get; internal set; } = new PlayerHatEventFactory();
        public PipeEvent FromSerializedString(string serializedData)
        {
            var pEvent = new PlayerHatEvent();
            var Split = serializedData.Split(',');
            pEvent.playerId = ushort.Parse(Split[0],CultureInfo.InvariantCulture);
            pEvent.hat = Split[1];
            return pEvent;
        }

        public string GetName() => PlayerHatEvent.Name;
    }
}
