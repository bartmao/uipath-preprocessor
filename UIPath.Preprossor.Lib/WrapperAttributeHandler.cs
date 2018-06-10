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

        public void Handle(string templateName, params string[] args)
        {
            var tPath = Path.Combine(WorkItem.WorkingPath, templateName + ".xaml");
            var docTxt = File.ReadAllText(tPath);

            var counter = 1;
            foreach (var arg in args)
            {
                docTxt = docTxt.Replace("\"$" + counter + "\"", "\"" + arg + "\"");
                counter++;
            }

            var doc = XDocument.Parse(docTxt);
            var replacement = doc.Descendants().SingleOrDefault(e => e.XAttribute("DisplayName")?.Value == "Placeholder");

            if (replacement != null)
            {
                replacement.ReplaceWith(WorkItem.Ele);
            }
            WorkItem.Ele.ReplaceWith(doc.XElement("Activity").XElement("Sequence"));

            // Namespaces
            var existingNs = WorkItem.Doc.GetNamespaces();
            var templateNs = doc.GetNamespaces();
            foreach (var ns in templateNs)
            {
                if (!existingNs.ContainsKey(ns.Key))
                {
                    WorkItem.Doc.XElement("Activity").SetAttributeValue(ns.Value.Name, ns.Value.Value);
                    Console.WriteLine("Add attribute:" + ns.Value.Value);
                }
            }
        }
    }
}
