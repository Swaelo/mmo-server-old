namespace Swaelo_Server
{
    partial class PacketReader
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PacketReader));
            this.cmsNotifyIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsSend = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiSend = new System.Windows.Forms.ToolStripMenuItem();
            this.lvImages = new System.Windows.Forms.ImageList(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsslInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.lvClients = new System.Windows.Forms.ListView();
            this.chstatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chip = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmsNotifyIcon.SuspendLayout();
            this.cmsSend.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmsNotifyIcon
            // 
            this.cmsNotifyIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.cmsNotifyIcon.Name = "cmsNotifyIcon";
            this.cmsNotifyIcon.Size = new System.Drawing.Size(129, 26);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.openToolStripMenuItem.Text = "&Open App";
            // 
            // cmsSend
            // 
            this.cmsSend.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiSend});
            this.cmsSend.Name = "cmsSend";
            this.cmsSend.Size = new System.Drawing.Size(101, 26);
            // 
            // tsmiSend
            // 
            this.tsmiSend.Name = "tsmiSend";
            this.tsmiSend.Size = new System.Drawing.Size(100, 22);
            this.tsmiSend.Text = "Send";
            this.tsmiSend.ToolTipText = "Send to Client";
            // 
            // lvImages
            // 
            this.lvImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("lvImages.ImageStream")));
            this.lvImages.TransparentColor = System.Drawing.Color.Transparent;
            this.lvImages.Images.SetKeyName(0, "user-available.ico");
            this.lvImages.Images.SetKeyName(1, "user-invisible.ico");
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslInfo});
            this.statusStrip1.Location = new System.Drawing.Point(0, 206);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(405, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsslInfo
            // 
            this.tsslInfo.AutoSize = false;
            this.tsslInfo.Name = "tsslInfo";
            this.tsslInfo.Size = new System.Drawing.Size(100, 17);
            this.tsslInfo.Text = "Ready";
            this.tsslInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // timer1
            // 
            this.timer1.Interval = 60000;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.cmsNotifyIcon;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // lvClients
            // 
            this.lvClients.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvClients.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chstatus,
            this.chip,
            this.chPort});
            this.lvClients.ContextMenuStrip = this.cmsSend;
            this.lvClients.Location = new System.Drawing.Point(2, 6);
            this.lvClients.Name = "lvClients";
            this.lvClients.Size = new System.Drawing.Size(400, 197);
            this.lvClients.SmallImageList = this.lvImages;
            this.lvClients.TabIndex = 3;
            this.lvClients.UseCompatibleStateImageBehavior = false;
            this.lvClients.View = System.Windows.Forms.View.Details;
            // 
            // chstatus
            // 
            this.chstatus.Text = "Request";
            this.chstatus.Width = 83;
            // 
            // chip
            // 
            this.chip.Text = "Operaction";
            this.chip.Width = 140;
            // 
            // chPort
            // 
            this.chPort.Text = "ClientID";
            this.chPort.Width = 63;
            // 
            // PacketReader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(405, 228);
            this.Controls.Add(this.lvClients);
            this.Controls.Add(this.statusStrip1);
            this.Name = "PacketReader";
            this.Text = "PacketReader";
            this.Load += new System.EventHandler(this.PacketReader_Load);
            this.cmsNotifyIcon.ResumeLayout(false);
            this.cmsSend.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip cmsNotifyIcon;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmsSend;
        private System.Windows.Forms.ToolStripMenuItem tsmiSend;
        internal System.Windows.Forms.ImageList lvImages;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsslInfo;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ListView lvClients;
        private System.Windows.Forms.ColumnHeader chstatus;
        private System.Windows.Forms.ColumnHeader chip;
        private System.Windows.Forms.ColumnHeader chPort;
    }
}