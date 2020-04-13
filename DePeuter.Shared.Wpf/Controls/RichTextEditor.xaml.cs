using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DePeuter.Shared.Wpf.Controls
{
    public partial class RichTextEditor : UserControl
    {
        /// <summary></summary>
        public static readonly DependencyProperty XamlProperty =
		  DependencyProperty.Register("Xaml", typeof(string), typeof(RichTextEditor),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary></summary>
        public static readonly DependencyProperty IsToolBarVisibleProperty =
		  DependencyProperty.Register("IsToolBarVisible", typeof(bool), typeof(RichTextEditor),
          new PropertyMetadata(true));

        /// <summary></summary>
        public static readonly DependencyProperty IsContextMenuEnabledProperty =
		  DependencyProperty.Register("IsContextMenuEnabled", typeof(bool), typeof(RichTextEditor),
          new PropertyMetadata(true));

        /// <summary></summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
		  DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(RichTextEditor),
          new PropertyMetadata(false));

        /// <summary></summary>
        //public static readonly DependencyProperty AvailableFontsProperty =
        //  DependencyProperty.Register("AvailableFonts", typeof(Collection<String>), typeof(RichTextEditor),
        //  new PropertyMetadata(new Collection<String>(
        //      new List<String>(4) 
        //      {
        //          "Arial",
        //          "Courier New",
        //          "Tahoma",
        //          "Times New Roman"
        //      }
        //)));


        private TextRange _textRange = null;

        //public bool AvailableFontsEnabled
        //{
        //    get { return bAvailableFonts.Visibility == Visibility.Visible; }
        //    set { bAvailableFonts.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        //}

        /// <summary></summary>
        public RichTextEditor()
        {
            InitializeComponent();
        }

        /// <summary></summary>
        public string Xaml
        {
            get { return GetValue(XamlProperty) as string; }
            set
            {
                SetValue(XamlProperty, value);
            }
        }

        /// <summary></summary>
        public bool IsToolBarVisible
        {
            get { return (GetValue(IsToolBarVisibleProperty) as bool? == true); }
            set
            {
                SetValue(IsToolBarVisibleProperty, value);
                //this.mainToolBar.Visibility = (value == true) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary></summary>
        public bool IsContextMenuEnabled
        {
            get
            {
                return (GetValue(IsContextMenuEnabledProperty) as bool? == true);
            }
            set
            {
                SetValue(IsContextMenuEnabledProperty, value);
            }
        }

        /// <summary></summary>
        public bool IsReadOnly
        {
            get { return (GetValue(IsReadOnlyProperty) as bool? == true); }
            set
            {
                SetValue(IsReadOnlyProperty, value);
                SetValue(IsToolBarVisibleProperty, !value);
                SetValue(IsContextMenuEnabledProperty, !value);
            }
        }

        /// <summary></summary>
        //public Collection<String> AvailableFonts
        //{
        //    get { return GetValue(AvailableFontsProperty) as Collection<String>; }
        //    set
        //    {
        //        SetValue(AvailableFontsProperty, value);
        //    }
        //}

        private void FontColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            this.mainRTB.Selection.ApplyPropertyValue(ForegroundProperty, e.NewValue.ToString(CultureInfo.InvariantCulture));
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.mainRTB != null && this.mainRTB.Selection != null)
                this.mainRTB.Selection.ApplyPropertyValue(FontFamilyProperty, e.AddedItems[0]);
        }

        private void insertLink_Click(object sender, RoutedEventArgs e)
        {
            this._textRange = new TextRange(this.mainRTB.Selection.Start, this.mainRTB.Selection.End);
            this.uriInputPopup.IsOpen = true;
            uriInput.Focus();
        }

        private void uriCancelClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this.uriInputPopup.IsOpen = false;
            this.uriInput.Text = string.Empty;
        }

        private void uriSubmitClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this.uriInputPopup.IsOpen = false;
            this.mainRTB.Selection.Select(this._textRange.Start, this._textRange.End);
            if(!string.IsNullOrEmpty(this.uriInput.Text))
            {
                this._textRange = new TextRange(this.mainRTB.Selection.Start, this.mainRTB.Selection.End);
                //try
                //{
                    var hlink = new Hyperlink(this._textRange.Start, this._textRange.End);
                    hlink.NavigateUri = new Uri(this.uriInput.Text, UriKind.RelativeOrAbsolute);
                //}
                //catch
                //{
                //    //MessageBus.Show(MessageType.Error, "Failed to set Hyperlink.");
                //    //MessageBox.Show("Failed to set Hyperlink.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
                this.uriInput.Text = string.Empty;
            }
            else
                this.mainRTB.Selection.ClearAllProperties();
        }

        private void uriInput_KeyPressed(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Enter:
                    this.uriSubmitClick(sender, e);
                    break;
                case Key.Escape:
                    this.uriCancelClick(sender, e);
                    break;
                default:
                    break;
            }
        }

        private void Hyperlink_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            var hyperlink = (Hyperlink)sender;
            if (hyperlink.NavigateUri == null)
            {
                return;
            }
            Process.Start(hyperlink.NavigateUri.ToString());
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if(!this.IsContextMenuEnabled == true)
                e.Handled = true;
        }

        private void MainToolBar_OnLoaded(object sender, RoutedEventArgs e)
        {
            var toolBar = (ToolBar)sender;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if(overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if(mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness();
            }
        }
    }
}