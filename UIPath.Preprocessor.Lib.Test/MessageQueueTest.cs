using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIPath.Preprocessor.Lib.Test
{
    [TestClass]
    public class MessageQueueTest
    {
        [TestMethod]
        public void TestConcurrency()
        {
            var pi1 = new ProcessStartInfo();
            pi1.FileName = Path.Combine(Environment.CurrentDirectory, "Communicator.exe");
            pi1.Arguments = @"C:\Personal\communicator.data" + " w 0 test";
            pi1.RedirectStandardOutput = true;
            pi1.UseShellExecute = false;
            var p = Process.Start(pi1);
            Console.WriteLine("1");
            Console.Out.Flush();
            p.WaitForExit();

            //var pi2 = new ProcessStartInfo();
            //pi2.FileName = Path.Combine(Environment.CurrentDirectory, "Communicator.exe");
            //pi2.Arguments = @"C:\Personal\communicator.data" + " r";
            //Process.Start(pi2);
        }
    }
}
