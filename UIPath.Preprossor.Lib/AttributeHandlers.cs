using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIPath.Preprossor.Lib
{
    public static class AttributeHandlers
    {
        public static List<AttributeHanlder> Handlers { get; set; } = new List<AttributeHanlder>();

        public static void Load()
        {
            Handlers.Add(new WrapperAttributeHandler());
            Handlers.Add(new WorkflowAttributeHandler());
        }
    }
}
