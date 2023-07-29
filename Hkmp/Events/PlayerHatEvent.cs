using HkmpPouch;
using System.Globalization;

namespace Hat.Hkmp
{
    public class GetPlayerHatEvent : PipeEvent
    {
        public static string Name = "GetPlayerHatEvent";

        public ushort playerId;
        public override string GetName() => Name;

        public override string ToString()
        {
            return $"{playerId.ToString(CultureInfo.InvariantCulture)}";
        }
    }
    public class GetPlayerHatEventFactory : IEventFactory
    {
        public static GetPlayerHatEventFactory Instance { get; internal set; } = new GetPlayerHatEventFactory();
        public PipeEvent FromSerializedString(string serializedData)
        {
            var pEvent = new GetPlayerHatEvent();
            pEvent.playerId = ushort.Parse(serializedData, CultureInfo.InvariantCulture);
            return pEvent;
        }

        public string GetName() => GetPlayerHatEvent.Name;
    }
}
