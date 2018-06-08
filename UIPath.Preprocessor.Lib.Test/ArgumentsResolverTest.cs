using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UIPath.Preprossor.Lib;

namespace UIPath.Preprocessor.Lib.Test
{
    [TestClass]
    public class ArgumentsResolverTest
    {
        [TestMethod]
        public void TestResolve()
        {
            _TestResolve("");
            _TestResolve("\"\",\"\"");
            _TestResolve("\"test\",\"test\"");
            _TestResolve("12345.000,\"test\"");
            _TestResolve("Nothing,\"test\"");
            _TestResolve("False,\"test\"");
            _TestResolve("False, 12345.00");
            _TestResolve("Nothing, Nothing");
            _TestResolve("\"\\\"\", Nothing");
            _TestResolve(" \"\\\\\", Nothing");
            _TestResolve("\"\\tTest\\t\\\"\", Nothing");
        }

        public void _TestResolve(string str)
        {
            Console.WriteLine(str);
            var paras = ArgumentsResolver.Resolve(str);
            foreach (var p in paras)
            {
                Console.WriteLine("Parameter: " + p?.ToString() ?? "");
            }
            Console.WriteLine(new string('=', 100));
        }
    }
}
