using System;
using System.Runtime.CompilerServices;

namespace GeoIT_Infrastructure.Advanced.MessageBox
{
	/// <summary>
	/// Internal DataStructure used to represent a button
	/// </summary>
	public class MessageBoxExButton
	{
		/// <summary>
		/// Gets or Sets the text of the button
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Gets or Sets the return value when this button is clicked
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Gets or Sets the tooltip that is displayed for this button
		/// </summary>
		public string HelpText { get; set; }

		/// <summary>
		/// Gets or Sets wether this button is a cancel button. i.e. the button
		/// that will be assumed to have been clicked if the user closes the message box
		/// without pressing any button.
		/// </summary>
		public bool IsCancelButton { get; set; }

	    public MessageBoxExButton()
	    {
	    }

	    public MessageBoxExButton(MessageBoxExButtons button)
	    {
            var buttonText = MessageBoxExManager.GetLocalizedString(button.ToString());
            if(buttonText == null)
            {
                buttonText = button.ToString();
            }

            var buttonVal = button.ToString();

	        Text = buttonText;
	        Value = buttonVal;

            if(button == MessageBoxExButtons.Cancel)
            {
                IsCancelButton = true;
            }
	    }

        public MessageBoxExButton(string text)
        {
            if(text == null)
                throw new ArgumentNullException("text", "Text of a button cannot be null");

            Text = text;
            Value = text;
        }

	    public MessageBoxExButton(string text, string value)
	    {
            if(text == null)
                throw new ArgumentNullException("text", "Text of a button cannot be null");

            if(value == null)
                throw new ArgumentNullException("value", "Value of a button cannot be null");

	        Text = text;
	        Value = value;
	    }

        public MessageBoxExButton(string text, string value, string helpText, bool isCancelButton) : this(text, value)
        {
            HelpText = helpText;
            IsCancelButton = isCancelButton;
        }

        public static MessageBoxExButton Create(MessageBoxExButtons button)
        {
            return new MessageBoxExButton(button);
        }

        public static MessageBoxExButton Create(string text)
        {
            return new MessageBoxExButton(text);
        }

        public static MessageBoxExButton Create(string text, string value)
        {
            return new MessageBoxExButton(text, value);
        }

	    public static MessageBoxExButton Create(string text, string value, string helpText, bool isCancelButton)
	    {
	        return new MessageBoxExButton(text, value, helpText, isCancelButton);
	    }
	}
}
