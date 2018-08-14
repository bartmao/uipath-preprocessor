using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UIPath.Preprocessor.Lib.Test
{
    [TestClass]
    public class MessageQueueTest
    {
        [TestMethod]
        public void TestConcurrency()
        {
            var fComm = Path.Combine(Environment.CurrentDirectory, "Communicator.exe");
            int counter_r = 1;
            int counter_w = 1;
            var exitSignal = new ManualResetEvent(false);

            var pwi = new ProcessStartInfo();
            pwi.FileName = fComm;
            pwi.Arguments = @"C:\Personal\communicator.data" + " w 0 test";
            pwi.RedirectStandardOutput = true;
            pwi.UseShellExecute = false;

            var pri = new ProcessStartInfo();
            pri.FileName = fComm;
            pri.Arguments = @"C:\Personal\communicator.data" + " r";
            pri.RedirectStandardOutput = true;
            pri.UseShellExecute = false;

            ThreadPool.QueueUserWorkItem(o =>
            {
                while (--counter_r >= 0)
                {
                    Process pr = null;
                    try
                    {
                        Console.WriteLine("Start new process for read");
                        pr = Process.Start(pri);
                        Console.Write(pr.StandardOutput.ReadToEnd());
                        pr.WaitForExit();
                    }
                    finally
                    {
                        if(pr != null)
                            Console.Write(pr.StandardOutput.ReadToEnd());
                    }
                }

                if (counter_r <= 0 && counter_w <= 0) exitSignal.Set();
            }, null);

            ThreadPool.QueueUserWorkItem(o =>
            {
                while (--counter_w >= 0)
                {
                    Process pw = null;
                    try
                    {
                        Console.WriteLine("Start new process for write");
                        pw = Process.Start(pwi);
                        Console.Write(pw.StandardOutput.ReadToEnd());
                        pw.WaitForExit();
                    }
                    finally
                    {
                        Console.Write(pw.StandardOutput.ReadToEnd());
                    }

                }

                if (counter_r <= 0 && counter_w <= 0) exitSignal.Set();
            }, null);

            exitSignal.WaitOne();
        }
    }
}
