﻿namespace ZSubtitle
{
    partial class frmText
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
            this.tmrAlpha = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // tmrAlpha
            // 
            this.tmrAlpha.Enabled = true;
            this.tmrAlpha.Interval = 50;
            this.tmrAlpha.Tick += new System.EventHandler(this.tmrAlpha_Tick);
            // 
            // frmText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(284, 326);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MinimizeBox = false;
            this.Name = "frmText";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "frmText";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer tmrAlpha;
    }
}