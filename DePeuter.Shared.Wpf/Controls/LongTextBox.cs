using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DePeuter.Shared.Wpf.Controls
{
    public class LongTextBox : TextBox, INotifyPropertyChanged
    {
        public int MaximumIntegerLength { get; set; }

        public static readonly DependencyProperty SupportNegativeProperty = DependencyProperty.Register("SupportNegative", typeof(bool), typeof(LongTextBox), new FrameworkPropertyMetadata(true));
        public bool SupportNegative
        {
            get
            {
                return (bool)GetValue(SupportNegativeProperty);
            }
            set
            {
                SetValue(SupportNegativeProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(long?), typeof(LongTextBox), new FrameworkPropertyMetadata(default(long?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ValuePropertyChangedCallback));
        private static void ValuePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((LongTextBox)sender).Value = (long?)e.NewValue;
        }

        public long? Value
        {
            get
            {
                return (long?)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
                OnPropertyChanged();

                var caretIndex = CaretIndex;
                Text = value == null ? string.Empty : ToFloatingPointString(value.Value);
                CaretIndex = caretIndex < Text.Length ? caretIndex : Text.Length;
            }
        }

        public LongTextBox()
        {
            MaximumIntegerLength = 9;

            KeyDown += LongTextBox_KeyDown;
            TextAlignment = TextAlignment.Right;
            TextChanged += LongTextBox_TextChanged;
        }

        void LongTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Text = LeaveOnlyNumbers(Text);

            if (string.IsNullOrEmpty(Text) || Text == "-")
            {
                SetValue(ValueProperty, null);
            }
            else
            {
                SetValue(ValueProperty, long.Parse(Text));
            }
        }

        public static string ToFloatingPointString(long value)
        {
            return ToFloatingPointString(value, NumberFormatInfo.CurrentInfo);
        }
        public static string ToFloatingPointString(long value, NumberFormatInfo formatInfo)
        {
            var res = value.ToString("0");
            return res;
        }

        void LongTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
            {
                if (SupportNegative && Text != null && !Text.Contains("-"))
                {
                    var caretIndex = CaretIndex;
                    Text = "-" + Text;
                    CaretIndex = caretIndex + 1;
                }
                e.Handled = true;
                return;
            }

            if (e.Key == Key.OemPlus || e.Key == Key.Add)
            {
                if (Text != null && Text.Contains("-"))
                {
                    var caretIndex = CaretIndex;
                    Text = Text.Substring(1);
                    CaretIndex = caretIndex - 1;
                }
                e.Handled = true;
                return;
            }

            var isDecimalSeperator = IsDecimalSeperator(e.Key);

            if (!IsActionKey(e.Key) && !IsNumberKey(e.Key) && isDecimalSeperator)
            {
                e.Handled = true;
                return;
            }

            if (Text != null)
            {
                var integerText = Text.Replace("-", "");
                if (integerText.Length == MaximumIntegerLength)
                {
                    e.Handled = true;
                }
            }
        }

        private bool IsDecimalSeperator(Key inKey)
        {
            return inKey == Key.Decimal || inKey == Key.OemComma || inKey == Key.OemPeriod;
        }

        private bool IsNumberKey(Key inKey)
        {
            return !((inKey < Key.D0 || inKey > Key.D9) && (inKey < Key.NumPad0 || inKey > Key.NumPad9));
        }

        private bool IsActionKey(Key inKey)
        {
            return inKey == Key.Delete || inKey == Key.Back || inKey == Key.Tab || inKey == Key.Return || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);
        }

        private string LeaveOnlyNumbers(string inString)
        {
            var tmp = inString;
            foreach (char c in inString)
            {
                if (!IsDigit(c) && ((!SupportNegative && c == '-') || (c != '-')))
                {
                    tmp = tmp.Replace(c.ToString(), "");
                }
            }
            return tmp;
        }

        public bool IsDigit(char c)
        {
            return (c >= '0' && c <= '9');
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        private void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                var msg = "Invalid property name: " + propertyName;

                throw new Exception(msg);
            }
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion
    }
}
