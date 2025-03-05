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
            switch (expectedType)
            {
                //кидаем исключение если ждали переменную
                case TokenType.Identifier:
                    throw new SintaxException($"Ожидалась переменная, " +
               $"а получен {token.Type} ('{token.Value}') в строке {token.Line}, столбце {token.Column}", token); 
                
                //кидаем исключение если ждали целое число
                case TokenType.Integer:
                    throw new SintaxException($"Ожидалось число, " +
               $"а получен {token.Type} ('{token.Value}') в строке {token.Line}, столбце {token.Column}", token);

                //кидаем исключение если ждали что-то другое
                default:
                    throw new SintaxException($"Ожидался {expectedType} со значением '{expectedValue}', " +
               $"а получен {token.Type} ('{token.Value}') в строке {token.Line}, столбце {token.Column}", token);
            }
        }
        pos++;
        return token;
    }

    public void ParseLanguage()
    {
        Consume(TokenType.Keyword, "Начало");

        if (CurrentToken.Type != TokenType.Keyword || (CurrentToken.Value != "First" && CurrentToken.Value != "Second"))
        {
            throw new SintaxException("Ожидалось 'First' или 'Second' для множества.", CurrentToken);
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
                varName = ParseVariable();
                variables[varName] = 0;
            }
        }
        else if (CurrentToken.Value == "Second")
        {
            Consume(TokenType.Keyword, "Second");
            // Читаем целые числа до первой запятой, которая относится к Слагаемому
            List<int> secondNumbers = new List<int>();
            secondNumbers.Add(ParseInteger());

            while (CurrentToken.Type == TokenType.Integer)
            {
                // Смотрим, есть ли запятая после текущего числа
                if (pos + 1 < tokens.Count && tokens[pos + 1].Type == TokenType.Punctuation && tokens[pos + 1].Value == ",")
                {
                    // Если есть запятая, это уже начало Слагаемого, выходим
                    break;
                }
                secondNumbers.Add(ParseInteger());
            }

            //foreach (int value in secondNumbers)
            //{
            //    variables[$"int_{value}"] = value;
            //}
        }
        else
        {
            throw new SintaxException("Ожидалось 'First' или 'Second' для множества.", CurrentToken);
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
        labels.Add(ParseInteger());
        while (CurrentToken.Type == TokenType.Integer)
        {
            labels.Add(ParseInteger());
        }
        Consume(TokenType.Punctuation, ":");
        string varName = ParseVariable();
        Consume(TokenType.Operator, "=");
        int result = ParsePravChast();
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
            return ParseInteger();
        }
        else if (CurrentToken.Type == TokenType.Punctuation && CurrentToken.Value == "(")
        {
            Consume(TokenType.Punctuation, "(");
            int result = ParsePravChast();
            Consume(TokenType.Punctuation, ")");
            return result;
        }
        else
        {
            throw new SintaxException($"Ошибка в выражении: {CurrentToken}", CurrentToken);
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