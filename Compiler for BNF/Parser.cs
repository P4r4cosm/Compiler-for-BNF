using Compiler_for_BNF;
using Compiler_for_BNF.Exceptions;
using System;
using System.Collections.Generic;

public class Parser
{
    private readonly List<Token> tokens;
    private int pos = 0;
    public Dictionary<string, int> variables = new Dictionary<string, int>();

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    private Token CurrentToken => pos < tokens.Count ? tokens[pos] : new Token(TokenType.EndOfFile, "", 0, 0, 0);

    private Token Consume(TokenType expectedType, string expectedValue = null)
    {
        Token token = CurrentToken;
        if (token.Type != expectedType || (expectedValue != null && token.Value != expectedValue))
        {
            string expected = expectedType switch
            {
                TokenType.Identifier => "переменная",
                TokenType.Integer => "целое число",
                TokenType.Keyword => $"ключевое слово '{expectedValue}'",
                TokenType.Punctuation => $"знак пунктуации '{expectedValue}'",
                TokenType.Operator => $"оператор '{expectedValue}'",
                _ => expectedType.ToString()
            };
            if (CurrentToken.Value==":")
            {
                throw new SintaxException("Пустой оператор", token);
            }
            throw new SintaxException($"Пропущен {expected}", token);
        }
        pos++;
        return token;
    }

    public void ParseLanguage()
    {
        Consume(TokenType.Keyword, "Начало");

        if (CurrentToken.Type != TokenType.Keyword || (CurrentToken.Value != "First" && CurrentToken.Value != "Second"))
        {
            throw new SintaxException("После 'Начало' должно идти множество ('First' или 'Second')", CurrentToken);
        }
        ParseMnozhestvo();

        while (CurrentToken.Type == TokenType.Keyword && (CurrentToken.Value == "First" || CurrentToken.Value == "Second"))
        {
            ParseMnozhestvo();
        }

        ParseSlagaemoe();

        while (CurrentToken.Type != TokenType.EndOfFile &&
               !(CurrentToken.Type == TokenType.Keyword && CurrentToken.Value == "Конец"))
        {
            ParseOper();
        }

        Consume(TokenType.Keyword, "Конец");
    }

    private void ParseMnozhestvo()
    {
        if (CurrentToken.Value == "First")
        {
            Consume(TokenType.Keyword, "First");
            string varName = ParseVariable();
            variables[varName] = 0;
            while (CurrentToken.Type == TokenType.Punctuation && CurrentToken.Value == ",")
            {
                Consume(TokenType.Punctuation, ",");
                if (CurrentToken.Type != TokenType.Identifier)
                {
                    throw new SintaxException($"В блоке 'First' после запятой должна быть переменная", CurrentToken);
                }
                varName = ParseVariable();
                variables[varName] = 0;
            }
            // Проверка на пропущенную запятую
            if (CurrentToken.Type == TokenType.Identifier)
            {
                throw new SintaxException($"Отсутствует запятая между '{varName}' и '{CurrentToken.Value}'.", CurrentToken);
            }
            //Consume(TokenType.Keyword, "Second");
            //List<int> secondNumbers = new List<int>();
            //secondNumbers.Add(ParseInteger());
            //while (CurrentToken.Type == TokenType.Integer)
            //{
            //    secondNumbers.Add(ParseInteger());
            //}
        }
        else if (CurrentToken.Value == "Second")
        {
            // Логика для "Second" остается без изменений
            Consume(TokenType.Keyword, "Second");
            List<int> secondNumbers = new List<int>();
            secondNumbers.Add(ParseInteger());
            while (CurrentToken.Type == TokenType.Integer)
            {
                if (pos + 1 < tokens.Count && tokens[pos + 1].Type == TokenType.Punctuation && tokens[pos + 1].Value == ",")
                {
                    break;
                }
                secondNumbers.Add(ParseInteger());
            }
        }
        else
        {
            throw new SintaxException($"Необходимо хотя бы одно множество ('First' или 'Second')", CurrentToken);
        }
    }

    private void ParseSlagaemoe()
    {
        int value = ParseInteger();
        //variables[$"slag_{value}"] = value;
        while (CurrentToken.Type == TokenType.Punctuation && CurrentToken.Value == ",")
        {
            Consume(TokenType.Punctuation, ",");
            value = ParseInteger();
            //variables[$"slag_{value}"] = value;
        }
        Consume(TokenType.Keyword, "Конец слагаемого");
    }

    private void ParseOper()
    {
        List<int> labels = new List<int>();
        if (CurrentToken.Type != TokenType.Integer)
        {
            throw new SintaxException($"Должна быть метка (целое число) перед ':'", CurrentToken);
        }
        labels.Add(ParseInteger());
        while (CurrentToken.Type == TokenType.Integer)
        {
            labels.Add(ParseInteger());
        }
        Consume(TokenType.Punctuation, ":");
        string varName = ParseVariable();
        Consume(TokenType.Operator, "=");
        int result = ParsePravChast();

        // Проверка на лишний ':'
        if (CurrentToken.Type == TokenType.Punctuation && CurrentToken.Value == ":")
        {
            throw new SintaxException($"Не найдена метка/метки для оператора", CurrentToken);
        }
        variables[varName] = result;
    }
    private int ParsePravChast()
    {
        bool isNegative = false;
        if (CurrentToken.Type == TokenType.Operator && CurrentToken.Value == "-")
        {
            Consume(TokenType.Operator, "-");
            isNegative = true;
        }
        int result = ParseVyr1();
        if (isNegative) result = -result;

        while (CurrentToken.Type == TokenType.Operator && (CurrentToken.Value == "+" || CurrentToken.Value == "-"))
        {
            string op = Consume(TokenType.Operator).Value;
            // Проверка на валидный операнд после оператора
            if (!(CurrentToken.Type == TokenType.Identifier ||
                  CurrentToken.Type == TokenType.Integer ||
                  (CurrentToken.Type == TokenType.Punctuation && CurrentToken.Value == "(") ||
                  (CurrentToken.Type == TokenType.Keyword && CurrentToken.Value == "not")))
            {
                throw new SintaxException($"Встречен '{CurrentToken.Value}' после оператора '{op}'", CurrentToken);
            }
            int next = ParseVyr1();
            result = op == "+" ? result + next : result - next;
        }
        return result;
    }

    private int ParseVyr1()
    {
        int result = ParseVyr2();
        while (CurrentToken.Type == TokenType.Operator && (CurrentToken.Value == "*" || CurrentToken.Value == "/"))
        {
            string op = Consume(TokenType.Operator).Value;
            int next = ParseVyr2();
            result = op == "*" ? result * next : result / next;
        }
        return result;
    }

    private int ParseVyr2()
    {
        int result = ParseVyr3();
        while (CurrentToken.Type == TokenType.Operator && (CurrentToken.Value == "&" || CurrentToken.Value == "|"))
        {
            string op = Consume(TokenType.Operator).Value;
            int next = ParseVyr3();
            result = op == "&" ? result & next : result | next; // "&" как AND, "|" как OR
        }
        return result;
    }

    private int ParseVyr3()
    {
        bool isNot = false;
        if (CurrentToken.Type == TokenType.Keyword && CurrentToken.Value == "not")
        {
            Consume(TokenType.Keyword, "not");
            isNot = true;
        }
        int result = ParseVyr4();
        return isNot ? ~result : result;
    }

    private int ParseVyr4()
    {
        if (CurrentToken.Type == TokenType.Identifier)
        {
            string varName = ParseVariable();
            if (!variables.ContainsKey(varName))
                throw new SintaxException($"Переменная {varName} не определена.", CurrentToken);
            return variables[varName];
        }
        else if (CurrentToken.Type == TokenType.Integer)
        {
            int value = ParseInteger();
            return value;
        }
        else if (CurrentToken.Type == TokenType.Punctuation && CurrentToken.Value == "(")
        {
            Consume(TokenType.Punctuation, "(");
            int result = ParsePravChast();
            if (CurrentToken.Type != TokenType.Punctuation || CurrentToken.Value != ")")
            {
                if (CurrentToken.Type == TokenType.Identifier || CurrentToken.Type == TokenType.Integer)
                    throw new SintaxException($"Пропущен оператор", CurrentToken);
                throw new SintaxException($"Ожидалась закрывающая скобка ')', но найден '{CurrentToken.Value}'", CurrentToken);
            }
            Consume(TokenType.Punctuation, ")");
            return result;
        }
        else if (CurrentToken.Value == ")")
        {
            throw new SintaxException($"Встречена лишняя закрывающая скобка '{CurrentToken.Value}'", CurrentToken);
        }
        else
        {
            throw new SintaxException($"Ошибка в выражении: неожиданный символ '{CurrentToken.Value}'", CurrentToken);
        }
    }

    private string ParseVariable()
    {
        return Consume(TokenType.Identifier).Value;
    }

    private int ParseInteger()
    {
        string value = Consume(TokenType.Integer).Value;
        return Convert.ToInt32(value, 8); // Парсим как восьмеричное число
    }
}