using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TTRider.uEpisodes.Properties;

namespace TTRider.uEpisodes.TVDatabase
{
    class CustomMapping
    {
        public string OriginalShowName { get; private set; }
        public string ShowName { get; private set; }

        public CustomMapping(string originalShowName, string showName)
        {
            this.OriginalShowName = string.Join(" ", FileData.GetWordset(originalShowName).ToList());
            this.ShowName = showName;
        }
    }

    class CustomMappingCollection : System.Collections.ObjectModel.KeyedCollection<string, CustomMapping>
    {
        private bool initializing;

        public CustomMappingCollection()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                this.initializing = true;

                if (!string.IsNullOrWhiteSpace(Settings.Default.ShowMappings))
                {
                    foreach (var kv in Settings.Default.ShowMappings
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries))
                        .Where(kv => kv.Length > 1)
                        .Where(kv => !this.Contains(kv[0])))
                    {
                        this.Add(new CustomMapping(kv[0], kv[1]));
                    }
                }
            }
            finally
            {
                this.initializing = false;
            }
        }

        private void SaveChanges()
        {
            if (initializing)
            {
                return;
            }
            var sb = new StringBuilder();
            foreach (var line in this)
            {
                sb.AppendLine(line.OriginalShowName + "\t" + line.ShowName);
            }
            Settings.Default.ShowMappings = sb.ToString();
            Settings.Default.Save();

        }

        protected override string GetKeyForItem(CustomMapping item)
        {
            if (item == null) return "";
            return item.OriginalShowName;
        }

        protected override void InsertItem(int index, CustomMapping item)
        {
            base.InsertItem(index, item);
            SaveChanges();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            SaveChanges();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            SaveChanges();
        }

        protected override void SetItem(int index, CustomMapping item)
        {
            base.SetItem(index, item);
            SaveChanges();
        }
    }
}
