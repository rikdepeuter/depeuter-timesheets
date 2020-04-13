using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GeoIT_Infrastructure.Controls
{
    public class TreeView2 : TreeView
    {
        public TreeView2()
        {
            DrawMode = TreeViewDrawMode.OwnerDrawText;
        }

        private Color _highlightText = SystemColors.HighlightText;
        private Color _highlight = SystemColors.Highlight; 

        public Color HighlightText { get { return _highlightText; } set { _highlightText = value; } }
        public Color Highlight { get { return _highlight; } set { _highlight = value; } }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            var font = e.Node.NodeFont ?? e.Node.TreeView.Font;
            var fore = e.Node.ForeColor;
            if(fore == Color.Empty) fore = e.Node.TreeView.ForeColor;
            var bounds = new Rectangle(e.Bounds.X + 1, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            if(e.Node == e.Node.TreeView.SelectedNode)
            {
                fore = _highlightText;
                e.Graphics.FillRectangle(new SolidBrush(_highlight), e.Bounds);

                ControlPaint.DrawFocusRectangle(e.Graphics, bounds, fore, _highlight);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, bounds, fore, TextFormatFlags.GlyphOverhangPadding);
            }
            else
            {
                e.Graphics.FillRectangle(SystemBrushes.Window, bounds);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, bounds, fore, TextFormatFlags.GlyphOverhangPadding);
            }
        }
    }
}
