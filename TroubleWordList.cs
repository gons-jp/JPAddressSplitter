using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPAddressSplitter
{
    public class TroubleWordList
    {
        public string Key = "";
        public string Value = "";
        public CharacterType CharType = CharacterType.C_None;
        public CharacterClass CharClass = CharacterClass.CLS_None;

        public TroubleWordList(string key, string value, CharacterClass charclass, CharacterType chartype)
        {
            Key = key;
            Value = value;
            CharType = chartype;
            CharClass = charclass;
        }
    }
}
