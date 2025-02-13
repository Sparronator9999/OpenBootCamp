// This file is part of OpenBootCamp.
// Copyright © Sparronator9999 2024-2025.
//
// OpenBootCamp is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// OpenBootCamp is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
//
// You should have received a copy of the GNU General Public License along with
// OpenBootCamp. If not, see <https://www.gnu.org/licenses/>.

namespace OBC.InstallManager
{
    partial class MainForm
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
            System.Windows.Forms.ToolStripMenuItem tsiFile;
            System.Windows.Forms.ToolStripMenuItem tsiExit;
            System.Windows.Forms.ToolStripMenuItem tsiHelp;
            System.Windows.Forms.ToolStripMenuItem tsiAbout;
            System.Windows.Forms.ToolStripMenuItem tsiSource;
            System.Windows.Forms.Label lblKA;
            System.Windows.Forms.Label lblMHD;
            System.Windows.Forms.Label lblOBCS;
            System.Windows.Forms.Label lblOBCO;
            this.lblOBCOState = new System.Windows.Forms.Label();
            this.btnOBCOInstall = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tblDrivers = new System.Windows.Forms.TableLayoutPanel();
            this.lblMHDState = new System.Windows.Forms.Label();
            this.lblKAState = new System.Windows.Forms.Label();
            this.lblOBCSState = new System.Windows.Forms.Label();
            this.btnMHDInstall = new System.Windows.Forms.Button();
            this.btnKAInstall = new System.Windows.Forms.Button();
            this.btnOBCSInstall = new System.Windows.Forms.Button();
            this.btnOBCSStart = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.grpStatus = new System.Windows.Forms.GroupBox();
            tsiFile = new System.Windows.Forms.ToolStripMenuItem();
            tsiExit = new System.Windows.Forms.ToolStripMenuItem();
            tsiHelp = new System.Windows.Forms.ToolStripMenuItem();
            tsiAbout = new System.Windows.Forms.ToolStripMenuItem();
            tsiSource = new System.Windows.Forms.ToolStripMenuItem();
            lblKA = new System.Windows.Forms.Label();
            lblMHD = new System.Windows.Forms.Label();
            lblOBCS = new System.Windows.Forms.Label();
            lblOBCO = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.tblDrivers.SuspendLayout();
            this.grpStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsiFile
            // 
            tsiFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            tsiExit});
            tsiFile.Name = "tsiFile";
            tsiFile.Size = new System.Drawing.Size(37, 20);
            tsiFile.Text = "File";
            // 
            // tsiExit
            // 
            tsiExit.Name = "tsiExit";
            tsiExit.Size = new System.Drawing.Size(93, 22);
            tsiExit.Text = "Exit";
            tsiExit.Click += new System.EventHandler(this.tsiExit_Click);
            // 
            // tsiHelp
            // 
            tsiHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            tsiAbout,
            tsiSource});
            tsiHelp.Name = "tsiHelp";
            tsiHelp.Size = new System.Drawing.Size(44, 20);
            tsiHelp.Text = "Help";
            // 
            // tsiAbout
            // 
            tsiAbout.Name = "tsiAbout";
            tsiAbout.Size = new System.Drawing.Size(139, 22);
            tsiAbout.Text = "About";
            // 
            // tsiSource
            // 
            tsiSource.Name = "tsiSource";
            tsiSource.Size = new System.Drawing.Size(139, 22);
            tsiSource.Text = "Source code";
            // 
            // lblKA
            // 
            lblKA.Anchor = System.Windows.Forms.AnchorStyles.Right;
            lblKA.AutoSize = true;
            lblKA.Location = new System.Drawing.Point(83, 70);
            lblKA.Margin = new System.Windows.Forms.Padding(3);
            lblKA.Name = "lblKA";
            lblKA.Size = new System.Drawing.Size(77, 15);
            lblKA.TabIndex = 0;
            lblKA.Text = "KeyAgent.sys";
            // 
            // lblMHD
            // 
            lblMHD.Anchor = System.Windows.Forms.AnchorStyles.Right;
            lblMHD.AutoSize = true;
            lblMHD.Location = new System.Drawing.Point(57, 101);
            lblMHD.Margin = new System.Windows.Forms.Padding(3);
            lblMHD.Name = "lblMHD";
            lblMHD.Size = new System.Drawing.Size(103, 15);
            lblMHD.TabIndex = 1;
            lblMHD.Text = "MacHALDriver.sys";
            // 
            // lblOBCS
            // 
            lblOBCS.Anchor = System.Windows.Forms.AnchorStyles.Right;
            lblOBCS.AutoSize = true;
            lblOBCS.Location = new System.Drawing.Point(28, 8);
            lblOBCS.Margin = new System.Windows.Forms.Padding(3);
            lblOBCS.Name = "lblOBCS";
            lblOBCS.Size = new System.Drawing.Size(132, 15);
            lblOBCS.TabIndex = 0;
            lblOBCS.Text = "OpenBootCamp service";
            // 
            // lblOBCO
            // 
            lblOBCO.Anchor = System.Windows.Forms.AnchorStyles.Right;
            lblOBCO.AutoSize = true;
            lblOBCO.Location = new System.Drawing.Point(3, 39);
            lblOBCO.Margin = new System.Windows.Forms.Padding(3);
            lblOBCO.Name = "lblOBCO";
            lblOBCO.Size = new System.Drawing.Size(157, 15);
            lblOBCO.TabIndex = 1;
            lblOBCO.Text = "OpenBootCamp overlay app";
            // 
            // lblOBCOState
            // 
            this.lblOBCOState.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblOBCOState.AutoSize = true;
            this.lblOBCOState.Location = new System.Drawing.Point(166, 39);
            this.lblOBCOState.Margin = new System.Windows.Forms.Padding(3);
            this.lblOBCOState.Name = "lblOBCOState";
            this.lblOBCOState.Size = new System.Drawing.Size(92, 15);
            this.lblOBCOState.TabIndex = 5;
            this.lblOBCOState.Text = "Unknown status";
            // 
            // btnOBCOInstall
            // 
            this.btnOBCOInstall.AutoSize = true;
            this.btnOBCOInstall.Location = new System.Drawing.Point(364, 34);
            this.btnOBCOInstall.Name = "btnOBCOInstall";
            this.btnOBCOInstall.Size = new System.Drawing.Size(75, 25);
            this.btnOBCOInstall.TabIndex = 12;
            this.btnOBCOInstall.Text = "Install";
            this.btnOBCOInstall.UseVisualStyleBackColor = true;
            this.btnOBCOInstall.Click += new System.EventHandler(this.btnOBCOInstall_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            tsiFile,
            tsiHelp});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(448, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tblDrivers
            // 
            this.tblDrivers.AutoSize = true;
            this.tblDrivers.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tblDrivers.ColumnCount = 5;
            this.tblDrivers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblDrivers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblDrivers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblDrivers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblDrivers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblDrivers.Controls.Add(lblKA, 0, 2);
            this.tblDrivers.Controls.Add(this.lblMHDState, 1, 3);
            this.tblDrivers.Controls.Add(this.lblKAState, 1, 2);
            this.tblDrivers.Controls.Add(lblMHD, 0, 3);
            this.tblDrivers.Controls.Add(lblOBCS, 0, 0);
            this.tblDrivers.Controls.Add(this.lblOBCSState, 1, 0);
            this.tblDrivers.Controls.Add(lblOBCO, 0, 1);
            this.tblDrivers.Controls.Add(this.lblOBCOState, 1, 1);
            this.tblDrivers.Controls.Add(this.btnMHDInstall, 4, 3);
            this.tblDrivers.Controls.Add(this.btnKAInstall, 4, 2);
            this.tblDrivers.Controls.Add(this.btnOBCOInstall, 4, 1);
            this.tblDrivers.Controls.Add(this.btnOBCSInstall, 4, 0);
            this.tblDrivers.Controls.Add(this.btnOBCSStart, 3, 0);
            this.tblDrivers.Controls.Add(this.label1, 0, 4);
            this.tblDrivers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblDrivers.Location = new System.Drawing.Point(3, 19);
            this.tblDrivers.Name = "tblDrivers";
            this.tblDrivers.RowCount = 6;
            this.tblDrivers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblDrivers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblDrivers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblDrivers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblDrivers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblDrivers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblDrivers.Size = new System.Drawing.Size(442, 146);
            this.tblDrivers.TabIndex = 6;
            // 
            // lblMHDState
            // 
            this.lblMHDState.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMHDState.AutoSize = true;
            this.lblMHDState.Location = new System.Drawing.Point(166, 101);
            this.lblMHDState.Margin = new System.Windows.Forms.Padding(3);
            this.lblMHDState.Name = "lblMHDState";
            this.lblMHDState.Size = new System.Drawing.Size(92, 15);
            this.lblMHDState.TabIndex = 5;
            this.lblMHDState.Text = "Unknown status";
            // 
            // lblKAState
            // 
            this.lblKAState.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKAState.AutoSize = true;
            this.lblKAState.Location = new System.Drawing.Point(166, 70);
            this.lblKAState.Margin = new System.Windows.Forms.Padding(3);
            this.lblKAState.Name = "lblKAState";
            this.lblKAState.Size = new System.Drawing.Size(92, 15);
            this.lblKAState.TabIndex = 4;
            this.lblKAState.Text = "Unknown status";
            // 
            // lblOBCSState
            // 
            this.lblOBCSState.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblOBCSState.AutoSize = true;
            this.lblOBCSState.Location = new System.Drawing.Point(166, 8);
            this.lblOBCSState.Margin = new System.Windows.Forms.Padding(3);
            this.lblOBCSState.Name = "lblOBCSState";
            this.lblOBCSState.Size = new System.Drawing.Size(92, 15);
            this.lblOBCSState.TabIndex = 4;
            this.lblOBCSState.Text = "Unknown status";
            // 
            // btnMHDInstall
            // 
            this.btnMHDInstall.AutoSize = true;
            this.btnMHDInstall.Location = new System.Drawing.Point(364, 96);
            this.btnMHDInstall.Name = "btnMHDInstall";
            this.btnMHDInstall.Size = new System.Drawing.Size(75, 25);
            this.btnMHDInstall.TabIndex = 8;
            this.btnMHDInstall.Text = "Install";
            this.btnMHDInstall.UseVisualStyleBackColor = true;
            this.btnMHDInstall.Click += new System.EventHandler(this.btnMHDInstall_Click);
            // 
            // btnKAInstall
            // 
            this.btnKAInstall.AutoSize = true;
            this.btnKAInstall.Location = new System.Drawing.Point(364, 65);
            this.btnKAInstall.Name = "btnKAInstall";
            this.btnKAInstall.Size = new System.Drawing.Size(75, 25);
            this.btnKAInstall.TabIndex = 6;
            this.btnKAInstall.Text = "Install";
            this.btnKAInstall.UseVisualStyleBackColor = true;
            this.btnKAInstall.Click += new System.EventHandler(this.btnKAInstall_Click);
            // 
            // btnOBCSInstall
            // 
            this.btnOBCSInstall.AutoSize = true;
            this.btnOBCSInstall.Location = new System.Drawing.Point(364, 3);
            this.btnOBCSInstall.Name = "btnOBCSInstall";
            this.btnOBCSInstall.Size = new System.Drawing.Size(75, 25);
            this.btnOBCSInstall.TabIndex = 10;
            this.btnOBCSInstall.Text = "Install";
            this.btnOBCSInstall.UseVisualStyleBackColor = true;
            this.btnOBCSInstall.Click += new System.EventHandler(this.btnOBCSInstall_Click);
            // 
            // btnOBCSStart
            // 
            this.btnOBCSStart.AutoSize = true;
            this.btnOBCSStart.Location = new System.Drawing.Point(283, 3);
            this.btnOBCSStart.Name = "btnOBCSStart";
            this.btnOBCSStart.Size = new System.Drawing.Size(75, 25);
            this.btnOBCSStart.TabIndex = 11;
            this.btnOBCSStart.Text = "Start";
            this.btnOBCSStart.UseVisualStyleBackColor = true;
            this.btnOBCSStart.Click += new System.EventHandler(this.btnOBCSStart_Click);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.tblDrivers.SetColumnSpan(this.label1, 5);
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(56, 127);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(329, 15);
            this.label1.TabIndex = 13;
            this.label1.Text = "Hover over an item for a brief description of what it does.";
            // 
            // grpStatus
            // 
            this.grpStatus.AutoSize = true;
            this.grpStatus.Controls.Add(this.tblDrivers);
            this.grpStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpStatus.Location = new System.Drawing.Point(0, 24);
            this.grpStatus.Name = "grpStatus";
            this.grpStatus.Size = new System.Drawing.Size(448, 168);
            this.grpStatus.TabIndex = 12;
            this.grpStatus.TabStop = false;
            this.grpStatus.Text = "Status";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(448, 192);
            this.Controls.Add(this.grpStatus);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OpenBootCamp install manager";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tblDrivers.ResumeLayout(false);
            this.tblDrivers.PerformLayout();
            this.grpStatus.ResumeLayout(false);
            this.grpStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Label lblKAState;
        private System.Windows.Forms.TableLayoutPanel tblDrivers;
        private System.Windows.Forms.Label lblMHDState;
        private System.Windows.Forms.Label lblOBCSState;
        private System.Windows.Forms.GroupBox grpStatus;
        private System.Windows.Forms.Button btnKAInstall;
        private System.Windows.Forms.Button btnMHDInstall;
        private System.Windows.Forms.Button btnOBCSInstall;
        private System.Windows.Forms.Button btnOBCSStart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOBCOInstall;
        private System.Windows.Forms.Label lblOBCOState;
    }
}

