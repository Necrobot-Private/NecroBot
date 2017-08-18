using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace System
{
    public static class UIExtensions {
        public static void AppendText(this RichTextBox box, string text, string color)
        {
            box.Dispatcher.Invoke(() =>
            {
                BrushConverter bc = new BrushConverter();
                TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd)
                {
                    Text = text
                };
                try
                {
                    // Mapping console color DarkYellow to Wheat color.
                    if (color == "DarkYellow")
                        color = "Wheat";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                        bc.ConvertFromString(color));
                }
                catch (FormatException)
                {
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                    bc.ConvertFromString("white"));
                }
            });
        }
    }
}
