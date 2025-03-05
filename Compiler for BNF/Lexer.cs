using Compiler_for_BNF.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Compiler_for_BNF
{
    public class Lexer
    {
        private readonly string input;
        private int pos = 0;
        private int line = 1;
        private int column = 1;
        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "Начало", "First", "Second", "Конец", "Конец слагаемого", "not"
        };
        private static readonly HashSet<string> Operators = new HashSet<string> { "+", "-", "*", "/", "|", "&", "=" };
        private static readonly HashSet<string> Punctuation = new HashSet<string> { ",", ":", "(", ")" };

        public Lexer(string input)
        {
            this.input = input;
        }

        private char CurrentChar => pos < input.Length ? input[pos] : '\0';

        private void MoveNext()
        {
            pos++;
            column++;
        }

        public Token GetNextToken()
        {
            while (char.IsWhiteSpace(CurrentChar))
                MoveNext();

            if (CurrentChar == '\0')
                return new Token(TokenType.EndOfFile, "", line, column,pos);

            if (char.IsLetter(CurrentChar))
                return ReadIdentifierOrKeyword();

            if (char.IsDigit(CurrentChar))
                return ReadNumber();

            if (Operators.Contains(CurrentChar.ToString()) || Punctuation.Contains(CurrentChar.ToString()))
                return ReadOperatorOrPunctuation();
            // Вместо выбрасывания исключения – создаем токен ошибки и продвигаемся дальше
            var errorToken = new Token(TokenType.Error, CurrentChar.ToString(), line, column,pos);
            MoveNext();
            return errorToken;
        }

        private Token ReadIdentifierOrKeyword()
        {
            int startCol = column;
            StringBuilder sb = new StringBuilder();
            // Читаем первую последовательность букв и цифр
            while (char.IsLetterOrDigit(CurrentChar))
            {
                sb.Append(CurrentChar);
                MoveNext();
            }
            string value = sb.ToString();

            // Если прочитано "Конец", проверяем, не следует ли "слегаемого"
            if (value == "Конец")
            {
                int savedPos = pos;
                int savedColumn = column;
                // Пропускаем пробельные символы (но не все, а только те, что разделяют слова)
                while (char.IsWhiteSpace(CurrentChar))
                    MoveNext();

                StringBuilder sb2 = new StringBuilder();
                while (char.IsLetterOrDigit(CurrentChar))
                {
                    sb2.Append(CurrentChar);
                    MoveNext();
                }
                string nextWord = sb2.ToString();
                if (nextWord == "слагаемого")
                {
                    // Объединяем в один токен
                    value = value + " " + nextWord;
                }
                else
                {
                    // Если не совпадает, возвращаемся в сохранённое положение
                    pos = savedPos;
                    column = savedColumn;
                }
            }
            TokenType type = Keywords.Contains(value) ? TokenType.Keyword : TokenType.Identifier;
            return new Token(type, value, line, startCol,pos);
        }


        private Token ReadNumber()
        {
            StringBuilder sb = new StringBuilder();
            int startCol = column;
            bool hasDigits = false;

            while (char.IsDigit(CurrentChar))
            {
                if (!"01234567".Contains(CurrentChar))
                {
                    throw new SymbolException($"Недопустимая цифра '{CurrentChar}' для восьмеричной системы в строке {line}, столбце {column}",
                        new Token(TokenType.Error, CurrentChar.ToString(), line, column, pos));
                }
                sb.Append(CurrentChar);
                MoveNext();
                hasDigits = true;
            }

            if (!hasDigits)
                throw new SymbolException($"Ожидалась цифра в строке {line}, столбце {startCol}",
                    new Token(TokenType.Error, "", line, startCol, pos));

            return new Token(TokenType.Integer, sb.ToString(), line, startCol, pos);
        }

        private Token ReadOperatorOrPunctuation()
        {
            string value = CurrentChar.ToString();
            int startCol = column;
            MoveNext();
            TokenType type = Operators.Contains(value) ? TokenType.Operator : TokenType.Punctuation;
            return new Token(type, value, line, startCol,pos);
        }

        // Собираем все токены в список
        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();
            Token token;
            do
            {
                token = GetNextToken();
                if (token.Type==TokenType.Error)
                {
                    throw new SymbolException($"Неожиданный символ: {token.Value}, строка: {token.Line} столбец: {token.Column}", token);
                }
                tokens.Add(token);
            } while (token.Type != TokenType.EndOfFile);
            return tokens;
        }
    }

}
