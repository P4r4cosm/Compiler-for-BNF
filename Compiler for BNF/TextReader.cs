using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_for_BNF
{
    public class TextReader
    {
        private readonly string _filePath;

        public TextReader(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<string> GetInfo()
        {
            return await File.ReadAllTextAsync(_filePath);
        }
    }
}
