using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UIPath.Preprossor.Lib
{
    public abstract class ActivityHandler
    {
        public WorkContext WorkContext { get; set; }

        public abstract bool Test();
    }
}
