using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace TTRider.uEpisodes
{
    class DispatchedCollection<T> : ObservableCollection<T> where T:class
    {
        public DispatchedCollection()
        { }

        public DispatchedCollection(List<T> list)
            : base(list) { }

        public DispatchedCollection(IEnumerable<T> enumeration)
            : base(enumeration) { }

        public IEnumerable<T> Add(IEnumerable<T> items, Func<T, T, bool> filter = null)
        {
            var ret = new List<T>();

            if (items != null)
            {
                ret.AddRange(items.Where(i => this.Add(i, filter)));
            }
            return ret;
        }

        public bool Add(T item, Func<T, T, bool> filter = null)
        {
            if (filter == null)
            {
                base.Add(item);
                return true;
            }
            if (!this.Any(f => filter(f, item)))
            {
                base.Add(item);
                return true;
            }
            return false;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                base.OnCollectionChanged(e);
            }
            else
            {
                var ea = e;
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action<NotifyCollectionChangedEventArgs>(base.OnCollectionChanged), ea);
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                base.OnPropertyChanged(e);
            }
            else
            {
                var ea = e;
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action<PropertyChangedEventArgs>(base.OnPropertyChanged), ea);
            }
        }
    }
}
