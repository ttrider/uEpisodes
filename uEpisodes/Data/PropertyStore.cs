using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO;
using System.Windows;
using System.Xml;
using System.Reflection;

namespace TTRider.uEpisodes.Data
{
    [DataContract(Name = "PropertyStore", Namespace = "http://schemas.ttrider.com/PropertyStore")]
    public class PropertyStore : IDisposable
                               , IExtensibleDataObject
                               , INotifyPropertyChanged
                               , INotifyPropertyChanging
    {
        #region Property Change


        protected virtual void OnPropertyChanged(string name, params string[] names)
        {
            if (PropertyChanged != null)
            {
                if ((Application.Current == null) || (Application.Current.Dispatcher == null) || Application.Current.Dispatcher.CheckAccess())
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(name));

                    foreach (string t in names)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(t));
                    }
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        new Action(() =>
                            {
                                if (PropertyChanged != null)
                                {
                                    PropertyChanged(this, new PropertyChangedEventArgs(name));

                                    foreach (string t in names)
                                    {
                                        PropertyChanged(this, new PropertyChangedEventArgs(t));
                                    }
                                }
                            }), null);
                }
            }
        }

        protected virtual void OnPropertyChanging(string name)
        {
            if (PropertyChanging != null)
            {
                if ((Application.Current == null) || (Application.Current.Dispatcher == null) || Application.Current.Dispatcher.CheckAccess())
                {
                    PropertyChanging(this, new PropertyChangingEventArgs(name));
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        new Action(() =>
                            {
                                if (PropertyChanging != null)
                                {
                                    PropertyChanging(this, new PropertyChangingEventArgs(name));
                                }
                            }), null);
                }
            }
        }



        protected void SetValue<T>(ref T field, T value, params string[] names)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                foreach (var name in names)
                {
                    OnPropertyChanging(name);
                }
                field = value;
                foreach (var name in names)
                {
                    OnPropertyChanged(name);
                }
                this.IsDirty = true;
            }
        }

        protected void SetValue<T>(ref T field, T value, Action<T, T> action, params string[] names)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                foreach (var name in names)
                {
                    OnPropertyChanging(name);
                }
                var original = field;
                field = value;
                if (action != null)
                {
                    action(original, value);
                }
                foreach (var name in names)
                {
                    OnPropertyChanged(name);
                }
                this.IsDirty = true;
            }
        }

        bool isDirty;
        public virtual bool IsDirty
        {
            get { return this.isDirty; }
            set
            {
                if (this.isDirty != value)
                {
                    OnPropertyChanging("IsDirty");
                    isDirty = value;
                    OnPropertyChanged("IsDirty");
                    if (DirtyChanged != null)
                    {
                        DirtyChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        public event EventHandler DirtyChanged;

        #endregion Property Change

        #region Dispose

        ~PropertyStore()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

            this.isDisposed = true;
            if (Disposed != null)
            {
                Disposed(this, EventArgs.Empty);
            }
        }

        bool isDisposed;
        public bool IsDisposed
        {
            get { return this.isDisposed; }
        }

        public event EventHandler Disposed;
        #endregion Dispose

        #region IExtensibleDataObject
        ExtensionDataObject extensionDataObjectValue;
        ExtensionDataObject IExtensibleDataObject.ExtensionData
        {
            get { return this.extensionDataObjectValue; }
            set { this.extensionDataObjectValue = value; }
        }
        #endregion IExtensibleDataObject

    }

    public class EditableAttribute : Attribute
    {
    }

    [DataContract(Name = "PropertyStore", Namespace = "http://schemas.ttrider.com/PropertyStore")]
    public class EditablePropertyStore : PropertyStore
                                       , IEditableObject
    {
        byte[] savedData;
        DataContractSerializer serializer;

        #region IEditableObject Members

        public virtual void BeginEdit()
        {
            if (savedData == null)
            {
                var values = new List<object>();
                var knownTypes = new HashSet<Type>();

                foreach (var prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (Attribute.IsDefined(prop, typeof(EditableAttribute)))
                    {
                        if (prop.CanRead && prop.CanWrite)
                        {
                            values.Add(prop.GetValue(this, null));
                            knownTypes.Add(prop.PropertyType);
                        }
                    }
                }

                foreach (var prop in GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (Attribute.IsDefined(prop, typeof(EditableAttribute)))
                    {
                        values.Add(prop.GetValue(this));
                        knownTypes.Add(prop.FieldType);
                    }
                }

                this.serializer = new DataContractSerializer(typeof(List<object>), knownTypes);
                var ms = new MemoryStream();
                using (XmlDictionaryWriter w = XmlDictionaryWriter.CreateBinaryWriter(ms))
                {
                    serializer.WriteObject(w, values);
                    w.Flush();

                    this.savedData = new byte[ms.Length];
                }
                Array.Copy(ms.GetBuffer(), this.savedData, this.savedData.Length);
            }
        }

        public virtual void CancelEdit()
        {
            if ((this.savedData != null) && (this.serializer != null))
            {
                var ms = new MemoryStream(this.savedData);
                using (XmlDictionaryReader w = XmlDictionaryReader.CreateBinaryReader(ms, XmlDictionaryReaderQuotas.Max))
                {
                    var values = (List<object>)(serializer.ReadObject(w));
                    int index = 0;

                    foreach (var prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (Attribute.IsDefined(prop, typeof(EditableAttribute)))
                        {
                            if (prop.CanRead && prop.CanWrite)
                            {
                                prop.SetValue(this, values[index++], null);
                            }
                        }
                    }

                    foreach (var prop in GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (Attribute.IsDefined(prop, typeof(EditableAttribute)))
                        {
                            prop.SetValue(this, values[index++]);
                        }
                    }
                }

                this.savedData = null;
                this.serializer = null;
            }
        }

        public virtual void EndEdit()
        {
            this.savedData = null;
        }

        #endregion
    }
}
