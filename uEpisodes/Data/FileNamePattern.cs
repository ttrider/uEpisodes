using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TTRider.uEpisodes.Data
{
    class FileNamePattern
    {
        static readonly Regex showRegex = new Regex(@"\{show", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex seasonRegex = new Regex(@"\{season", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex episodeRegex = new Regex(@"\{episode", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex titleRegex = new Regex(@"\{title", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string ApplyFilePattern(string pattern, string show, int season, int episode, string title)
        {
            var args = new List<object>();

            if (showRegex.IsMatch(pattern))
            {
                pattern = showRegex.Replace(pattern, "{" + args.Count.ToString(CultureInfo.InvariantCulture));
                args.Add(show);
            }
            if (seasonRegex.IsMatch(pattern))
            {
                pattern = seasonRegex.Replace(pattern, "{" + args.Count.ToString(CultureInfo.InvariantCulture));
                args.Add(season);
            }
            if (episodeRegex.IsMatch(pattern))
            {
                pattern = episodeRegex.Replace(pattern, "{" + args.Count.ToString(CultureInfo.InvariantCulture));
                args.Add(episode);
            }
            if (titleRegex.IsMatch(pattern))
            {
                pattern = titleRegex.Replace(pattern, "{" + args.Count.ToString(CultureInfo.InvariantCulture));
                args.Add(title);
            }

            var newName = string.Format(pattern, args.ToArray());

            var dir = CleanupName(Path.GetDirectoryName(newName), Path.GetInvalidPathChars());
            var file = CleanupName(Path.GetFileName(newName), Path.GetInvalidFileNameChars());

            return Path.Combine(dir, file);
        }

        private static string CleanupName(string name, char[] invalidChars)
        {
            var sb = new StringBuilder(name);
            foreach (var t in invalidChars)
            {
                sb.Replace(t, ' ');
            }
            return sb.ToString();
        }
        
        public static string ApplyFilePattern(string pattern, string filePath, string show, int season, int episode, string title, bool createDirectory = false)
        {
            var fileName = ApplyFilePattern(pattern, show, season, episode, title);

            var dir = Path.GetDirectoryName(filePath);
            if (dir == null) throw new DirectoryNotFoundException(filePath);

            var newFileName = Path.Combine(dir, fileName) + Path.GetExtension(filePath);

            if (createDirectory)
            {
                dir = Path.GetDirectoryName(newFileName);
                if (dir == null) throw new DirectoryNotFoundException(filePath);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            return newFileName;
        }
    }
}
