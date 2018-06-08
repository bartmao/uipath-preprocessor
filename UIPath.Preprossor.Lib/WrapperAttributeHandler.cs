using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UIPath.Preprossor.Lib
{
    public class WrapperAttributeHandler : AttributeHanlder
    {
        public WrapperAttributeHandler() : base("Wrapper")
        {
        }

        public void Handle(string templateName, string arg)
        {
            var tPath = Path.Combine(WorkItem.WorkingPath, templateName + ".xaml");
            var doc = XDocument.Load(File.OpenRead(tPath));
            var replacement = doc.Descendants().SingleOrDefault(e => e.XAttribute("DisplayName")?.Value == "Placeholder");
            if (replacement != null)
            {
                replacement.AddAfterSelf(WorkItem.Ele);
                replacement.Remove();
            }
            WorkItem.Ele.AddAfterSelf(doc.XElement("Activity").XElement("Sequence").Elements());
            WorkItem.Ele.Remove();

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

            //var counter = 1;
            //WorkItem.Doc.XElement("Activity").Value = WorkItem.Doc.XElement("Activity").Value.Replace("\"$" + counter + "\"", "\"" + arg + "\"");

            //foreach (var arg in args)
            //{
            //    WorkItem.Doc.XElement("Activity").Value = WorkItem.Doc.XElement("Activity").Value.Replace("\"$" + counter + "\"", "\"" + arg + "\"");
            //    counter++;
            //}
        }
    }
}
