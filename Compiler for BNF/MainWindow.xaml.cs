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
            var textReader = new TextReader("D:\\C# projects\\Compiler for BNF\\Compiler for BNF\\Resources\\BNF.txt");
            BNFBox.Text = await textReader.GetInfo();
        }

        private void ComplileButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                string code = InputBox.Text;
                Lexer lexer = new Lexer(code);
                List<Token> tokens = lexer.Tokenize();
                Parser parser = new Parser(tokens);
                parser.ParseLanguage();
                OutputBox.Text = "Программа корректна";
            }
            catch (Exception ex)
            {
                OutputBox.Text = $"Ошибка: {ex.Message}";
            }
        }
    }
}