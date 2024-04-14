using HkmpPouch;
using System.Globalization;

namespace Hat.Hkmp
{
    public class RequestFileEvent : PipeEvent
    {
        public RequestFileEvent()
        {
            base.IsReliable = true;
        }
        public static string Name = "RequestFileEvent";

        public string fileHash = "";
        public ushort partNumber = 0;
        public override string GetName()=> Name;

        public override string ToString()
        {
            return $"{fileHash},{partNumber}";
        }
    }
    public class RequestFileEventFactory : IEventFactory
    {
        public static RequestFileEventFactory Instance { get; internal set; }  = new RequestFileEventFactory();
        public PipeEvent FromSerializedString(string serializedData)
        {
            var pEvent = new RequestFileEvent();
            var Split = serializedData.Split(',');
            pEvent.fileHash = Split[0];
            pEvent.partNumber = ushort.Parse(Split[1], CultureInfo.InvariantCulture);
            return pEvent;
        }

        public string GetName() => RequestFileEvent.Name;
    }
}
