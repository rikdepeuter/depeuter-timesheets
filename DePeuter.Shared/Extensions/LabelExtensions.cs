using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public static class LabelExtensions
{
    public static void Clear(this Label label)
    {
        label.Text = string.Empty;
    }
}