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
                wapperTxt = wapperTxt.Replace("$" + counter + "$", XMLExetension.Escape(arg));
                counter++;
            }

            var wdoc = XDocument.Parse(wapperTxt);

            var wrapper = wdoc.XElement("Activity").XElement("Sequence");
            WorkContext.WrapTarget(wrapper);

            // Namespaces
            var existingNs = WorkContext.Doc.GetNamespaces();
            var templateNs = wdoc.GetNamespaces();
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
