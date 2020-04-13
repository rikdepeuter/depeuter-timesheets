using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GeoIT_Infrastructure.Controls
{
    public enum InputValidationMode
    {
        Both,
        Integer,
        Double,
        Letter
    }

    public class TextBox2 : TextBox
    {
        private InputValidationMode _mode = InputValidationMode.Both;
        public InputValidationMode Mode { get { return _mode; } set { _mode=value; } }

        private bool _supportResetButton = true;
        public bool SupportResetButton
        {
            get { return _supportResetButton; }
            set { _supportResetButton = value; }
        }

        public int? IntegerValue
        {
            get
            {
                try
                {
                    return int.Parse(Text);
                }
                catch { }
                return null;
            }
            set { Text = value == null ? string.Empty : value.ToString(); }
        }

        public double? DoubleValue
        {
            get
            {
                try
                {
                    return double.Parse(Text);
                }
                catch { }
                return null;
            }
            set { Text = value == null ? string.Empty : value.ToString(); }
        }

        void TextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch(Mode)
            {
                case InputValidationMode.Integer:
                    e.Handled = e.KeyChar != '\b' && !char.IsDigit(e.KeyChar);
                    break;
                case InputValidationMode.Double:
                    if(Text.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) &&
                        e.KeyChar == char.Parse(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                    {
                        e.Handled = true;
                    }
                    else
                    {
                        e.Handled = e.KeyChar != '\b' && e.KeyChar != char.Parse(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) && !char.IsDigit(e.KeyChar);
                    }
                    break;
                case InputValidationMode.Letter:
                    e.Handled = e.KeyChar != '\b' && !char.IsLetter(e.KeyChar);
                    break;
            }
        }

        public event EventHandler OnEnterPressed;
        public event EventHandler OnReset;

        public delegate object TextChangedAsyncHandler(string text);
        public event TextChangedAsyncHandler TextChangedAsync;

        public delegate void TextChangedAsyncCompletedHandler(object sender, object result);
        public event TextChangedAsyncCompletedHandler TextChangedAsyncCompleted;

        private readonly Button _bReset = new Button();

        public System.Drawing.Image ResetImage { get { return _bReset.Image; } set { _bReset.Image = value; } }
        public System.Drawing.Size ResetSize { get { return _bReset.Size; } set { _bReset.Size = value; } }
        public int ResetXOffset { get; set; }
        public int ResetYOffset { get; set; }

        public TextBox2()
            : base()
        {
            previousCursor = Cursor;

            ResetXOffset = -2;
            ResetYOffset = -3;

            _bReset.Parent = this;
            _bReset.Visible = false;
            _bReset.TabStop = false;

            _bReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            _bReset.ForeColor = System.Drawing.Color.Transparent;
            _bReset.Size = new System.Drawing.Size(20, 20);
            _bReset.TabIndex = this.TabIndex + 1;
            _bReset.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            _bReset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            _bReset.UseVisualStyleBackColor = true;
            _bReset.Click += new System.EventHandler(this.bReset_Click);
            _bReset.MouseEnter += new EventHandler(bReset_MouseEnter);
            _bReset.MouseLeave += new EventHandler(bReset_MouseLeave);

            BreakTextBox_Resize(null, null);

            Resize += new EventHandler(BreakTextBox_Resize);
            TextChanged += new EventHandler(BreakTextBox_TextChanged);
            KeyDown += new KeyEventHandler(BreakTextBox_KeyDown);
            KeyPress += new KeyPressEventHandler(TextBox2_KeyPress);
        }

        void BreakTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                this.Clear();
                if(OnReset != null)
                    OnReset(this, EventArgs.Empty);
            }
            if(e.KeyCode == Keys.Enter)
            {
                if(OnEnterPressed != null)
                {
                    Focus();
                    OnEnterPressed(this, EventArgs.Empty);
                }
            }
        }

        private Cursor previousCursor;
        void bReset_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = previousCursor;
        }
        void bReset_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private DateTime latestAsync;
        void BreakTextBox_TextChanged(object sender, EventArgs e)
        {
            _bReset.Visible = SupportResetButton && this.Text != string.Empty;

            if(TextChangedAsync != null)
            {
                var utcNow = DateTime.UtcNow;
                latestAsync = utcNow;
                var bw = new BackgroundWorker();
                bw.DoWork += DoWorkEventHandler;
                bw.RunWorkerCompleted += RunWorkerCompletedEventHandler;
                bw.RunWorkerAsync(new object[] { utcNow, Text });
            }
        }

        private void DoWorkEventHandler(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var res = (object[])e.Argument;
            res[1] = TextChangedAsync((string)res[1]);
            e.Result = res;
        }

        private void RunWorkerCompletedEventHandler(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if(e.Error != null)
            {
                TextChangedAsyncCompleted(this, e.Error);
                return;
            }

            var res = (object[])e.Result;
            var startTime = (DateTime)res[0];
            if(startTime != latestAsync) return;

            if(TextChangedAsyncCompleted != null)
            {
                TextChangedAsyncCompleted(this, res[1]);
            }
        }

        void BreakTextBox_Resize(object sender, EventArgs e)
        {
            _bReset.Location = new System.Drawing.Point(this.Width - _bReset.Size.Width + ResetXOffset, ResetYOffset);
        }

        private void bReset_Click(object sender, EventArgs e)
        {
            this.Clear();
            if(OnReset != null)
                OnReset(this, EventArgs.Empty);
            this.Focus();
        }
    }
}
