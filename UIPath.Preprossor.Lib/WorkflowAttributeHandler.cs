﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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

            var n = WorkItem.Ele.Parent;
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

            var p = Path.Combine(WorkItem.WorkingPath, newWorkflowName + ".xaml");
            if (File.Exists(p)) throw new Exception("Workflow file existed:" + p);
            File.Copy(WorkItem.FileName, p, true);
            XDocument newDoc = null;
            XElement mainNode;
            using (var stream = File.OpenRead(p))
            {
                newDoc = XDocument.Load(stream);
                mainNode = newDoc.XElement("Activity");
                mainNode.XElement("Sequence").RemoveAll();
                mainNode.XElement("Sequence").Add(WorkItem.Ele);
            }

            if (mainNode.XElement("Members", XMLExetension.ns_x) == null)
            {
                var e = new XElement(XName.Get("Members", XMLExetension.ns_x));
                mainNode.AddFirst(e);
            }
            var nMembers = mainNode.XElement("Members", XMLExetension.ns_x);
            foreach (var v in variables)
            {
                var pt = XMLExetension.ParseElementFromTemplate("Property", v.Attribute("Name").Value, v.XAttribute("TypeArguments", XMLExetension.ns_x).Value);
                nMembers.Add(pt);
            }
            newDoc.Save(p);

            var eleWorkflow = XMLExetension.ParseElementFromTemplate("InvokeWorkflowFile", newWorkflowName);
            foreach (var m in nMembers.Elements())
            {
                var args = eleWorkflow.XElement("InvokeWorkflowFile.Arguments", XMLExetension.ns_ui);
                var type = m.Attribute("Type").Value;
                var name = m.Attribute("Name").Value;
                type = type.Substring(type.IndexOf('(') + 1, type.LastIndexOf(')') - type.IndexOf('(') - 1);

                var direction = "In";
                //if (m.Attribute("Type").Value.StartsWith("InArgument"))
                //{
                //    direction = "In";
                //}
                //else if (m.Attribute("Type").Value.StartsWith("OutArgument"))
                //{
                //    direction = "Out";
                //}
                //else if (m.Attribute("Type").Value.StartsWith("InOutArgument"))
                //{
                //    direction = "InOut";
                //}
                direction = "InOut";
                args.Add(XMLExetension.ParseElement(string.Format("<{2}Argument x:TypeArguments=\"{0}\" x:Key=\"{1}\" >[{1}]</{2}Argument>", type, name, direction)));
            }
            WorkItem.Ele.ReplaceWith(eleWorkflow);
        }
    }
}