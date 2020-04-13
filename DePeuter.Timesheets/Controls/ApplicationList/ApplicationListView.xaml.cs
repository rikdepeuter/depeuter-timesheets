using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DePeuter.Timesheets.Controls.ApplicationList
{
    /// <summary>
    /// Interaction logic for ApplicationListView.xaml
    /// </summary>
    public partial class ApplicationListView : UserControl
    {
        private ApplicationListViewModel ViewModel { get { return (ApplicationListViewModel)DataContext; } }

        public ApplicationListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tbFilter.Focus();
        }

        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                ViewModel.UpdateFilesForSelectedApplication(files);
            }
        }
    }
}