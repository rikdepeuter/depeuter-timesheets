using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DePeuter.Shared.Wpf.Internal;

namespace DePeuter.Shared.Wpf.MessageBoxWpf
{
    public class MessageBox
    {
        private static MessageBoxResult[] ConvertToMessageBoxResults(MessageBoxButton button)
        {
            var buttons = new List<MessageBoxResult>();

            switch(button)
            {
                case MessageBoxButton.OK:
                    buttons.Add(MessageBoxResult.OK);
                    break;
                case MessageBoxButton.OKCancel:
                    buttons.Add(MessageBoxResult.OK);
                    buttons.Add(MessageBoxResult.Cancel);
                    break;
                case MessageBoxButton.YesNo:
                    buttons.Add(MessageBoxResult.Yes);
                    buttons.Add(MessageBoxResult.No);
                    break;
                case MessageBoxButton.YesNoCancel:
                    buttons.Add(MessageBoxResult.Yes);
                    buttons.Add(MessageBoxResult.No);
                    buttons.Add(MessageBoxResult.Cancel);
                    break;
                default:
                    throw new NotImplementedException("MessageBoxButton." + button);
            }

            return buttons.ToArray();
        }
        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            return Show(messageBoxText, caption, MessageBoxImage.Information, ConvertToMessageBoxResults(MessageBoxButton.OK));
        }
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            return Show(messageBoxText, caption, image, ConvertToMessageBoxResults(button));
        }
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxImage image, MessageBoxButton button)
        {
            return Show(messageBoxText, caption, image, ConvertToMessageBoxResults(button));
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxImage image, params MessageBoxResult[] buttons)
        {
            return Global.InvokeOnGuiThread(() =>
            {
                var form = new MessageBoxWpfWindow(messageBoxText, caption, image, buttons.Select(MessageBoxWpfButton.Create).ToArray());
                form.ShowDialog();

                if(form.Result == null)
                {
                    return MessageBoxResult.None;
                }

                return form.Result.ToEnum<MessageBoxResult>();
            });
        }

        public static string Show(string messageBoxText, string caption, MessageBoxImage image, params IMessageBoxWpfButton[] buttons)
        {
            return Global.InvokeOnGuiThread(() =>
            {
                var form = new MessageBoxWpfWindow(messageBoxText, caption, image, buttons.ToArray());
                form.ShowDialog();

                return form.Result;
            });
        }
    }
}
