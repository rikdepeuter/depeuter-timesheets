using System.Windows;

namespace DePeuter.Shared.Wpf.MessageBoxWpf
{
    public interface IMessageBoxWpfButton
    {
        string Value { get; set; }
        object Content { get; set; }
        object Tooltip { get; set; }
    }
    public class MessageBoxWpfButton : IMessageBoxWpfButton
    {
        public string Value { get; set; }
        public object Content { get; set; }
        public object Tooltip { get; set; }

        public MessageBoxWpfButton(object content)
            : this(content.ToString(), content)
        {
        }
        public MessageBoxWpfButton(MessageBoxResult value, object content = null, object tooltip = null)
            : this(value.ToString(), content ?? value.ToString(), tooltip)
        {
        }
        public MessageBoxWpfButton(string value, object content, object tooltip = null)
        {
            Value = value;
            Content = content;
            Tooltip = tooltip;
        }

        public static IMessageBoxWpfButton Create(object content)
        {
            return new MessageBoxWpfButton(content);
        }
        public static IMessageBoxWpfButton Create(MessageBoxResult value)
        {
            return new MessageBoxWpfButton(value);
        }
        //public static IMessageBoxWpfButton Create(MessageBoxResult value, object content, object tooltip = null)
        //{
        //    return new MessageBoxWpfButton(value, content, tooltip);
        //}
        public static IMessageBoxWpfButton Create(string value, object content, object tooltip = null)
        {
            return new MessageBoxWpfButton(value, content, tooltip);
        }
    }
}
