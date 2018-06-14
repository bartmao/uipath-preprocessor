using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace UIPath.Preprossor.Lib
{
    public class WrapperAttributeHandler : AttributeHanlder
    {
        public WrapperAttributeHandler() : base("Wrapper")
        {
        }

        public void Handle(string wrapperFile, params string[] args)
        {
            var wPath = Path.Combine(WorkContext.WorkingPath, wrapperFile + ".xaml");
            var wapperTxt = File.ReadAllText(wPath);
            var target = WorkContext.Activity;

            var counter = 1;
            foreach (var arg in args)
            {
                wapperTxt = wapperTxt.Replace("\"$" + counter + "\"", "\"" + arg + "\"");
                counter++;
            }

            var doc = XDocument.Parse(wapperTxt);
            var replacement = doc.Descendants().SingleOrDefault(e => e.XAttribute("DisplayName")?.Value == "Placeholder");
            if (replacement != null)
            {
                replacement.ReplaceWith(target);
                //WorkContext.MoveActivity(replacement, wPath);
            }

            var wrapper = doc.XElement("Activity").XElement("Sequence");
            WorkContext.ReplaceActivity(wrapper);

            // Namespaces
            var existingNs = WorkContext.Doc.GetNamespaces();
            var templateNs = doc.GetNamespaces();
            foreach (var ns in templateNs)
            {
                if (!existingNs.ContainsKey(ns.Key))
                {
                    WorkContext.Doc.XElement("Activity").SetAttributeValue(ns.Value.Name, ns.Value.Value);
                    Console.WriteLine("Add attribute:" + ns.Value.Value);
                }
            }
        }
    }
}
