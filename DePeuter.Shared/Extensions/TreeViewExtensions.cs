using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;

public static class TreeviewExtensions
{
    public static void RemoveNodes(this TreeView tv, Func<TreeNode, bool> predicate)
    {
        foreach (TreeNode tn in tv.Nodes)
        {
            tn.Remove(predicate);
        }
    }

    public static void OnAllNodes(this TreeView tv, Action<TreeNode> action)
    {
        foreach(TreeNode tn in tv.Nodes)
        {
            tn.OnAllChildren(action);
        }
    }

    public static TreeNode FindNode(this TreeView tv, Func<TreeNode, bool> predicate)
    {
        foreach(TreeNode tn in tv.Nodes)
        {
            var res = tn.FindChild(predicate);
            if (res != null)
            {
                return res;
            }
        }

        return null;
    }

    public static List<TreeNode> FindNodes(this TreeView tv, Func<TreeNode, bool> predicate)
    {
        var res = new List<TreeNode>();

        foreach(TreeNode tn in tv.Nodes)
        {
            res.AddRange(tn.FindChildren(predicate));
        }

        return res;
    }

    public static void SelectPreviousNode(this TreeView tv)
    {
        var selectedNode = tv.SelectedNode;
        if(selectedNode == null)
        {
            return;
        }

        var previousNode = selectedNode.PrevNode;
        if(previousNode == null)
        {
            previousNode = selectedNode.Parent;
        }
        else
        {
            while(previousNode.Nodes.Count > 0)
            {
                previousNode = previousNode.Nodes[previousNode.Nodes.Count - 1];
            }
        }

        if(previousNode == null)
        {
            return;
        }

        tv.SelectedNode = previousNode;
    }

    public static void SelectNextNode(this TreeView tv)
    {
        TreeNode nextNode = null;

        var selectedNode = tv.SelectedNode;
        if(selectedNode == null)
        {
            if(tv.Nodes.Count > 0)
            {
                nextNode = tv.Nodes[0];
            }
            else
            {
                return;
            }
        }

        if(nextNode == null)
        {
            if(selectedNode.Nodes.Count > 0)
            {
                nextNode = selectedNode.Nodes[0];
            }
            else
            {
                nextNode = selectedNode.NextNode;
            }
        }

        if(nextNode == null)
        {
            nextNode = selectedNode.Parent;
            while(nextNode != null)
            {
                var nextNode2 = nextNode.NextNode;
                if(nextNode2 == null)
                {
                    nextNode = nextNode.Parent;
                }
                else
                {
                    nextNode = nextNode2;
                    break;
                }
            }
        }

        if(nextNode == null)
        {
            return;
        }

        tv.SelectedNode = nextNode;
    }
}