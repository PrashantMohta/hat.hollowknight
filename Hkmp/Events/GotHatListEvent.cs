using HkmpPouch;
using System;

namespace Hat.Hkmp
{
    public class GotHatListEvent : PipeEvent
    {
        public static string Name = "GotHatListEvent";

        public string[] Hatlist;
        public override string GetName() => Name;

        public override string ToString()
        {
            return String.Join(",", Hatlist);
        }
    }
    public class GotHatListEventFactory : IEventFactory
    {
        public static GotHatListEventFactory Instance { get; internal set; } = new GotHatListEventFactory();
        public PipeEvent FromSerializedString(string serializedData)
        {
            var pEvent = new GotHatListEvent();
            pEvent.Hatlist = serializedData.Split(',');
            return pEvent;
        }

        public string GetName() => GotHatListEvent.Name;
    }
}
