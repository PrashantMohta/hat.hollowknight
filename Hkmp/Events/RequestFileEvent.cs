using HkmpPouch;

namespace Hat.Hkmp
{
    public class RequestFileEvent : PipeEvent
    {
        public static string Name = "RequestFileEvent";

        public string fileHash = "";

        public override string GetName()=> Name;

        public override string ToString()
        {
            return fileHash;
        }
    }
    public class RequestFileEventFactory : IEventFactory
    {
        public static RequestFileEventFactory Instance { get; internal set; }  = new RequestFileEventFactory();
        public PipeEvent FromSerializedString(string serializedData)
        {
            var pEvent = new RequestFileEvent();
            pEvent.fileHash = serializedData;
            return pEvent;
        }

        public string GetName() => RequestFileEvent.Name;
    }
}
