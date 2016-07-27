using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sayclip
{
  public interface iTranslator
    {
    
        string translate(string text);
        string[] separateSentences(string text);
        string unificateSentences(string[] sentences);

    }
}
