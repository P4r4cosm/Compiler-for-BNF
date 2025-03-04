using Compiler_for_BNF;
using System.Windows;

public class Parser
{
    private readonly List<Token> tokens;
    private int pos = 0;
    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    private Token CurrentToken => pos < tokens.Count ? tokens[pos] : new Token(TokenType.EndOfFile, "", 0, 0);

    private Token Consume(TokenType expectedType, string expectedValue = null)
    {
        Token token = CurrentToken;
        if (token.Type != expectedType || (expectedValue != null && token.Value != expectedValue))
            throw new Exception($"Ожидался {expectedType} со значением '{expectedValue}', а получен {token.Type} ('{token.Value}') в строке {token.Line}, столбце {token.Column}");
        pos++;
        return token;
    }

    // Язык = "Начало" Множество ... Множество Слагаемое Опер ... Опер "Конец"
    public void ParseLanguage()
    {

        string language = string.Empty;
        foreach (Token token in tokens)
        {
            language = language + token.ToString() + "\t";

        }
        //MessageBox.Show(language);
        Consume(TokenType.Keyword, "Начало");

        // Разбираем блоки Множество (может быть несколько)
        while (CurrentToken.Type == TokenType.Keyword && (CurrentToken.Value == "First" || CurrentToken.Value == "Second"))
        {
            ParseMnozhestvo();
        }

        // Разделитель между блоком Множество и Слагаемое – запятая
        Consume(TokenType.Punctuation, ",");
        //MessageBox.Show(CurrentToken.Value);
        // Разбираем слагаемое
        ParseSlagaemoe();

        // Разбираем операторы до ключевого слова "Конец"
        while (CurrentToken.Type != TokenType.EndOfFile &&
               !(CurrentToken.Type == TokenType.Keyword && CurrentToken.Value == "Конец"))
        {
            ParseOper();
        }
        Consume(TokenType.Keyword, "Конец");
    }

    // Множество = ("First" Перем { "," Перем }) | ("Second" Цел { Цел, причем если после целого идёт запятая – значит конец множества})
    private void ParseMnozhestvo()
    {
        if (CurrentToken.Value == "First")
        {
            Consume(TokenType.Keyword, "First");
            ParseVariable();
            // Разделитель между переменными – запятая
            while (CurrentToken.Type == TokenType.Punctuation && CurrentToken.Value == ",")
            {
                Consume(TokenType.Punctuation, ",");
                ParseVariable();
            }
        }
        else if (CurrentToken.Value == "Second")
        {
            Consume(TokenType.Keyword, "Second");
            // Читаем целые числа для множества.
            // Обязательно читаем хотя бы одно число.
            ParseInteger();
            // Пока следующий токен – целое число, смотрим, не стоит ли после него запятая.
            while (CurrentToken.Type == TokenType.Integer)
            {
                // Если после текущего целого стоит запятая, значит список множетсва заканчивается.
                if ((pos + 1) < tokens.Count &&
                    tokens[pos + 1].Type == TokenType.Punctuation &&
                    tokens[pos + 1].Value == ",")
                {
                    ParseInteger(); // Читаем последнее число множества.
                    break;        // И выходим, не потребляя запятую – она станет разделителем.
                }
                else
                {
                    ParseInteger();
                }
            }
        }
        else
        {
            throw new Exception("Ожидалось 'First' или 'Second' для множества.");
        }
    }

    // Слагаемое = Цел { "," Цел } "Конец слагаемого"
    private void ParseSlagaemoe()
    {
        ParseInteger();
        while (CurrentToken.Type == TokenType.Punctuation && CurrentToken.Value == ",")
        {
            Consume(TokenType.Punctuation, ",");
            ParseInteger();
        }
        Consume(TokenType.Keyword, "Конец слагаемого");
    }

    // Опер = Метка { Метка } ":" Перем "=" Прав.часть
    private void ParseOper()
    {
        // Метка – целое число
        ParseInteger();
        while (CurrentToken.Type == TokenType.Integer)
        {
            ParseInteger();
        }
        Consume(TokenType.Punctuation, ":");
        ParseVariable();
        Consume(TokenType.Operator, "=");
        ParsePravChast();
    }

    private void ParsePravChast()
    {
        if (CurrentToken.Type == TokenType.Operator && CurrentToken.Value == "-")
            Consume(TokenType.Operator, "-");
        ParseVyr1();
        while (CurrentToken.Type == TokenType.Operator && (CurrentToken.Value == "+" || CurrentToken.Value == "-"))
        {
            Consume(TokenType.Operator);
            ParseVyr1();
        }
    }
    private void ParseVyr1()
    {
        ParseVyr2();
        while (CurrentToken.Type == TokenType.Operator && (CurrentToken.Value == "*" || CurrentToken.Value == "/"))
        {
            Consume(TokenType.Operator);
            ParseVyr2();
        }
    }
    private void ParseVyr2()
    {
        ParseVyr3();
        while (CurrentToken.Type == TokenType.Operator && (CurrentToken.Value == "v" || CurrentToken.Value == "^"))
        {
            Consume(TokenType.Operator);
            ParseVyr3();
        }
    }
    private void ParseVyr3()
    {
        if (CurrentToken.Type == TokenType.Keyword && CurrentToken.Value == "not")
            Consume(TokenType.Keyword, "not");
        ParseVyr4();
    }
    private void ParseVyr4()
    {
        if (CurrentToken.Type == TokenType.Identifier)
            ParseVariable();
        else if (CurrentToken.Type == TokenType.Integer)
            ParseInteger();
        else if (CurrentToken.Type == TokenType.Punctuation && CurrentToken.Value == "(")
        {
            Consume(TokenType.Punctuation, "(");
            ParsePravChast();
            Consume(TokenType.Punctuation, ")");
        }
        else
        {
            throw new Exception($"Ошибка в выражении: {CurrentToken}");
        }
    }
    private void ParseVariable()
    {
        Consume(TokenType.Identifier);
    }
    private void ParseInteger()
    {
        Consume(TokenType.Integer);
    }
}
