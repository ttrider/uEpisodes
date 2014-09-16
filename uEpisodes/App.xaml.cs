using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Windows;
using TTRider.uEpisodes.Data;
using TTRider.uEpisodes.Properties;

namespace TTRider.uEpisodes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        

        protected override void OnStartup(StartupEventArgs e)
        {
            Trace.TraceInformation("Starting up with arguments: {0}", string.Join(" ", e.Args.Select(o => (o ?? "").ToString(CultureInfo.InvariantCulture))));

            try
            {
                RegisterChannel("Server");
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(MsgPack), "uEpisodes", WellKnownObjectMode.SingleCall);

                Trace.TraceInformation("Running as Server");
                InvokeCommand(e.Args);
            }
            catch (Exception)
            {
                Trace.TraceInformation("Server is already running");
                RegisterChannel("Client");
                var proxy = (MsgPack)Activator.GetObject(typeof(MsgPack), "ipc://Server/uEpisodes");
                Trace.TraceInformation("Sending arguments to server");
                proxy.CallApplication(Environment.CurrentDirectory, e.Args);
                Trace.TraceInformation("Exiting");
                this.Shutdown();
            }


            base.OnStartup(e);
        }

        private void InvokeCommand(string[] args)
        {
            Trace.TraceInformation("Queueing command with arguments: {0}", string.Join(" ", args.Select(o => (o ?? "").ToString(CultureInfo.InvariantCulture))));

            Dispatcher.BeginInvoke(new Action(() =>
            {
                Trace.TraceInformation("Processing command with arguments: {0}", string.Join(" ", args.Select(o => (o ?? "").ToString(CultureInfo.InvariantCulture))));

                var fileset = new HashSet<string>();
                HashSet<string> copySet = null;
                HashSet<string> moveSet = null;

                for (var i = 0; i < args.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(args[i]))
                    {
                        switch (args[i].ToLower())
                        {
                            case "-copy":
                                if (copySet == null)
                                {
                                    copySet = new HashSet<string>();
                                }
                                if ((i < args.Length - 1) && (!args[i + 1].StartsWith("-")))
                                {
                                    copySet.Add(args[i + 1]);
                                    i++;
                                }
                                break;
                            case "-move":
                                if (moveSet == null)
                                {
                                    moveSet = new HashSet<string>();
                                }
                                if ((i < args.Length - 1) && (!args[i + 1].StartsWith("-")))
                                {
                                    moveSet.Add(args[i + 1]);
                                    i++;
                                }
                                break;
                            case "-show":
                                this.MainWindow.Show();
                                this.MainWindow.Activate();
                                break;
                            case "-hide":
                                this.MainWindow.WindowState = WindowState.Minimized;
                                break;
                            default:
                                fileset.Add(args[i]);
                                break;
                        }
                    }
                }

                Trace.TraceInformation("Copy commands: '{0}'", copySet == null ? "None" : string.Join(", ", copySet.Select(o => (o ?? "").ToString(CultureInfo.InvariantCulture))));
                Trace.TraceInformation("Move commands: '{0}'", moveSet == null ? "None" : string.Join(", ", moveSet.Select(o => (o ?? "").ToString(CultureInfo.InvariantCulture))));
                Trace.TraceInformation("File Sets: '{0}'", fileset.Count == 0 ? "None" : string.Join(", ", fileset.Select(o => (o ?? "").ToString(CultureInfo.InvariantCulture))));

                var model = AppModel.Current;

                foreach (var rawfile in fileset)
                {
                    var file = rawfile.Trim(' ', '\t', '\'', '"', '\r', '\n');

                    if (file.IndexOfAny(new[] { '*', '?' }) == -1)
                    {
                        // no wildcards - use standard method
                        model.OpenFileSystemItems(file, copySet, moveSet);
                    }
                    else
                    {
                        var dirName = Path.GetDirectoryName(file);
                        if (dirName != null)
                        {
                            var dir = Path.GetFullPath(dirName);
                            var pattern = Path.GetFileName(file);
                            if (pattern != null)
                            {
                                foreach (var item in Directory.EnumerateFiles(dir, pattern))
                                {
                                    model.AddFile(item, copySet, moveSet);
                                }
                            }
                        }
                    }
                }
            }));
        }

        static void RegisterChannel(string name)
        {
            var properties = new Dictionary<string, string>
                {
                    {"name", name},
                    {"portName", name},
                    {"typeFilterLevel", "Full"}
                };
            var channel = new IpcChannel(properties,
                new BinaryClientFormatterSinkProvider(properties, null),
                new BinaryServerFormatterSinkProvider(properties, null));
            ChannelServices.RegisterChannel(channel, false);
        }

        class MsgPack : MarshalByRefObject
        {
            public void CallApplication(string currentDirectory, string[] args)
            {
                Environment.CurrentDirectory = currentDirectory;

                var app = ((App)Current);

                app.Dispatcher.BeginInvoke(new Action(() => app.InvokeCommand(args)));

            }
        }
    }
}
