using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UIPath.Preprossor.Lib
{
    public class WorkItem
    {
        public XDocument Doc { get; set; }

        public string FileName { get; set; }

        public string WorkingPath { get; set; }

        public List<Tuple<string, string>> Attributes { get; set; }

        public XElement GetActivity()
        {
            return Doc.Descendants().Single(e => e.Attribute("UIPathPreprocessor") != null);
        }
    }
}
