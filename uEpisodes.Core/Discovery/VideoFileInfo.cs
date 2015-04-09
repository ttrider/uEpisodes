using System;
using System.Collections.Generic;
using System.Linq;

namespace TTRider.uEpisodes.Core.Discovery
{
    public class VideoFileInfo
    {
        internal VideoFileInfo(string path)
        {
            this.VideoIdSet = new List<VideoId>();
            this.Path = path;
            this.FileParts = path.Split(new[]
            {
                '\\',
                '/'
            }, StringSplitOptions.RemoveEmptyEntries).Reverse().ToList();
        }

        public string Path { get; private set; }
        public List<string> FileParts { get; private set; }

        public string FileName
        {
            get { return this.FileParts[0]; }
        }

        public List<VideoId> VideoIdSet { get; private set; }
    }
}