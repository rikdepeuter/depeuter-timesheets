using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using DePeuter.Shared.Database;
using log4net;

namespace DePeuter.Timesheets
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

            //DatabaseProviders.RegisterProvider<NpgsqlProviderAttribute>();
            DatabaseProviders.RegisterProvider<MSSQLProviderAttribute>();
        }
    }
}