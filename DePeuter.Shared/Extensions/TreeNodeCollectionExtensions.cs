using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public static class TreeNodeCollectionExtensions
{
    public static void AddByIndexSelector(this TreeNodeCollection nodes, TreeNode tn, Func<TreeNode, string> indexSelector)
    {
        var newPlace = indexSelector(tn);

        var hasInserted = false;

        for(var i = 0; i < nodes.Count; i++)
        {
            var place = indexSelector(nodes[i]);

            if(string.Compare(place, newPlace) > 0)
            {
                nodes.Insert(i, tn);
                hasInserted = true;
                break;
            }
        }

        if(!hasInserted)
        {
            nodes.Add(tn);
        }
    }

    public static int FindIndex(this TreeNodeCollection nodes, Func<TreeNode, bool> indexSelector)
    {
        for(var i = 0; i < nodes.Count; i++)
        {
            if (indexSelector(nodes[i]))
            {
                return i;
            }
        }

        return -1;
    }
}