using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UIPath.Preprossor.Lib
{
    public class Globals
    {
        public static Dictionary<string, XDocument> ToSaveXMLFiles = new Dictionary<string, XDocument>();

        public static List<Action> CallbackList = new List<Action>();

        public static void AddToSaveXMLFile(string xml, XDocument doc)
        {
            if (!ToSaveXMLFiles.ContainsKey(xml))
            {
                ToSaveXMLFiles.Add(xml, doc);
            }
        }
    }
}
