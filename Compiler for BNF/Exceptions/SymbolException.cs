using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Compiler_for_BNF.Exceptions
{
    public class SymbolException:Exception
    {
        public override string Message { get;} = string.Empty;
        public SymbolException(string message,Token token)
        {
            Message = message;
            Data["Token"] = token;
        }
        public SymbolException(Token token) 
        {
            Data["Token"]=token;
        }
    }
}
