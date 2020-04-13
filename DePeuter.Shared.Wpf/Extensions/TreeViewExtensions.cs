using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DePeuter.Shared.Wpf.Controls;

namespace DePeuter.Shared.Wpf.Extensions
{
    public static class TreeViewExtensions 
    {
        public static TreeViewDataItem GoToPreviousItem(this TreeView tv, Func<TreeViewDataItem, bool> whereClausule, bool forceSearchFromBeginning)
        {
            return GoToTreeViewDataItem(tv, whereClausule, forceSearchFromBeginning, true);
        }

        public static TreeViewDataItem GoToNextItem(this TreeView tv, Func<TreeViewDataItem, bool> whereClausule, bool forceSearchFromBeginning)
        {
            return GoToTreeViewDataItem(tv, whereClausule, forceSearchFromBeginning, false);
        }

        private static TreeViewDataItem GoToTreeViewDataItem(this TreeView tv, Func<TreeViewDataItem, bool> whereClausule, bool forceSearchFromBeginning, bool searchReverse)
        {
            TreeViewDataItem startItem = null;

            if(forceSearchFromBeginning || tv.SelectedItem == null)
            {
                var firstItem = tv.Items.Cast<TreeViewDataItem>().FirstOrDefault();
                if(firstItem != null)
                {
                    startItem = (TreeViewDataItem)firstItem.Parent;
                }
            }
            else
            {
                startItem = (TreeViewDataItem)tv.SelectedItem;
            }

            var res = startItem != null ? startItem.FindItem(whereClausule, searchReverse: searchReverse) : null;

            if(res != null)
            {
                ((TreeViewDataItem)res.Parent).Expand();
                res.IsSelected = true;
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }

            return res;
        }
    }
}
