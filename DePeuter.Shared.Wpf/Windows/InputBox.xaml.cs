using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DePeuter.Shared.Wpf.Internal;

namespace DePeuter.Shared.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public string Value { get; private set; }

        public InputBox(string title, string defaultValue = null, CharacterCasing? characterCasing = null)
        {
            InitializeComponent();

            Title = title;
            tbInput.Text = string.Format("{0}", defaultValue);

            if (characterCasing != null)
            {
                tbInput.CharacterCasing = characterCasing.Value;    
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbInput.Focus();
        }

        public static string GetValue(string title, string defaultValue = null, CharacterCasing? characterCasing = null)
        {
            return Global.InvokeOnGuiThread(() =>
            {
                var f = new InputBox(title, defaultValue, characterCasing);
                f.ShowDialog();

                return f.Value;
            });
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            Value = tbInput.Text;

            Close();
        }
    }
}
