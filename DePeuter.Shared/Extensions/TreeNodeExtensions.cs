using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;

public static class TreeNodeExtensions
{
    public static TreeNode FindChild(this TreeNode tn, Func<TreeNode, bool> predicate)
    {
        if (tn == null) return null;

        foreach(TreeNode child in tn.Nodes)
        {
            if(child == null) continue;

            var res = child.FindChild(predicate);
            if (res != null)
            {
                return res;
            }
        }

        return predicate(tn) ? tn : null;
    }

    public static List<TreeNode> GetAllChildren(this TreeNode tn)
    {
        return FindChildren(tn, x => true);
    }

    public static List<TreeNode> FindChildren(this TreeNode tn, Func<TreeNode, bool> predicate)
    {
        var res = new List<TreeNode>();

        if(tn == null) return res;

        foreach(TreeNode child in tn.Nodes)
        {
            if(child == null) continue;

            res.AddRange(child.FindChildren(predicate));
        }

        if (predicate(tn))
        {
            res.Add(tn);
        }

        return res;
    }

    public static void Remove(this TreeNode tn, Func<TreeNode, bool> predicate)
    {
        if (tn == null) return;

        foreach (TreeNode child in tn.Nodes)
        {
            if (child == null) continue;

            child.Remove(predicate);
        }

        if(predicate(tn))
        {
            tn.Remove();
        }
    }

    public static IEnumerable<TreeNode> Children(this TreeNode node)
    {
        foreach (TreeNode child in node.Nodes)
        {
            foreach (var childNode in Children(child))
            {
                yield return childNode;
            }
        }

        yield return node;
    }

    public static IEnumerable<TreeNode> Parents(this TreeNode node)
    {
        var p = node.Parent;
        while (p != null)
        {
            yield return p;

            p = p.Parent;
        }
    }

    public static void OnAllChildren(this TreeNode tn, Action<TreeNode> recursive)
    {
        if (tn == null) return;

        foreach (TreeNode child in tn.Nodes)
        {
            child.OnAllChildren(recursive);
        }

        recursive(tn);
    }

    public static void OnAllParents(this TreeNode tn, Action<TreeNode> recursive)
    {
        if (tn == null) return;

        if (tn.Parent != null)
        {
            tn.Parent.OnAllParents(recursive);
        }

        recursive(tn);
    }

    
}