using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using DePeuter.Timesheets.Constants;

namespace DePeuter.Timesheets.Infrastructure.ViewModel
{
    public interface ITabViewModel
    {
        string HeaderText { get; }
    }

    public interface IShortcutKey
    {
        void ProcessShortcutKey(ProcessShortcutKeyEventArgs e);
    }

    public class ProcessShortcutKeyEventArgs : EventArgs
    {
        public ShortcutKey Key { get; private set; }
        public bool Handled;

        public ProcessShortcutKeyEventArgs(ShortcutKey key)
        {
            Key = key;
        }
    }
}