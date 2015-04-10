using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using Microsoft.Win32;

namespace TTRider.uEpisodes.Core.Properties
{


    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    sealed partial class Settings
    {
        private NetworkCredential uTorrentCredential;

        public Settings()
        {
            this.SettingChanging += this.SettingChangingEventHandler;
            this.SettingsSaving += this.SettingsSavingEventHandler;

            //this.EditableCopyToPattern = new EditableStringModel(this,"CopyToPattern");
            //this.EditableMoveToPattern = new EditableStringModel(this, "MoveToPattern");
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(sender, e);
            if (e.PropertyName == "CopyToPattern")
            {
                base.OnPropertyChanged(sender, new PropertyChangedEventArgs("CopyToPatternExample"));
            }
            if (e.PropertyName == "MoveToPattern")
            {
                base.OnPropertyChanged(sender, new PropertyChangedEventArgs("MoveToPatternExample"));
            }
            if (e.PropertyName == "AdditionalExtensions")
            {
                base.OnPropertyChanged(sender, new PropertyChangedEventArgs("AdditionalVideoFileExtensions"));
            }
        }

        

        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            //this.Save();
        }

        private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
        {
            // Add code to handle the SettingsSaving event here.
        }


        public string   uTorrentUserName
        {
            get
            {
                if (this.uTorrentCredential == null)
                {
                    if (string.IsNullOrWhiteSpace(this.uTorrentCredentials))
                    {
                        return null;
                    }

                    var nc = new NetworkCredential();
                    if (nc.InitializeFromDpApi(this.uTorrentCredentials))
                    {
                        this.uTorrentCredential = nc;
                    }
                    else
                    {
                        return null;
                    }
                }
                return this.uTorrentCredential.UserName;
            }
            set
            {
                if (this.uTorrentCredential == null)
                {
                    this.uTorrentCredential = new NetworkCredential();
                }
                this.uTorrentCredential.UserName = value ?? "";
                this.uTorrentCredentials = this.uTorrentCredential.StoreToDpApi();
            }
        }

        public string uTorrentPassword
        {
            get
            {
                if (this.uTorrentCredential == null)
                {
                    if (string.IsNullOrWhiteSpace(this.uTorrentCredentials))
                    {
                        return null;
                    }

                    var nc = new NetworkCredential();
                    if (nc.InitializeFromDpApi(this.uTorrentCredentials))
                    {
                        this.uTorrentCredential = nc;
                    }
                    else
                    {
                        return null;
                    }
                }
                return this.uTorrentCredential.Password;
            }
            set
            {
                if (this.uTorrentCredential == null)
                {
                    this.uTorrentCredential = new NetworkCredential();
                }
                this.uTorrentCredential.Password = value ?? "";
                this.uTorrentCredentials = this.uTorrentCredential.StoreToDpApi();
            }
        }

        public IEnumerable<string> GetVideoExtensions()
        {
            return GetSystemVideoExtensions().Union(
                string.IsNullOrWhiteSpace(this.AdditionalExtensions)
                    ? new string[0]
                    : this.AdditionalExtensions.Split(new[] {';', ',', ' ', '\r', '\n'},
                                                      StringSplitOptions.RemoveEmptyEntries))
                                      .Distinct(StringComparer.OrdinalIgnoreCase)
                                      .OrderBy(s => s);
        }

        public IEnumerable<string> GetSystemVideoExtensions()
        {
            var classesRoot = Registry.ClassesRoot;
            var names = classesRoot.GetSubKeyNames();
            return from t in names
                   let sk = classesRoot.OpenSubKey(t, RegistryKeyPermissionCheck.ReadSubTree)
                   where sk != null
                   let pVal = sk.GetValue("PerceivedType")
                   let cVal = sk.GetValue("Content Type")
                   where ((pVal != null)
                          && (pVal.ToString().Equals("video", StringComparison.OrdinalIgnoreCase))) ||
                         ((cVal != null)
                          && (cVal.ToString().StartsWith("video", StringComparison.OrdinalIgnoreCase)))
                   select t;
        }

        public IEnumerable<string> GetFileNamePatterns()
        {
            return this.FileNamePatterns.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<string> GetIgnoredDirectories()
        {
            return this.IgnoreDirectories.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);
        }
        
    }
}
