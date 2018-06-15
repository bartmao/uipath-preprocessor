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
        //public static XDocument CurDoc { get; set; }

        //public static string FileName { get; set; }

        static void Main(string[] args)
        {
            args = new string[1];
            args[0] = @"C:\Users\bmao002\Documents\UiPath\test1\project.json";
            if (args.Length < 1) return;

            ActivityHandlers.Load();
            if (args[0].EndsWith(".xaml"))
            {
                ProcessWorkflow(args[0], Path.GetDirectoryName(args[0]));
            }
            else if (args[0].EndsWith("project.json"))
            {
                var projDir = Path.GetDirectoryName(args[0]);
                var outProjDir = Path.GetDirectoryName(projDir) + "\\" + Path.GetFileName(projDir) + "_Out";
                if (Directory.Exists(outProjDir))
                    DeleteDirectory(outProjDir);
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(projDir, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(projDir, outProjDir));
                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(projDir, "*.*", SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(projDir, outProjDir), true);

                foreach (string newPath in Directory.GetFiles(outProjDir, "*.xaml", SearchOption.AllDirectories))
                    ProcessWorkflow(newPath, outProjDir);
            }
        }

        static void ProcessWorkflow(string workflowFile, string workingFolder)
        {
            var doc = XDocument.Load(workflowFile);

            var mainSeq = doc.Elements().First().Elements().Single(e => e.Name.LocalName == "Sequence");
            var activities = new List<XElement>();
            DFSActvities(mainSeq, activities);

            #region process activities
            foreach (var activity in activities)
            {
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

                var workContext = new WorkContext(activity)
                {
                    Doc = doc,
                    FileName = workflowFile,
                    WorkingPath = workingFolder,
                    Attributes = attrs
                };
                foreach (var h in ActivityHandlers.TypedHandlers)
                {
                    if (h.Test(activity, attrs))
                    {
                        h.WorkContext = workContext;
                        h.Handle();
                    }
                }

                foreach (var attrTuple in attrs)
                {
                    var attribute = attrTuple.Item1;
                    var h = ActivityHandlers.AttributeHandlers.FirstOrDefault(h1 => h1.Name == attribute);
                    if (h == null)
                    {
                        Console.WriteLine("No handler for " + attribute);
                        continue;
                    }

                    Console.WriteLine("Processing attribute:" + attribute);
                    var method = h.GetType().GetMethod("Handle");
                    h.WorkContext = workContext;

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
            }
            #endregion

            doc.Save(workflowFile);
            foreach (var file in Globals.ToSaveXMLFiles.Keys)
            {
                Globals.ToSaveXMLFiles[file].Save(file);
            }
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

        static void DeleteDirectory(string dir)
        {
            foreach (string newPath in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                File.Delete(newPath);
            foreach (string newPath in Directory.GetDirectories(dir, "*.*", SearchOption.AllDirectories))
                Directory.Delete(newPath);
        }
    }
}
