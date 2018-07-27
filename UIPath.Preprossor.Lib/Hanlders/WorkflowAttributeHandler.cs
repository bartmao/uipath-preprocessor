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
    public class WorkflowAttributeHandler : AttributeHanlder
    {
        public WorkflowAttributeHandler() : base("Workflow")
        {
        }

        public void Handle(string newWorkflowName)
        {
            // 1. New doc of the workflow copy from origin
            // 2. Only keep the ele inside the main sequence
            // 3. Keep all arguments
            // 4*(0). Get all accessible varibles in original doc and convert them to arguments
            // 5. Invoke new workflow from origin workflow
            var n = WorkContext.Activity.Parent;
            var variables = new List<XElement>();
            while (n != null)
            {
                if (n.Name.LocalName == "Sequence" && n.XElement("Sequence.Variables") != null)
                {
                    foreach (var ele in n.XElement("Sequence.Variables").Elements())
                    {
                        var vname = ele.XAttribute("Name").Value;
                        if (variables.FirstOrDefault(v => v.XAttribute("Name").Value == vname) == null)
                        {
                            variables.Add(ele);
                        }
                    }
                }
                n = n.Parent;
            }

            // Create new workflow
            var p = Path.Combine(WorkContext.WorkingPath, newWorkflowName + ".xaml");
            //if (File.Exists(p)) throw new Exception("Workflow file existed:" + p);
            File.Copy(WorkContext.FileName, p, true);
            XDocument newDoc = null;
            XElement root;
            using (var stream = File.OpenRead(p))
            {
                newDoc = XDocument.Load(stream);
                root = newDoc.XElement("Activity");
                root.XElement("Sequence").RemoveAll();

                // Add arguments for new workflow
                if (root.XElement("Members", XMLExetension.ns_x) == null)
                {
                    var e = new XElement(XName.Get("Members", XMLExetension.ns_x));
                    root.AddFirst(e);
                }
                var nMembers = root.XElement("Members", XMLExetension.ns_x);
                foreach (var v in variables)
                {
                    var pt = XMLExetension.ParseElementFromTemplate("Property", v.Attribute("Name").Value, v.XAttribute("TypeArguments", XMLExetension.ns_x).Value);
                    nMembers.Add(pt);
                }
                var eleWorkflow = XMLExetension.ParseElementFromTemplate("InvokeWorkflowFile", newWorkflowName);
                foreach (var m in nMembers.Elements())
                {
                    var args = eleWorkflow.XElement("InvokeWorkflowFile.Arguments", XMLExetension.ns_ui);
                    var type = m.Attribute("Type").Value;
                    var name = m.Attribute("Name").Value;
                    type = type.Substring(type.IndexOf('(') + 1, type.LastIndexOf(')') - type.IndexOf('(') - 1);

                    var direction = "InOut";
                    args.Add(XMLExetension.ParseElement(string.Format("<{2}Argument x:TypeArguments=\"{0}\" x:Key=\"{1}\" >[{1}]</{2}Argument>", type, name, direction)));
                }

                // Change location of target activity
                WorkContext.Activity.SetAttributeValue("UIPathPreprocessor", "TRUE");
                root.XElement("Sequence").Add(WorkContext.Activity);
                var newEle = newDoc.Root.XPathSelectElement("//*[@UIPathPreprocessor]");
                newEle.Attribute("UIPathPreprocessor").Remove();
                Globals.AddToSaveXMLFile(p, newDoc);

                WorkContext.Activity.ReplaceWith(eleWorkflow);
                WorkContext.Activity = newEle;
                WorkContext.Doc = newDoc;
            }
        }
    }
}
