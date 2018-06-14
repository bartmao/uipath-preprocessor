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
    public static class XMLExetension
    {
        public const string ns_sap2010 = "http://schemas.microsoft.com/netfx/2010/xaml/activities/presentation";
        public const string ns_x = "http://schemas.microsoft.com/winfx/2006/xaml";
        public const string ns_ui = "http://schemas.uipath.com/workflow/activities";

        public static XmlNamespaceManager NSManager = new XmlNamespaceManager(new NameTable());

        static XMLExetension()
        {
            NSManager.AddNamespace("", "http://schemas.microsoft.com/netfx/2009/xaml/activities");
            NSManager.AddNamespace("a", "http://schemas.microsoft.com/netfx/2009/xaml/activities");
            NSManager.AddNamespace("sap2010", ns_sap2010);
            NSManager.AddNamespace("x", ns_x);
            NSManager.AddNamespace("ui", ns_ui);
        }

        public static XElement ParseElement(string strXml)
        {
            XmlParserContext parserContext = new XmlParserContext(null, XMLExetension.NSManager, null, XmlSpace.None);
            XmlTextReader txtReader = new XmlTextReader(strXml, XmlNodeType.Element, parserContext);
            return System.Xml.Linq.XElement.Load(txtReader);
        }

        public static XElement ParseElementFromTemplate(string template, params string[] placeholders)
        {
            var t = File.ReadAllText(Environment.CurrentDirectory + "\\Templates\\" + template + ".xml");
            return ParseElement(string.Format(t, placeholders));
        }

        public static XAttribute XAttribute(this XElement ele, string name, string ns = "")
        {
            return ele.Attribute(XName.Get(name, ns));
        }

        public static XElement XElement(this XElement ele, string name, string ns = "http://schemas.microsoft.com/netfx/2009/xaml/activities")
        {
            return ele.Element(XName.Get(name, ns));
        }

        public static XElement XElement(this XContainer ele, string name, string ns = "http://schemas.microsoft.com/netfx/2009/xaml/activities")
        {
            return ele.Element(XName.Get(name, ns));
        }

        public static Dictionary<string, XAttribute> GetNamespaces(this XContainer doc)
        {
            var attrs = doc.XElement("Activity").Attributes().Where(a => a.Name.NamespaceName == "http://www.w3.org/2000/xmlns/");
            return attrs.ToDictionary(a => a.Value, a => a);
        }

        public static string Escape(string unescaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }
    }

}
