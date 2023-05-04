using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPAddressSplitter
{
    public class SplitInfo
    {
        public ValueType AddressType;
        public string AddressValue;

        public SplitInfo(ValueType addType, string AddVal)
        {
            AddressType = addType;
            AddressValue = AddVal;
        }
    }
}
