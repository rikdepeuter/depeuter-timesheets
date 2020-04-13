using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public static class PropertyGridExtensions
{
    public static void Select(this PropertyGrid pg, Func<GridItem, bool> predicate)
    {
        var root = pg.SelectedGridItem;
        while(root.Parent != null)
            root = root.Parent;

        pg.SelectedGridItem = Find(root, predicate);
    }
    public static void SelectCategory(this PropertyGrid pg, string category)
    {
        Select(pg, gi => gi.GridItemType == GridItemType.Category && gi.Label == category);
    }
    public static void SelectProperty(this PropertyGrid pg, string property)
    {
        Select(pg, gi => gi.PropertyDescriptor != null && gi.PropertyDescriptor.Name == property);
    }
    private static GridItem Find(GridItem gridItem, Func<GridItem, bool> predicate)
    {
        if(gridItem == null) return null;

        if(predicate(gridItem)) return gridItem;

        foreach(GridItem child in gridItem.GridItems)
        {
            var res = Find(child, predicate);
            if(res != null)
            {
                return res;
            }
        }

        return null;
    }
}