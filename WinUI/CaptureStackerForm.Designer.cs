namespace CaptureStacker
{
    partial class CaptureStackerForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.captureMode = new System.Windows.Forms.Label();
            this.PictureIndex = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // captureMode
            // 
            this.captureMode.BackColor = System.Drawing.Color.White;
            this.captureMode.Font = new System.Drawing.Font("MS UI Gothic", 12F);
            this.captureMode.Location = new System.Drawing.Point(30, 10);
            this.captureMode.Name = "captureMode";
            this.captureMode.Size = new System.Drawing.Size(80, 25);
            this.captureMode.TabIndex = 0;
            this.captureMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PictureIndex
            // 
            this.PictureIndex.Font = new System.Drawing.Font("MS UI Gothic", 11F);
            this.PictureIndex.Location = new System.Drawing.Point(130, 10);
            this.PictureIndex.Name = "PictureIndex";
            this.PictureIndex.Size = new System.Drawing.Size(60, 25);
            this.PictureIndex.TabIndex = 1;
            this.PictureIndex.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CaptureStackerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 46);
            this.Controls.Add(this.PictureIndex);
            this.Controls.Add(this.captureMode);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "CaptureStackerForm";
            this.Text = "CaptureStacker";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CaptureStackerForm_FormClosing);
            this.Load += new System.EventHandler(this.CaptureStackerForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label captureMode;
        private System.Windows.Forms.Label PictureIndex;
    }
}

