using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_for_BNF.Exceptions
{
    class SintaxException: Exception
    {
        public override string Message { get; } = string.Empty;
        public SintaxException(string message, Token token)
        {
            Message = message;
            Data["Token"] = token;
        }
        public SintaxException(Token token)
        {
            Data["Token"] = token;
        }
    }
}
