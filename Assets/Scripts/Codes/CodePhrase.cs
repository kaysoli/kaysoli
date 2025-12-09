using System.Collections.Generic;
using UnityEngine;

namespace RadioDispatch.Codes
{
    [System.Serializable]
    public class CodePhrase
    {
        public string Code;
        public string Meaning;
        public List<string> Variants = new();
    }
}
