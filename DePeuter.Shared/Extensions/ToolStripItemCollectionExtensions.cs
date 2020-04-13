using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public static class ToolStripItemCollectionExtensions
{
    public static IEnumerable<ToolStripItem> AsEnumerable(this ToolStripItemCollection collection)
    {
        foreach(ToolStripItem value in collection)
        {
            yield return value;
        }
    }
}