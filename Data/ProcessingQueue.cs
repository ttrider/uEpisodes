using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace TTRider.uEpisodes.Data
{
    class ProcessingQueue
    {
        public static readonly ProcessingQueue Current = new ProcessingQueue();

        readonly ConcurrentQueue<ProcessingQueueItem> queue = new ConcurrentQueue<ProcessingQueueItem>();
        private DispatcherTimer timer;

        ProcessingQueue()
        {
            this.CompletedItems = new ObservableCollection<ProcessingQueueItem>();
            this.QueuedItems = new WaitingCollection(this);
            this.timer = new DispatcherTimer(
                TimeSpan.FromSeconds(1),
                DispatcherPriority.SystemIdle,
                (sender, args) =>
                {
                    foreach(var item in queue)
                    {
                        item.UpdateStatus();
                    }
                }, Application.Current.Dispatcher);
        }

        public ObservableCollection<ProcessingQueueItem> CompletedItems { get; private set; }

        public IEnumerable<ProcessingQueueItem> QueuedItems { get; private set; }

        class WaitingCollection : IEnumerable<ProcessingQueueItem>, INotifyCollectionChanged
        {
            private readonly ProcessingQueue owner;

            public WaitingCollection(ProcessingQueue owner)
            {
                this.owner = owner;
            }

            public void NotifyCollectionChanged()
            {
                if (CollectionChanged != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            if (CollectionChanged != null)
                            {
                                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                            }
                        }), null);
                }
            }

            public IEnumerator<ProcessingQueueItem> GetEnumerator()
            {
                return owner.queue.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return owner.queue.GetEnumerator();
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged;
        }
    }

    class ProcessingQueueItem : PropertyStore
    {
        private string status;
        private DateTime timeToRun;




        public string Status
        {
            get { return this.status; }
            set { SetValue(ref this.status, value, "Status"); }
        }

        public void Cancel()
        {
        }

        public void Retry()
        {
        }

        internal void UpdateStatus()
        {
            
        }
    }
}
