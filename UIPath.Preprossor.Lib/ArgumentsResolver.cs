using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace UIPath.Preprossor.Lib
{
    public class ArgumentsResolver
    {
        public XElement Activity { get; set; }

        public ArgumentsResolver(XElement activity)
        {
            Activity = activity;
        }

        /// <summary>
        /// Accept types:
        /// 1.String 2.Number 3.Nothing 4.Boolean 5.WF variable($xxx) 6.Activity attribute($$xxx)
        /// </summary>
        /// <param name="paraString"></param>
        /// <returns></returns>
        public object[] Resolve(string paraString)
        {
            var parameters = new List<object>();
            if (string.IsNullOrWhiteSpace(paraString)) return parameters.ToArray();

            var sb = new StringBuilder();
            bool canParse = false;
            bool isStr = false;
            char leading = '\0';
            for (int i = 0; i < paraString.Length; i++)
            {
                if (canParse)
                {
                    parameters.Add(GetParameter(sb));
                    sb = new StringBuilder();
                    canParse = false;
                }

                var c = paraString[i];
                if (c == '\\' && leading != '\\')
                {
                    leading = c;
                    continue;
                }

                if (leading == '\\' && isStr)
                {
                    if (c == '"') sb.Append(c);
                    else if (c == 't') sb.Append('\t');
                    else if (c == 'n') sb.Append('\n');
                    else if (c == 'r') sb.Append('\r');
                    else if (c == '\\') sb.Append('\\');
                    else if (c == 'b') sb.Append('\b');
                    else if (c == '0') sb.Append('\0');
                    else sb.Append("\\" + c);

                    leading = '\0';
                }
                else
                {
                    if (c == '"' && !isStr)
                    {
                        isStr = true;
                    }
                    else if (c == '"' && isStr)
                    {
                        isStr = false;
                    }

                    if (isStr)
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        if (c == ',')
                        {
                            canParse = true;
                        }
                        else if (c == ' ')
                        {
                            // Skip
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }

                    leading = paraString[i];
                }
            }

            parameters.Add(GetParameter(sb));

            return parameters.ToArray();
        }

        private object GetParameter(StringBuilder sb)
        {
            double v;
            if (sb.Length >= 2 && sb[0] == '"' && sb[sb.Length - 1] == '"') return sb.ToString(1, sb.Length - 2);
            else if (sb.ToString().ToLower() == "false" || sb.ToString().ToLower() == "true") return bool.Parse(sb.ToString());
            else if (sb.ToString().ToLower() == "nothing" || sb.ToString() == "") return null;
            else if (double.TryParse(sb.ToString(), out v)) return v;
            else if (Regex.IsMatch(sb.ToString(), @"^\[.*\]$")) return sb.ToString();
            else if (Regex.IsMatch(sb.ToString(), "^\\$\\\".*\\\"$"))
            {
                try
                {
                    var attr = (IEnumerable)Activity.XPathEvaluate(sb.ToString(2, sb.Length - 3), XMLExetension.NSManager);
                    var attrVal =  XMLExetension.Escape(attr.Cast<XAttribute>().First().Value);
                    Console.WriteLine(attrVal);
                    return attrVal;
                }
                catch (InvalidOperationException)
                {
                    throw new Exception("Invalid XPath expression");
                }

            }

            throw new InvalidCastException("Failed to resolve paramter: " + sb.ToString());
        }
    }
}
