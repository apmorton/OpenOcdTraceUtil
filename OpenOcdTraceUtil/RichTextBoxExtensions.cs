using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenOcdTraceUtil
{
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            // set selection to the end of the textbox
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            // set selection color and append the text
            box.SelectionColor = color;
            box.AppendText(text);

            // set the selection color back to the default
            box.SelectionColor = box.ForeColor;
        }

        public static void ScrollToEnd(this RichTextBox box)
        {
            box.SelectionStart = box.Text.Length;
            box.ScrollToCaret();
        }
    }
}
