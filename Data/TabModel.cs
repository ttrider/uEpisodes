using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace TTRider.uEpisodes.Data
{
    class TabModel : DependencyObject, INotifyPropertyChanged
    {
        public string Title { get; set; }

        public ICollection Collection
        {
            get { return (ICollection)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Collection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.Register("Collection", typeof(ICollection), typeof(TabModel), new PropertyMetadata(null));



        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(TabModel), new PropertyMetadata(null));




        public object Commands
        {
            get { return GetValue(CommandsProperty); }
            set { SetValue(CommandsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Commands.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandsProperty =
            DependencyProperty.Register("Commands", typeof(object), typeof(TabModel), new PropertyMetadata(null));




        public int? Count
        {
            get
            {
                return Collection != null ? (Collection.Count != 0) ? (int?)Collection.Count : null : null;
            }
        }


        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == CollectionProperty)
            {
                var cs = e.NewValue as INotifyCollectionChanged;
                if (cs != null)
                {
                    cs.CollectionChanged += (ss, ee) =>
                        {
                            if (PropertyChanged != null)
                            {
                                PropertyChanged(this, new PropertyChangedEventArgs("Count"));
                            }
                        };
                }


            }
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
