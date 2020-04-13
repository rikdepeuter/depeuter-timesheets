using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DePeuter.Shared.Wpf.Controls.Infrastructure;
using PropertyChanged;

namespace DePeuter.Shared.Wpf.Controls
{
    [ImplementPropertyChanged]
    public class TreeViewDataItem : ControlViewModelBase
    {
        public bool IsRoot { get; private set; }

        public static TreeViewDataItem NewRootItem(ViewModelBase parent)
        {
            var item = new TreeViewDataItem(parent);
            item.IsRoot = true;
            return item;
        }

        public event EventHandler IsSelectedChanged;
        public event EventHandler IsExpandedChanged;

        public string Header { get; set; }
        public object Tag { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();

                if(IsSelectedChanged != null)
                {
                    IsSelectedChanged(this, EventArgs.Empty);
                }
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                OnPropertyChanged();

                if(IsExpandedChanged != null)
                {
                    IsExpandedChanged(this, EventArgs.Empty);
                }
            }
        }

        public ObservableCollection<TreeViewDataItem> Items { get; private set; }

        public TreeViewDataItem ParentItem
        {
            get { return Parent as TreeViewDataItem; }
        }

        public TreeViewDataItem(ViewModelBase parent)
            : base(parent)
        {
            Items = new ObservableCollection<TreeViewDataItem>();
        }

        public void ExecuteOnAllItems(Action<TreeViewDataItem> action)
        {
            if(Items == null)
            {
                return;
            }

            action(this);

            foreach(var item in Items)
            {
                ExecuteOnAllItems(item, action);
            }
        }

        private void ExecuteOnAllItems(TreeViewDataItem parent, Action<TreeViewDataItem> action)
        {
            if(parent == null)
            {
                return;
            }

            action(parent);

            foreach(var item in parent.Items)
            {
                ExecuteOnAllItems(item, action);
            }
        }

        private TreeViewDataItem GetLastOffspring()
        {
            var lastItem = Items.LastOrDefault();
            if(lastItem != null)
            {
                if(lastItem.Items.Any())
                {
                    return lastItem.GetLastOffspring();
                }

                return lastItem;
            }

            return null;
        }

        private TreeViewDataItem GetPreviousSibling()
        {
            var siblingItems = ParentItem.Items;

            var foundSelf = false;
            foreach(var item in siblingItems.Reverse())
            {
                if(!foundSelf)
                {
                    if(item == this)
                    {
                        foundSelf = true;
                    }
                    continue;
                }

                return item;
            }

            return null;
        }

        private TreeViewDataItem GetNextSibling()
        {
            var siblingItems = ParentItem.Items;

            var foundSelf = false;
            foreach(var item in siblingItems)
            {
                if(!foundSelf)
                {
                    if(item == this)
                    {
                        foundSelf = true;
                    }
                    continue;
                }

                return item;
            }

            return null;
        }

        public TreeViewDataItem FindItem(Func<TreeViewDataItem, bool> whereFunc, bool compareSelf = false, bool searchReverse = false)
        {
            return searchReverse ? FindPreviousItem(whereFunc, compareSelf) : FindNextItem(whereFunc, compareSelf);
        }

        public TreeViewDataItem FindPreviousItem(Func<TreeViewDataItem, bool> whereFunc, bool compareSelf, bool compareSelfWhenParentHasNoSiblings = false)
        {
            if(compareSelf && whereFunc(this))
            {
                return this;
            }

            if(ParentItem != null)
            {
                var previousSibling = GetPreviousSibling();
                if(previousSibling != null)
                {
                    //if previousSibling has items, go to the last offspring
                    if(previousSibling.Items.Any())
                    {
                        var lastOffspringItem = previousSibling.GetLastOffspring();

                        var res = lastOffspringItem.FindPreviousItem(whereFunc, true);
                        if(res != null)
                        {
                            return res;
                        }
                    }
                    else
                    {
                        var res = previousSibling.FindPreviousItem(whereFunc, true);
                        if(res != null)
                        {
                            return res;
                        }
                    }
                }
                else
                {
                    if(whereFunc(ParentItem))
                    {
                        return ParentItem;
                    }
                }

                //no previous siblings, go to previous sibling of parent
                {
                    if(compareSelfWhenParentHasNoSiblings && whereFunc(this))
                    {
                        return this;
                    }

                    //alleen maar op de 1e parent level, als hij naar level 2 gaat dan moet hij zichzelf eerst comparen
                    var res = ParentItem.FindPreviousItem(whereFunc, false, compareSelfWhenParentHasNoSiblings: true);
                    if(res != null)
                    {
                        return res;
                    }
                }
            }

            return null;
        }

        public TreeViewDataItem FindNextItem(Func<TreeViewDataItem, bool> whereFunc, bool compareSelf)
        {
            if(compareSelf && whereFunc(this))
            {
                return this;
            }

            if(Items != null)
            {
                foreach(var child in Items)
                {
                    var res = child.FindNextItem(whereFunc, true);
                    if(res != null)
                    {
                        return res;
                    }
                }
            }

            //if(!IsRoot)
            //{
            //    //search siblings when this is the first search level
            //    if(ParentItem != null)
            //    {
            //        var nextSibling = GetNextSibling();
            //        if(nextSibling != null)
            //        {
            //            var res = nextSibling.FindNextItem(whereFunc, true);
            //            if(res != null)
            //            {
            //                return res;
            //            }
            //        }

            //        //als er geen siblings meer zijn, ga verder naar volgende parent
            //        var parentItem = ParentItem;
            //        if(!parentItem.IsRoot)
            //        {
            //            var parentSibling = parentItem.GetNextSibling();
            //            if(parentSibling != null)
            //            {
            //                var res = parentSibling.FindNextItem(whereFunc, true);
            //                if(res != null)
            //                {
            //                    return res;
            //                }
            //                //break;
            //            }

            //            //parentItem = parentItem.ParentItem;
            //        }
            //    }
            //}

            return null;
        }

        public void Expand()
        {
            IsExpanded = true;

            if(ParentItem != null)
            {
                ParentItem.Expand();
            }
        }

        public void Collapse()
        {
            IsExpanded = false;
        }
    }
}
