using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UIPath.Preprossor.Lib
{
    public class AttributeControllerActivityHandler : TypedActivityHandler
    {
        public string AttributeExp { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="attrExp">[+|-]@attribute(.*)</param>
        /// <param name="regexFilter"></param>
        public AttributeControllerActivityHandler(string attrExp, string regexFilter) : base(regexFilter)
        {
            AttributeExp = attrExp;
        }

        public override void Handle()
        {
            var isAdd = AttributeExp.StartsWith("+");
            var activity = WorkContext.Activity;
            var annotation = activity.XAttribute("Annotation.AnnotationText", XMLExetension.ns_sap2010);
            var attrName = AttributeExp.Substring(2, AttributeExp.IndexOf('(') - 2);
            var attrValue = "";
            if (!AttributeExp.EndsWith("()"))
            {
                attrValue = AttributeExp.Substring(AttributeExp.IndexOf('(') + 1, AttributeExp.LastIndexOf(')') - AttributeExp.IndexOf('(') - 1);
            }

            if (isAdd)
            {
                if (annotation == null)
                {
                    activity.SetAttributeValue(XName.Get("Annotation.AnnotationText", XMLExetension.ns_sap2010), AttributeExp);
                    annotation = activity.XAttribute("Annotation.AnnotationText", XMLExetension.ns_sap2010);
                }
                else
                {
                    // If not exist the attribute, add it
                    if (WorkContext.Attributes.FirstOrDefault(t => t.Item1 == attrName && t.Item2 == attrValue) == null)
                    {
                        annotation.Value = annotation.Value + Environment.NewLine + AttributeExp.Substring(1);
                    }
                }

                WorkContext.Attributes.Add(Tuple.Create(attrName, attrValue));
            }
            else
            {
                if (annotation != null)
                {
                    // remove attribute
                    var existAttr = WorkContext.Attributes.FirstOrDefault(t => t.Item1 == attrName);
                    if (existAttr != null)
                    {
                        WorkContext.Attributes.RemoveAll(t => t.Item1 == existAttr.Item1 && t.Item2 == existAttr.Item2);
                        var arr = annotation.Value.Split('\n').Where(v => !v.Trim().StartsWith("@" + attrName)).Select(v => v.Trim());
                        annotation.Value = string.Join(Environment.NewLine, arr);
                    }
                }
            }

        }
    }
}
