using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DePeuter.Shared.Wpf.MessageBoxWpf
{
    /// <summary>
    /// Interaction logic for MessageBoxWpfWindow.xaml
    /// </summary>
    public partial class MessageBoxWpfWindow : Window
    {
        public string Result { get; set; }

        public MessageBoxWpfWindow(string messageBoxText, string caption, MessageBoxImage image, IMessageBoxWpfButton[] buttons)
        {
            InitializeComponent();

            Title = caption;
            tbMessage.Text = messageBoxText;
            iIcon.Source = GetImageSource(image);
            if(iIcon.Source == null)
            {
                iIcon.Visibility = Visibility.Collapsed;
            }

            if(buttons != null)
            {
                foreach(var button in buttons)
                {
                    var b = new Button();
                    b.Padding = new Thickness(10, 0, 10, 0);
                    b.Margin = new Thickness(2, 0, 2, 0);

                    b.Content = button.Content;
                    b.Tag = button.Value;
                    b.ToolTip = button.Tooltip;

                    if (b.Content is string)
                    {
                        b.MinWidth = 75;
                    }

                    b.Click += b_Click;

                    wpButtons.Children.Add(b);
                }
            }
        }

        void b_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;

            Result = b.Tag.ToString();

            Close();
        }

        private ImageSource GetImageSource(MessageBoxImage image)
        {
            if(image == MessageBoxImage.Asterisk)
                return SystemIcons.Asterisk.ToImageSource();

            if(image == MessageBoxImage.Error)
                return SystemIcons.Error.ToImageSource();

            if(image == MessageBoxImage.Exclamation)
                return SystemIcons.Exclamation.ToImageSource();

            if(image == MessageBoxImage.Hand)
                return SystemIcons.Hand.ToImageSource();

            if(image == MessageBoxImage.Information)
                return SystemIcons.Information.ToImageSource();

            if(image == MessageBoxImage.None)
                return null;

            if(image == MessageBoxImage.Question)
                return SystemIcons.Question.ToImageSource();

            if(image == MessageBoxImage.Stop)
                return SystemIcons.Shield.ToImageSource();

            if(image == MessageBoxImage.Warning)
                return SystemIcons.Warning.ToImageSource();

            throw new NotImplementedException("MessageBoxImage." + image);
        }

        private void MessageBoxWpfWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            } else if (e.Key == Key.Enter)
            {
                if (wpButtons.Children.Count > 0)
                {
                    var b = (Button)wpButtons.Children[0];

                    b_Click(b, null);

                    e.Handled = true;
                }
            }
        }
    }   
}