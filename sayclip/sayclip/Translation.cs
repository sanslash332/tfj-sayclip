using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sayclip
{
    public class Translation
    {
        public Int64 Id { get; set; }
        public string sourceText { get; set; }
        public string translatedText { get; set; }
        public string fromLanguage { get; set; }
        public string toLanguage { get; set; }
        public string plugin { get; set; }
    }
}
