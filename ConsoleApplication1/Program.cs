using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using UIPath.Preprossor.Lib;

namespace ConsoleApplication1
{
    class Program
    {
        public static XDocument CurDoc { get; set; }

        public static string FileName { get; set; }

        static void Main(string[] args)
        {
            // 1. Load workflow
            // 2. Find annotations contains attributes
            // 3. Handle these attributes
            // 4. Output new workflow
            AttributeHandlers.Load();
            var doc = LoadWorkflow(@"C:\Users\bmao002\Documents\UiPath\test1\Main1.xaml");
            var mainSeq = doc.Elements().First().Elements().Single(e => e.Name.LocalName == "Sequence");
            var activities = new List<XElement>();
            DFSActvities(mainSeq, activities);
            foreach (var activity in activities)
            {
                if (activity.XAttribute("WorkflowViewState.IdRef", XMLExetension.ns_sap2010) != null)
                {
                    var attr = activity.XAttribute("Annotation.AnnotationText", XMLExetension.ns_sap2010);
                    var attrs = new Dictionary<string, string>();
                    if (attr != null)
                    {
                        foreach (var line in attr.Value.Split('\n'))
                        {
                            var match = Regex.Match(line.Trim(), @"^@(\S+)\((.*)\)$");
                            if (match.Length >= 3)
                            {
                                attrs.Add(match.Groups[1].Value, match.Groups[2].Value);
                            }
                        }
                    }

                    foreach (var h in AttributeHandlers.Handlers)
                    {
                        if (attrs.Keys.Contains(h.Name))
                        {
                            var method = h.GetType().GetMethod("Handle");
                            var set_workItem = h.GetType().GetProperty("WorkItem");
                            set_workItem.SetValue(h, new WorkItem()
                            {
                                Doc = CurDoc,
                                Ele = activity,
                                FileName = FileName,
                                WorkingPath = @"C:\Users\bmao002\Documents\UiPath\test1"
                            });

                            var ps = method.GetParameters();
                            var margs = ArgumentsResolver.Resolve(attrs[h.Name]);
                            for (int i = 0; i < margs.Length; i++)
                            {
                                var marg = margs[i];
                                if (marg.ToString().StartsWith("$$"))
                                    marg = h.WorkItem.Ele.XAttribute(marg.ToString().Substring(2)).Value;
                                //h.WorkItem.Ele.XPathSelectElement()
                            }

                            if (ps.LastOrDefault()?.ParameterType?.IsArray ?? false)
                            {
                                var eleType = ps.LastOrDefault().ParameterType.GetElementType();
                                var paramsLen = margs.Length - ps.Length + 1;
                                var paramsObj = Array.CreateInstance(eleType, paramsLen);
                                for (int i = ps.Length - 1; i < margs.Length; i++)
                                {
                                    paramsObj.SetValue(margs[i], i - (ps.Length - 1));
                                }
                                var _args = margs.Take(ps.Length - 1).ToList();
                                _args.Add(paramsObj);
                                method.Invoke(h, _args.ToArray());
                            }
                            else
                            {
                                method.Invoke(h, margs);
                            }
                        }
                    }
                }
            }

            doc.Save(@"C:\Users\bmao002\Documents\UiPath\test1\Main1_1.xaml");
        }

        static void DFSActvities(XElement ele, List<XElement> activities)
        {
            foreach (var activity in ele.Elements())
            {
                if (activity.HasElements)
                {
                    DFSActvities(activity, activities);
                }
                activities.Add(activity);
            }
        }

        static XDocument LoadWorkflow(string fileName)
        {
            CurDoc = XDocument.Load(File.OpenRead(fileName));
            FileName = fileName;
            return CurDoc;
        }
    }
}
