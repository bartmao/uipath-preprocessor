using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UIPathInvoker
{
    public partial class Invoker : Form
    {
        public Invoker()
        {
            InitializeComponent();
        }

        private void btnOpenProj_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    label1.Text = fbd.SelectedPath;
                    ConstructTreeNodeW(label1.Text);
                }
            }
        }

        private void ConstructTreeNodes(TreeNode n)
        {
            var path = n.Tag.ToString();
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                var n1 = new TreeNode(dir.Replace(path + "\\", ""));
                n1.Tag = dir;
                n.Nodes.Add(n1);
                ConstructTreeNodes(n1);
            }
            foreach (var file in Directory.GetFiles(path))
            {
                var n2 = new TreeNode(file.Replace(path + "\\", ""));
                n2.Tag = file;
                n.Nodes.Add(n2);
            }
        }

        private void btnCompile_Click(object sender, EventArgs e)
        {
            var pi = new ProcessStartInfo();
            //pi.FileName = @"C:\Users\bmao002\AppData\Local\UiPath\app-18.2.3\UIRobot.exe";
            //pi.Arguments = @"-f C:\Users\bmao002\Documents\UiPath\test1\Main1.1.xaml";
            pi.FileName = Environment.CurrentDirectory + "\\Compiler.exe";
            pi.RedirectStandardOutput = true;
            pi.RedirectStandardError = true;
            pi.UseShellExecute = false;
            pi.CreateNoWindow = true;
            if (tv.SelectedNode != null && tv.SelectedNode.Text.EndsWith(".xaml"))
            {
                pi.Arguments = tv.SelectedNode.Tag.ToString();
            }
            else
            {
                pi.Arguments = label1.Text + "\\project.json";
            }

            var p = new Process();
            p.StartInfo = pi;
            p.ErrorDataReceived += P_ErrorDataReceived;
            p.OutputDataReceived += P_OutputDataReceived;
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();



        }

        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string line = e.Data;
            if (line != null)
            {
                richTextBox1.BeginInvoke(new Action(() =>
                {
                    var s = richTextBox1.Text.Length;
                    richTextBox1.AppendText(line + Environment.NewLine);
                    richTextBox1.SelectionStart = s;
                    richTextBox1.SelectionLength = line.Length;
                    richTextBox1.SelectionColor = Color.ForestGreen;
                }));
            }

        }

        private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            string line = e.Data;
            if (line != null)
            {
                var s = richTextBox1.Text.Length;
                richTextBox1.AppendText(line + Environment.NewLine);
                richTextBox1.SelectionStart = s;
                richTextBox1.SelectionLength = line.Length;
                richTextBox1.SelectionColor = Color.Red;
            }
        }

        private void ConstructTreeNodeW(string folderPath)
        {
            var root = new TreeNode(Path.GetFileName(folderPath));
            root.Tag = folderPath;
            ConstructTreeNodes(root);
            tv.Nodes.Add(root);
        }

        private void tv_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void tv_DragDrop(object sender, DragEventArgs e)
        {
            var fs = e.Data.GetData("FileNameW") as string[];
            if (fs != null && fs.Length > 0)
            {
                tv.Nodes.Clear();
                ConstructTreeNodeW(fs[0]);
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            btnRun.Enabled = false;
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    // Listen for running
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    var ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10086);
                    socket.Bind(ip);
                    socket.Listen(0);

                    var conn = socket.Accept();
                    using (var r = new StreamReader(new NetworkStream(conn)))
                    {
                        var msg = r.ReadLine();
                        
                        while (msg != null)
                        {
                            richTextBox1.BeginInvoke((Action)(() =>
                            {
                                richTextBox1.AppendText(msg + Environment.NewLine);
                            }));
                            Debug.WriteLine(msg);
                            msg = r.ReadLine();
                        }
                    }
                    socket.Dispose();
                    btnRun.Invoke((Action)(() => btnRun.Enabled = true));
                }
                catch (Exception)
                {
                    throw;
                }
            }, null);

            return;


            var pi = new ProcessStartInfo();
            pi.FileName = @"C:\Users\bmao002\AppData\Local\UiPath\app-18.2.3\UIRobot.exe";
            pi.Arguments = "-f \"C:\\Users\\bmao002\\Desktop\\New folder\\TestPreprocessor\\Main.xaml\"";
            pi.RedirectStandardOutput = true;
            pi.RedirectStandardError = true;
            pi.UseShellExecute = false;
            pi.CreateNoWindow = true;
            var p = new Process();
            p.ErrorDataReceived += P_ErrorDataReceived;
            p.OutputDataReceived += P_OutputDataReceived;
            p.StartInfo = pi;
            p.Start();
        }
    }
}
