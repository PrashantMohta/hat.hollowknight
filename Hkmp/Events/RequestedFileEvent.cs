using HkmpPouch;
using System.Globalization;

namespace Hat.Hkmp
{
    public class RequestedFileEvent : PipeEvent
    {
        public RequestedFileEvent() { 
            base.IsReliable = true;
        }

        public static string Name = "RequestedFileEvent";

        public string fileHash = "";

        public ushort partNumber = 0;

        public ushort totalParts = 1;
        public override string GetName()=> Name;

        public override string ToString()
        {
            return $"{fileHash},{partNumber},{totalParts}";
        }
    }
    public class RequestedFileEventFactory : IEventFactory
    {
        public static RequestedFileEventFactory Instance { get; internal set; }  = new RequestedFileEventFactory();
        public PipeEvent FromSerializedString(string serializedData)
        {
            var pEvent = new RequestedFileEvent();
            var Split = serializedData.Split(',');
            pEvent.fileHash = Split[0];
            pEvent.partNumber = ushort.Parse(Split[1], CultureInfo.InvariantCulture);
            pEvent.totalParts = ushort.Parse(Split[2], CultureInfo.InvariantCulture);
            return pEvent;
        }

        public string GetName() => RequestedFileEvent.Name;
    }
}
