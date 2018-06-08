using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UIPath.Preprossor.Lib
{
    public abstract class AttributeHanlder
    {
        protected WorkItem WorkItem { get; set; }

        public string Name { get; set; }

        public AttributeHanlder(string name)
        {
            Name = name;
        }
    }
}
