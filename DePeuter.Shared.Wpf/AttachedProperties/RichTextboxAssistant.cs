using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace DePeuter.Shared.Wpf.AttachedProperties
{
    /// <summary></summary>
    public static class RichTextBoxAssistant
    {
        /// <summary></summary>
        public static readonly DependencyProperty BoundDocumentXamlProperty =
           DependencyProperty.RegisterAttached("BoundDocumentXaml", typeof(string), typeof(RichTextBoxAssistant),
           new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundDocumentXamlChanged));

        private static readonly object IgnoreOnBoundDocumentXamlChanged = new object();

        private static void OnBoundDocumentXamlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as RichTextBox;

            if(box == null)
                return;

            if(box.Tag == IgnoreOnBoundDocumentXamlChanged)
            {
                return;
            }

            RemoveEventHandler(box);

            var xaml = GetBoundDocumentXaml(d);

            try
            {
                box.Document.LoadXaml(xaml);
            }
            finally
            {
                AttachEventHandler(box);
            }
        }

        private static void RemoveEventHandler(RichTextBox box)
        {
            var binding = BindingOperations.GetBinding(box, BoundDocumentXamlProperty);
            
            if(binding != null)
            {
                if(binding.UpdateSourceTrigger == UpdateSourceTrigger.Default || binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
                {
                    box.LostFocus -= HandleLostFocus;
                }
                else
                {
                    box.TextChanged -= HandleTextChanged;
                }
            }
        }

        private static void AttachEventHandler(RichTextBox box)
        {
            var binding = BindingOperations.GetBinding(box, BoundDocumentXamlProperty);

            if(binding != null)
            {
                if(binding.UpdateSourceTrigger == UpdateSourceTrigger.Default || binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
                {
                    box.LostFocus += HandleLostFocus;
                }
                else
                {
                    box.TextChanged += HandleTextChanged;
                }
            }
        }

        private static void HandleLostFocus(object sender, RoutedEventArgs e)
        {
            var box = (RichTextBox)sender;

            //var xaml = XamlWriter.Save(box.Document);
            //SetBoundDocumentXaml(box, xaml);

            var tr = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);

            using(var ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.XamlPackage);
                var xamlText = Convert.ToBase64String(ms.ToArray());

                var tag = box.Tag;
                box.Tag = IgnoreOnBoundDocumentXamlChanged;
                SetBoundDocumentXaml(box, xamlText);
                box.Tag = tag;
            }

            //DelayedAction.Invoke(sender, () =>
            //{
                
            //}, 300, "HandleLostFocus");

            //SetBoundDocumentText(box, tr.Text);
        }

        private static void HandleTextChanged(object sender, RoutedEventArgs e)
        {
            HandleLostFocus(sender, e);

            //var box = (RichTextBox)sender;

            //var tr = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);

            //using (var ms = new MemoryStream())
            //{
            //    tr.Save(ms, DataFormats.Xaml);
            //    var bytes = ms.ToArray();
            //    var xamlText = Encoding.UTF8.GetString(bytes);
            //    SetBoundDocumentXaml(box, xamlText);
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        public static string GetBoundDocumentXaml(DependencyObject dependencyObject)
        {
            if(dependencyObject != null)
            {
                return dependencyObject.GetValue(BoundDocumentXamlProperty) as string;
            }
            return string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="value"></param>
        public static void SetBoundDocumentXaml(DependencyObject dependencyObject, string value)
        {
            if(dependencyObject != null)
            {
                dependencyObject.SetValue(BoundDocumentXamlProperty, value);
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="dependencyObject"></param>
        ///// <returns></returns>
        //public static string GetBoundDocumentText(DependencyObject dependencyObject)
        //{
        //    if(dependencyObject != null)
        //    {
        //        return dependencyObject.GetValue(BoundDocumentTextProperty) as string;
        //    }
        //    return string.Empty;
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="dependencyObject"></param>
        ///// <param name="value"></param>
        //public static void SetBoundDocumentText(DependencyObject dependencyObject, string value)
        //{
        //    if(dependencyObject != null)
        //    {
        //        dependencyObject.SetValue(BoundDocumentTextProperty, value);
        //    }
        //}
    }
}
