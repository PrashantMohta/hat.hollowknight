using HkmpPouch;

namespace Hat.Hkmp
{
    public class RequestedFileEvent : PipeEvent
    {
        public static string Name = "RequestedFileEvent";

        public string fileHash = "";

        public override string GetName()=> Name;

        public override string ToString()
        {
            return fileHash;
        }
    }
    public class RequestedFileEventFactory : IEventFactory
    {
        public static RequestedFileEventFactory Instance { get; internal set; }  = new RequestedFileEventFactory();
        public PipeEvent FromSerializedString(string serializedData)
        {
            var pEvent = new RequestedFileEvent();
            pEvent.fileHash = serializedData;
            return pEvent;
        }

        public string GetName() => RequestedFileEvent.Name;
    }
}
