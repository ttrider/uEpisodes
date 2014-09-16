using System;
using System.Diagnostics;
using System.Text;

namespace TTRider.uEpisodes.Data
{
    class AppModelTraceItem
    {
        public DateTime Timestamp { get; set; }
        public TraceEventType Type { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }

        public override string ToString()
        {
            var msg = new StringBuilder(Message??"");
            msg.Replace('\t',' ');
            msg.Replace('\r', ' ');
            msg.Replace('\n', ' ');

            var msg2 = new StringBuilder(Details??"");
            msg2.Replace('\t',' ');
            msg2.Replace('\r', ' ');
            msg2.Replace('\n', ' ');

            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\r\n", Timestamp, Type, Process.GetCurrentProcess().Id, msg, msg2);
        }
    }
}
