﻿namespace UIPathInvoker
{
    partial class Invoker
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOpenProj = new System.Windows.Forms.Button();
            this.tv = new System.Windows.Forms.TreeView();
            this.btnCompile = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOpenProj
            // 
            this.btnOpenProj.Location = new System.Drawing.Point(22, 21);
            this.btnOpenProj.Name = "btnOpenProj";
            this.btnOpenProj.Size = new System.Drawing.Size(92, 23);
            this.btnOpenProj.TabIndex = 0;
            this.btnOpenProj.Text = "Open Project";
            this.btnOpenProj.UseVisualStyleBackColor = true;
            this.btnOpenProj.Click += new System.EventHandler(this.btnOpenProj_Click);
            // 
            // tv
            // 
            this.tv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tv.Location = new System.Drawing.Point(12, 84);
            this.tv.Name = "tv";
            this.tv.Size = new System.Drawing.Size(342, 163);
            this.tv.TabIndex = 1;
            // 
            // btnCompile
            // 
            this.btnCompile.Location = new System.Drawing.Point(137, 21);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(92, 23);
            this.btnCompile.TabIndex = 2;
            this.btnCompile.Text = "Compile";
            this.btnCompile.UseVisualStyleBackColor = true;
            this.btnCompile.Click += new System.EventHandler(this.btnCompile_Click);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(254, 21);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(92, 23);
            this.btnRun.TabIndex = 3;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 4;
            // 
            // Invoker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(366, 268);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnCompile);
            this.Controls.Add(this.tv);
            this.Controls.Add(this.btnOpenProj);
            this.Name = "Invoker";
            this.Text = "Invoker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOpenProj;
        private System.Windows.Forms.TreeView tv;
        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Label label1;
    }
}

