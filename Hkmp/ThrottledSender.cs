using Hat.Hkmp;
using HkmpPouch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Hat
{
    internal class ThrottledSender
    {
        private PipeServer pipe;
        private ushort currentPartNumber;
        private Timer EventTimer;
        private List<byte[]> buffer = new List<byte[]>();
        internal static ushort maxSliceSize = 500;
        private ushort playerId;
        private string filehash;

        public ThrottledSender(PipeServer pipe)
        {
            this.pipe = pipe;
        }
        public void CreateSlices(byte[] data)
        {

            for (ushort i = 0; i < data.Length;)
            {
                var slice = new List<byte>();
                var j = 0;
                while (j < maxSliceSize && i < data.Length)
                {
                    slice.Add(data[i]);
                    i++;
                    j++;
                }
                buffer.Add(slice.ToArray());
            }
        }
        public void Send(ushort playerId, string filehash, byte[] data)
        {
            this.playerId = playerId;
            this.filehash = filehash;
            pipe.Logger.Info($"{filehash} has size {data.Length}");
            CreateSlices(data);
            //todo make this actually work using acks
            this.currentPartNumber = 0;
            EventTimer = new Timer(5000);
            EventTimer.Elapsed += EventTimer_Elapsed;
            EventTimer.AutoReset = true;
            EventTimer.Start();
        }
        public void SendNext()
        {
            pipe.Logger.Info($"Sending {filehash} part {this.currentPartNumber}/{buffer.Count}");

            pipe.SendToPlayer(playerId, new RequestedFileEvent { fileHash = filehash, partNumber = this.currentPartNumber, totalParts = (ushort)buffer.Count, ExtraBytes = buffer[this.currentPartNumber] });
            this.currentPartNumber++;
            if (this.currentPartNumber > buffer.Count - 1)
            {
                pipe.Logger.Info($"SendBytesToPlayer end, sent {buffer.Count} packets!");
                EventTimer.Stop();
            }
        }
        private void EventTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendNext();
        }
    }
}
