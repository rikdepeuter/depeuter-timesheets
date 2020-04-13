using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using DePeuter.Timesheets.Constants;
using DePeuter.Timesheets.Database.Services;
using DePeuter.Timesheets.Infrastructure.ViewModel;

namespace DePeuter.Timesheets
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel { get { return (MainWindowViewModel)DataContext; } }

        private bool _maximizeOnNextStateChanged;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();

            var startupMinimized = DePeuterRegistry.Get("Timesheets", "StartupMinimized") as string;
            if(!string.IsNullOrEmpty(startupMinimized))
            {
                try
                {
                    if (bool.Parse(startupMinimized))
                    {
                        WindowState = WindowState.Minimized;
                        _maximizeOnNextStateChanged = true;
                    }
                }
                catch(Exception ex)
                {
                    ViewModel.HandleException(ex);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using(var db = new UpdaterDb())
                {
                    db.Update();

                    //var migrationConfig = db.GetConfiguration("Npgsql.Migrated");
                    //if (string.IsNullOrEmpty(migrationConfig.Value))
                    //{
                    //    using(var migration = new NpgsqlMigrationContext())
                    //    {
                    //        migration.Migrate(db);
                    //    }

                    //    migrationConfig.Value = true.ToString();
                    //    db.Save(migrationConfig);
                    //}
                }

                ViewModel.RaiseLoaded(sender, e);
            }
            catch(Exception ex)
            {
                ViewModel.HandleException(ex);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ProcessShortcutKeyEventArgs keyEventArgs = null;

            if(e.Key == Key.W && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                keyEventArgs = new ProcessShortcutKeyEventArgs(ShortcutKey.Close);
            }
            else if(e.Key == Key.Delete)
            {
                keyEventArgs = new ProcessShortcutKeyEventArgs(ShortcutKey.Delete);
            }
            else if(e.Key == Key.Escape)
            {
                keyEventArgs = new ProcessShortcutKeyEventArgs(ShortcutKey.Exit);
            }
            else if(e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                keyEventArgs = new ProcessShortcutKeyEventArgs(ShortcutKey.Find);
            }
            else if(e.Key == Key.N && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                keyEventArgs = new ProcessShortcutKeyEventArgs(ShortcutKey.New);
            }
            else if(e.Key == Key.F5)
            {
                keyEventArgs = new ProcessShortcutKeyEventArgs(ShortcutKey.Refresh);
            }
            else if(e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                keyEventArgs = new ProcessShortcutKeyEventArgs(ShortcutKey.Save);
            }
            else if(e.Key == Key.Q && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                keyEventArgs = new ProcessShortcutKeyEventArgs(ShortcutKey.SaveAndClose);
            }

            if(keyEventArgs != null)
            {
                ViewModel.HandleShortcutKey(keyEventArgs);
                if(keyEventArgs.Handled)
                {
                    e.Handled = true;
                }
            }
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if (_maximizeOnNextStateChanged)
            {
                WindowState = WindowState.Maximized;
                _maximizeOnNextStateChanged = false;
            }
        }
    }
}
