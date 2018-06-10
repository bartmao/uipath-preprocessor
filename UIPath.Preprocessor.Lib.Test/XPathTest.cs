using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Linq;
using UIPath.Preprossor.Lib;
using System.Collections;

namespace UIPath.Preprocessor.Lib.Test
{
    [TestClass]
    public class XPathTest
    {
        [TestMethod]
        public void TestXPath()
        {
            var doc = XDocument.Load(File.OpenRead(@"C:\Users\bmao002\Documents\UiPath\test1\Main1.xaml"));
            var member1 = doc.Root.XPathSelectElement("a:Sequence/a:Variables", XMLExetension.NSManager);
            var member2 = doc.Root.XPathSelectElement("//*[@sap2010:WorkflowViewState.IdRef=\"Click_1\"]", XMLExetension.NSManager);
            var member23 = (IEnumerable)doc.Root.XPathEvaluate("//*[@sap2010:WorkflowViewState.IdRef=\"Click_1\"]/ui:Click.Target/ui:Target/@Selector", XMLExetension.NSManager);
            //var ele = ()doc.Root.XPathEvaluate("//*[@sap2010:WorkflowViewState.IdRef=\"Click_1\"]/ui:Click.Target/ui:Target/@Selector", XMLExetension.NSManager);
            var node = member23.Cast<XAttribute>().FirstOrDefault();
        }
    }
}
