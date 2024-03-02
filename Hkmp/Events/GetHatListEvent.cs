using HkmpPouch;

namespace Hat.Hkmp
{
    public class GetHatListEvent : PipeEvent
    {
        public static string Name = "GetHatListEvent";

        public override string GetName() => Name;

        public override string ToString()
        {
            return "";
        }
    }
    public class GetHatListEventFactory : IEventFactory
    {
        public static GetHatListEventFactory Instance { get; internal set; } = new GetHatListEventFactory();
        public PipeEvent FromSerializedString(string serializedData)
        {
            var pEvent = new GetHatListEvent();
            return pEvent;
        }

        public string GetName() => GetHatListEvent.Name;
    }

}
