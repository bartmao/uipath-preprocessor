using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UIPath.Preprossor.Lib
{
    public abstract class TypedActivityHandler : ActivityHandler
    {
        public string RegexFilter { get; set; }

        public TypedActivityHandler(string regexFilter)
        {
            RegexFilter = regexFilter;
        }

        public override bool Test(XElement activity, List<Tuple<string, string>> attrs)
        {
            return Regex.IsMatch(activity.Name.LocalName, "^" + RegexFilter + "$");
        }

        public abstract void Handle();
    }
}
