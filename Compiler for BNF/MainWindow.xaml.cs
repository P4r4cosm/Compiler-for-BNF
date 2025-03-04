using Microsoft.Extensions.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Compiler_for_BNF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            LoadFileContentAsync();

        }
        private async void LoadFileContentAsync()
        {

            //Заполняем BNFBox.Text
            var textReaderBNF = new TextReader("D:\\C# projects\\Compiler for BNF\\Compiler for BNF\\Resources\\BNF.txt");
            BNFBox.Text = await textReaderBNF.GetInfo();

            //Заполняем InputBox
            var textReaderCode = new TextReader("D:\\C# projects\\Compiler for BNF\\Compiler for BNF\\Resources\\StartCode.txt");
            var code= textReaderCode.GetInfo();
            var richTextBoxHelper = new RichTextBoxHelper(InputBox);
            richTextBoxHelper.SetRichTextBoxText(await code);

        }

        private void ComplileButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var textRange = new TextRange(InputBox.Document.ContentStart, InputBox.Document.ContentEnd); //считываем текст из richtextbox
                string code = textRange.Text;
                Lexer lexer = new Lexer(code);
                List<Token> tokens = lexer.Tokenize();
                Parser parser = new Parser(tokens);
                parser.ParseLanguage();
                OutputBox.Text = "Программа корректна";
            }
            catch (Exception ex)
            {
                
                var TextRange= new TextRange(InputBox.Document.ContentStart, InputBox.Document.ContentEnd); //считываем текст из richtextbox
                string fullText=TextRange.Text; //получаем весь текст 
                var start = InputBox.Document.ContentStart.GetPositionAtOffset(0); //берём первую букву слова
                var end = start.GetPositionAtOffset(6+2); //последнюю букву слова
                var word = new TextRange(start, end); //делаем из слова textRange
                word.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Red); //красим фон



                OutputBox.Text = $"Ошибка: {ex.Message}";
            }
        }




        //обработчики событий для вставки текста без сохранения форматирования из предыдущего документа
        private void PasteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Разрешаем вставку, если есть текст в буфере обмена
            e.CanExecute = Clipboard.ContainsText();
        }

        private void PasteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Получаем чистый текст из буфера обмена
            string plainText = Clipboard.GetText();

            if (!string.IsNullOrEmpty(plainText))
            {
                // Получаем текущую позицию курсора в RichTextBox
                TextPointer insertionPosition = InputBox.CaretPosition;

                // Создаём новый параграф или используем существующий
                Paragraph paragraph = insertionPosition.Paragraph;
                if (paragraph == null)
                {
                    paragraph = new Paragraph();
                    InputBox.Document.Blocks.Add(paragraph);
                }

                // Вставляем текст как Run с настройками RichTextBox
                Run run = new Run(plainText, insertionPosition);

                // Применяем настройки RichTextBox (шрифт и размер из стиля)
                run.FontFamily = InputBox.FontFamily; // Consolas из вашего стиля
                run.FontSize = InputBox.FontSize;     // 18 из вашего стиля

                // Обновляем позицию курсора после вставки
                InputBox.CaretPosition = run.ContentEnd;
            }

            // Отмечаем событие как обработанное
            e.Handled = true;
        }
    }
}