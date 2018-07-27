using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UIPath.Preprossor.Lib
{
    public static class ActivityHandlers
    {
        public static List<TypedActivityHandler> TypedHandlers { get; set; } = new List<TypedActivityHandler>();

        public static List<AttributeHanlder> AttributeHandlers { get; set; } = new List<AttributeHanlder>();

        public static void Load()
        {
            var dlls = Directory.GetFiles(Environment.CurrentDirectory, "*.dll");
            foreach (var dll in dlls)
            {
                if (Regex.IsMatch(dll, ".*\\.*Hanlders?.dll"))
                {
                    Console.WriteLine("Load Assembly: " + dll);
                    Assembly.LoadFile(dll);
                }
            }

            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = ass.GetTypes();
                foreach (var type in types)
                {
                    if (typeof(TypedActivityHandler).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        if (type == typeof(AttributeControllerActivityHandler) && File.Exists(Environment.CurrentDirectory + "\\AttributesControllers.properties"))
                        {
                            var txt = File.ReadAllText(Environment.CurrentDirectory + "\\AttributesControllers.properties");
                            foreach (var line in txt.Split('\n'))
                            {
                                var args = line.Trim().Split('\t');
                                if (args.Length == 2)
                                {
                                    TypedHandlers.Add(new AttributeControllerActivityHandler(args[1].Trim(), args[0].Trim()));
                                }
                            }
                        }
                        else
                        {
                            TypedHandlers.Add((Activator.CreateInstance(type) as TypedActivityHandler));
                        }
                    }
                    else if (typeof(AttributeHanlder).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        AttributeHandlers.Add(Activator.CreateInstance(type) as AttributeHanlder);
                    }
                }
            }
        }
    }
}
