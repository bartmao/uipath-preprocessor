using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UIPath.Preprossor.Lib
{
    public abstract class AttributeHanlder : ActivityHandler
    {
        public string Name { get; set; }

        public AttributeHanlder(string name)
        {
            Name = name;
        }

        public override bool Test(XElement activity, List<Tuple<string, string>> attrs)
        {
            return attrs.SingleOrDefault(a => a.Item1 == Name) != null;
        }
    }
}
