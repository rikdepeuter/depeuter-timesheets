using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public static class ControlCollectionExtensions
{
    public static IEnumerable<Control> AsEnumerable(this Control.ControlCollection collection)
    {
        foreach(Control value in collection)
        {
            yield return value;
        }
    }
}