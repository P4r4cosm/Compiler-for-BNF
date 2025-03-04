using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_for_BNF
{
    public enum TokenType
    {
        Keyword,
        Identifier,
        Integer,
        Operator,
        Punctuation,
        Error,
        EndOfFile
    }
    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Line { get; }
        public int Column { get; }
        public Token(TokenType type, string value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }
        public override string ToString() => $"{Type}: '{Value}' (Line {Line}, Col {Column})";
    }

}
