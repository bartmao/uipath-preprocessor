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
            ActivityHandlers.Load();
            var doc = LoadWorkflow(@"C:\Users\bmao002\Documents\UiPath\test1\Main1.1.xaml");
            var mainSeq = doc.Elements().First().Elements().Single(e => e.Name.LocalName == "Sequence");
            var activities = new List<XElement>();
            DFSActvities(mainSeq, activities);
            foreach (var activity in activities)
            {
                if (activity.XAttribute("WorkflowViewState.IdRef", XMLExetension.ns_sap2010) != null)
                {
                    activity.SetAttributeValue("UIPathPreprocessor", "TRUE");

                    var attr = activity.XAttribute("Annotation.AnnotationText", XMLExetension.ns_sap2010);
                    var attrs = new List<Tuple<string, string>>();
                    if (attr != null)
                    {
                        foreach (var line in attr.Value.Split('\n'))
                        {
                            var match = Regex.Match(line.Trim(), @"^@(\S+)\((.*)\)$");
                            if (match.Length >= 3)
                            {
                                attrs.Add(Tuple.Create(match.Groups[1].Value, match.Groups[2].Value));
                            }
                        }
                    }

                    var workitem = new WorkItem()
                    {
                        Doc = CurDoc,
                        FileName = FileName,
                        WorkingPath = @"C:\Users\bmao002\Documents\UiPath\test1",
                        Attributes = attrs
                    };
                    foreach (var h in ActivityHandlers.TypedHandlers)
                    {
                        if (h.Test(activity, attrs))
                        {
                            h.WorkItem = workitem;
                            h.Handle();
                        }
                    }

                    foreach (var attrTuple in attrs)
                    {
                        var attribute = attrTuple.Item1;
                        var h = ActivityHandlers.AttributeHandlers.FirstOrDefault(h1 => h1.Name == attribute);
                        if (h == null) {
                            Console.WriteLine("No handler for " + attribute);
                            continue;
                        }

                        Console.WriteLine("Processing attribute:" + attribute);
                        var method = h.GetType().GetMethod("Handle");
                        h.WorkItem = workitem;

                        var ps = method.GetParameters();
                        var margs = new ArgumentsResolver(activity).Resolve(attrTuple.Item2);

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

                    // the target activity could updated by the hanlder
                    workitem.GetActivity().Attribute("UIPathPreprocessor").Remove();
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
