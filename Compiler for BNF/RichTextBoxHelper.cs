using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

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
                LineHeight = 14 // Устанавливаем межстрочное расстояние
            };

            // Добавляем параграф в документ
            RichTextBox.Document.Blocks.Add(paragraph);

            // Устанавливаем LineHeight для всего FlowDocument
            RichTextBox.Document.LineHeight = 14;
        }

    }
}
