using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Compiler_for_BNF
{
    public class RichTextBoxHelper
    {
        private RichTextBox RichTextBox { get; set; }
        public RichTextBoxHelper(RichTextBox richTextBox)
        {
            RichTextBox = richTextBox;
        }
        public void SetRichTextBoxText(string text)
        {
            // Очищаем существующие блоки
            RichTextBox.Document.Blocks.Clear();

            // Создаём новый параграф с текстом
            Paragraph paragraph = new Paragraph(new Run(text))
            {
                LineHeight = 14,// Устанавливаем межстрочное расстояние
                Margin = new System.Windows.Thickness(0)
            };

            // Добавляем параграф в документ
            RichTextBox.Document.Blocks.Add(paragraph);

            // Устанавливаем LineHeight для всего FlowDocument
            RichTextBox.Document.LineHeight = 14;
        }
        public void HighlightError(int index, int size)
        {
            var start = RichTextBox.Document.ContentStart.GetPositionAtOffset(index+2); //берём первую букву слова
            var end = start.GetPositionAtOffset(size); //последнюю букву слова
            var word = new TextRange(start, end); //делаем из слова textRange
            word.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Red); //красим фон
        }
    }
    
}
