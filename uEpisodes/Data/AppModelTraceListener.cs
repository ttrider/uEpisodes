using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace TTRider.uEpisodes.Data
{
    class AppModelTraceListener : TraceListener
    {
        public AppModelTraceListener()
        {
            this.LogItems = new ObservableCollection<AppModelTraceItem>();
            //Trace.Listeners.Add(this);
        }

        public string LogFile
        {
            get
            {
                var path = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TTRider",
                                    "uEpisodes", "log.txt"));
                if ((path.Directory != null) && (!path.Directory.Exists))
                {
                    path.Directory.Create();
                }
                return path.FullName;
            }
        }

        public ObservableCollection<AppModelTraceItem> LogItems { get; private set; }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            ProcessEvent(eventType); 
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            ProcessEvent(eventType); 
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            ProcessEvent(eventType);  
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            ProcessEvent(eventType, string.Format(format, args));     
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            ProcessEvent(eventType, message);    
        }

        public override void Fail(string message, string detailMessage)
        {
            ProcessEvent(TraceEventType.Error, message, detailMessage);
        }


        public override void Write(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                File.AppendAllText(LogFile, message);
            }
        }

        public override void WriteLine(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                File.AppendAllText(LogFile, message + "\r\n");
            }
        }

        private void ProcessEvent(TraceEventType eventLogEntryType, string message=null, string detailMessage=null)
        {
            var item = new AppModelTraceItem
                {
                    Details = detailMessage,
                    Message = message,
                    Type = eventLogEntryType,
                    Timestamp = DateTime.Now
                };

            if ((Application.Current != null) && (Application.Current.Dispatcher != null) && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => this.LogItems.Add(item)));
            }
            else
            {
                this.LogItems.Add(item);
            }
            File.AppendAllText(LogFile, item.ToString());
        }
    }
}
