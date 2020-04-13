using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public static class FileSystemWatcherExtensions
{
    public static void Start(this FileSystemWatcher fsw)
    {
        fsw.EnableRaisingEvents = true;
    }

    public static void Stop(this FileSystemWatcher fsw)
    {
        fsw.EnableRaisingEvents = false;
    }
}