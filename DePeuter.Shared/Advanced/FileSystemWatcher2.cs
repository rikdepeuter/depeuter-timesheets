//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;

//namespace System.IO
//{
//    public static class FileSystemWatcher2
//    {
//        public static FileSystemWatcher Watch(string path, string filter, bool includeSubdirectories, Action<FileSystemWatcher, FileSystemEventArgs> onCreated)
//        {
//            var fsw = new FileSystemWatcher();
//            fsw.Path = path;
//            fsw.IncludeSubdirectories = includeSubdirectories;
//            fsw.Filter = filter;
//            var isBusy = new List<string>();

//            if (onCreated != null)
//            {
//                fsw.Created += (object sender, FileSystemEventArgs e) =>
//                {
//                    if (isBusy.Contains(e.FullPath)) return;

//                    isBusy.Add(e.FullPath);
//                    try
//                    {
//                        onCreated((FileSystemWatcher)sender, e);
//                    }
//                    catch (Exception ex)
//                    {
//                        MessageBus.Show(ex);
//                    }
//                    finally
//                    {
//                        isBusy.Remove(e.FullPath);
//                    }
//                };
//            }

//            fsw.Start();

//            return fsw;
//        }

//        private static Dictionary<string, int> fileDelays = new Dictionary<string, int>();
//        private static Dictionary<string, DateTime> fileOnChangedEvents = new Dictionary<string, DateTime>();

//        public static FileSystemWatcher Watch(string file, Action<FileSystemWatcher, FileSystemEventArgs> onChanged, int delayMilliSeconds = 500)
//        {
//            if (fileDelays.ContainsKey(file))
//                fileDelays.Remove(file);
//            fileDelays.Add(file, delayMilliSeconds);

//            if (fileOnChangedEvents.ContainsKey(file))
//                fileOnChangedEvents.Remove(file);
//            fileOnChangedEvents.Add(file, DateTime.Now);

//            var fsw = new FileSystemWatcher();
//            fsw.Path = file.Substring(0, file.LastIndexOf('\\'));
//            fsw.IncludeSubdirectories = false;
//            fsw.Filter = file.Substring(file.LastIndexOf('\\') + 1);

//            fsw.Changed += (object sender, FileSystemEventArgs e) =>
//            {
//                System.Threading.Thread.Sleep(500);

//                var previousEventTime = fileOnChangedEvents[e.FullPath];
//                var difference = (DateTime.Now - previousEventTime).Milliseconds > fileDelays[e.FullPath];
//                if (difference)
//                {
//                    fileOnChangedEvents[e.FullPath] = DateTime.Now;
//                    try
//                    {
//                        onChanged((FileSystemWatcher)sender, e);
//                    }
//                    catch (Exception ex)
//                    {
//                        MessageBus.Show(ex);
//                    }
//                }
//            };

//            fsw.Start();

//            return fsw;
//        }
//    }
//}