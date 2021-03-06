﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UIPath.Preprossor.Lib
{
    public abstract class TypedActivityHandler : ActivityHandler
    {
        public string RegexFilter { get; set; }

        public TypedActivityHandler(string regexFilter)
        {
            RegexFilter = regexFilter;
        }

        public override bool Test()
        {
            var activity = WorkContext.Activity;
            var attrs = WorkContext.Attributes;
            return Regex.IsMatch(activity.Name.LocalName, "^" + RegexFilter + "$") && activity.Name.LocalName != "DebugSymbol.Symbol";
        }

        public abstract void Handle();
    }
}
