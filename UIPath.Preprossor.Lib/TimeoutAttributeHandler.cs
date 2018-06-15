using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace UIPath.Preprossor.Lib
{
    public class TimeoutAttributeHandler : AttributeHanlder
    {
        public TimeoutAttributeHandler() : base("Timeout")
        {
        }

        public void Handle(double milliseconds)
        {
            WorkContext.Activity.XPathSelectElement(".//ui:Target", XMLExetension.NSManager).SetAttributeValue("TimeoutMS", milliseconds);
        }
    }
}
