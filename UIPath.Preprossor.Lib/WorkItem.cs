using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace UIPath.Preprossor.Lib
{
    public class WorkContext
    {
        public XDocument Doc { get; set; }

        public string FileName { get; set; }

        public string WorkingPath { get; set; }

        public List<Tuple<string, string>> Attributes { get; set; }

        public XElement Activity { get; set; }

        public string TargetFile { get; set; }

        public XElement WorkflowTarget { get; set; }

        public XElement Target { get; set; }

        public string NewFileForOriginalActivity { get; set; }

        // Only for case when new workflow is genreated for keeping this activity
        public XElement OriginalActivity { get; set; }

        public WorkContext(XElement activity)
        {
            Doc = activity.Document;
            Activity = activity;
            OriginalActivity = activity;
            Target = activity;
        }

        public void MoveActivity(XElement container, string containerFile)
        {
            if (Activity == OriginalActivity)
            {
                container.Add(Activity);
                OriginalActivity = container.Elements().Last();
                NewFileForOriginalActivity = containerFile;
            }
            else
            {
                throw new Exception("Original activity is already moved");
            }
        }

        public void WrapTarget(XElement wrapper)
        {
            var replacement = wrapper.XPathSelectElement(".//*[@DisplayName=\"Placeholder\"]");//  wdoc.Descendants().SingleOrDefault(e => e.XAttribute("DisplayName")?.Value == "Placeholder");
            if (replacement != null)
            {
                Activity.SetAttributeValue("UIPathPreprocessor", "TRUE");
                replacement.ReplaceWith(Activity);
                Activity.ReplaceWith(wrapper);
                Activity = Doc.XPathSelectElement("//*[@UIPathPreprocessor]");
                Activity.Attribute("UIPathPreprocessor").Remove();
            }
            else
            {
                wrapper.SetAttributeValue("UIPathPreprocessor", "TRUE");
                Activity.ReplaceWith(wrapper);
                var newActivity = Doc.XPathSelectElement("//*[@UIPathPreprocessor]");
                newActivity.Attribute("UIPathPreprocessor").Remove();
                Activity = newActivity;
            }
        }
    }
}
