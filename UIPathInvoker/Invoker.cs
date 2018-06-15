using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
                    var root = new TreeNode(Path.GetFileName(label1.Text));
                    root.Tag = fbd.SelectedPath;
                    ConstructTreeNodes(root);
                    tv.Nodes.Add(root);
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
            if (tv.SelectedNode != null && tv.SelectedNode.Text.EndsWith(".xaml"))
                Process.Start("Compiler.exe", tv.SelectedNode.Tag.ToString());
            else
                Process.Start("Compiler.exe", label1.Text + "\\project.json");
        }
    }
}
