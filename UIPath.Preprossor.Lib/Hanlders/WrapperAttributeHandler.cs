using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace UIPath.Preprossor.Lib
{
    public class WrapperAttributeHandler : AttributeHanlder
    {
        private XDocument WrapperDoc { get; set; }

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

            WrapperDoc = XDocument.Parse(wapperTxt);

            var wrapper = WrapperDoc.XElement("Activity").XElement("Sequence");
            WorkContext.WrapTarget(wrapper);

            // Namespaces
            var existingNs = WorkContext.Doc.GetNamespaces();
            var templateNs = WrapperDoc.GetNamespaces();
            foreach (var ns in templateNs)
            {
                if (!existingNs.ContainsKey(ns.Key))
                {
                    WorkContext.Doc.XElement("Activity").SetAttributeValue(ns.Value.Name, ns.Value.Value);
                    Console.WriteLine("Add attribute:" + ns.Value.Value);
                }
            }

            MergeNamespaces();
        }

        public void MergeNamespaces()
        {
            var namespaces = WrapperDoc.Root.XPathSelectElements("//a:TextExpression.NamespacesForImplementation/sco:Collection/*", XMLExetension.NSManager).ToList();
            var existingNs = WorkContext.Doc.Root.XPathSelectElements("//a:TextExpression.NamespacesForImplementation/sco:Collection/*", XMLExetension.NSManager).Select(n => n.Value).ToList();
            var container = WorkContext.Doc.Root.XPathSelectElement("//a:TextExpression.NamespacesForImplementation/sco:Collection", XMLExetension.NSManager);
            foreach (var ns in namespaces)
            {
                if (!existingNs.Contains(ns.Value))
                {
                    Log.Info("Added Namespace: " + ns.Value);
                    container.Add(ns);
                }
            }

            var libs = WrapperDoc.Root.XPathSelectElements("//a:TextExpression.ReferencesForImplementation/sco:Collection/*", XMLExetension.NSManager).ToList();
            var existingLibs = WorkContext.Doc.Root.XPathSelectElements("//a:TextExpression.ReferencesForImplementation/sco:Collection/*", XMLExetension.NSManager).Select(n => n.Value).ToList();
            var libContainer = WorkContext.Doc.Root.XPathSelectElement("//a:TextExpression.ReferencesForImplementation/sco:Collection", XMLExetension.NSManager);
            foreach (var lib in libs)
            {
                if (!existingLibs.Contains(lib.Value))
                {
                    Log.Info("Added lib: " + lib.Value);
                    libContainer.Add(lib);
                }
            }
        }
    }
}
