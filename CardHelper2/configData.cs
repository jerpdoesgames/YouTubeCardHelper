using System;
using System.Collections.Generic;
using System.Text;

namespace CardHelper2
{
    class configData
    {
        public List<string> searchTerms { get; set; }
        public List<string> skipTerms { get; set; }
        public int lowFrequencyTermCountMaxGlobal { get; set; }
        public int lowFrequencyTermCountMaxList { get; set; }
    }
}
