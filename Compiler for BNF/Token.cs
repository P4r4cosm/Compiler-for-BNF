using System;
using System.Collections.Generic;
using System.Drawing;
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
        public int Size { get; }
        public int IndexInText { get; set; }
        public Token(TokenType type, string value, int line, int column,int index)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
            IndexInText = index;
            Size=value.Length;
        }
        public override string ToString() => $"{Type}: '{Value}' (Line {Line}, Col {Column})";
    }

}
