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
using System.Text.RegularExpressions;

namespace UIPath.Preprocessor.Lib.Test
{
    [TestClass]
    public class XPathTest
    {
        [TestMethod]
        public void TestXPath()
        {
            var f = @"C:\Users\bmao002\Documents\UiPath\test1\Main1.1.xaml";
            var projDir = Path.GetDirectoryName(f);
            var outProjDir = Path.GetDirectoryName(projDir) + "out";
        }
    }
}
