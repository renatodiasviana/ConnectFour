using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TreeNode is a base class for MinMaxNode. (I seperated tree stuffs from MinMaxNode then it can focus on AI code)
public class TreeNode
{
    List<TreeNode> Children;

    public TreeNode()
    {
        Children = new List<TreeNode>();
    }

    public bool Contains(TreeNode node)
    {
        foreach (TreeNode treeNode in Children)
        {
            if (treeNode == node)
                return true;
        }

        return false;
    }

    public int GetTreeChildrenCount()
    {
        return Children.Count;
    }

    public TreeNode GetTreeChild(int index)
    {
        if (index >= Children.Count)
            return null;

        return Children[index];
    }

    public void AddTreeChild(TreeNode node)
    {
        if (Contains(node))
            return;

        Children.Add(node);
    }

    public void RemoveTreeChild(TreeNode node)
    {
        Children.Remove(node);
    }
}
