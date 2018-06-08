using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIPath.Preprossor.Lib
{
    public class ArgumentsResolver
    {
        /// <summary>
        /// Accept types:
        /// 1.String 2.Number 3.Nothing 4.Boolean 5.uipath variable($xxx)
        /// </summary>
        /// <param name="paraString"></param>
        /// <returns></returns>
        public static object[] Resolve(string paraString)
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
                    if (sb.Length >= 2 && sb[0] == '"' && sb[sb.Length - 1] == '"') parameters.Add(sb.ToString(1, sb.Length - 2));
                    else if (sb.ToString().ToLower() == "false" || sb.ToString().ToLower() == "true") parameters.Add(bool.Parse(sb.ToString()));
                    else if (sb.ToString().ToLower() == "nothing" || sb.ToString() == "") parameters.Add(null);
                    else if (sb.Length >= 2 && sb[0] == '$') parameters.Add("[" + sb.ToString().Substring(1) + "]");
                    else parameters.Add(double.Parse(sb.ToString()));
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

            if (sb.Length >= 2 && sb[0] == '"' && sb[sb.Length - 1] == '"') parameters.Add(sb.ToString(1, sb.Length - 2));
            else if (sb.ToString().ToLower() == "false" || sb.ToString().ToLower() == "true") parameters.Add(bool.Parse(sb.ToString()));
            else if (sb.ToString().ToLower() == "nothing" || sb.ToString() == "") parameters.Add(null);
            else if (sb.Length >= 2 && sb[0] == '$') parameters.Add("[" + sb.ToString().Substring(1) + "]");
            else parameters.Add(double.Parse(sb.ToString()));

            return parameters.ToArray();
        }
    }
}
