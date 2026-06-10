namespace s1113354_陳冠瑋
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lstWordList = new System.Windows.Forms.ListBox();
            this.txtWord = new System.Windows.Forms.Label();
            this.txtPhonogram = new System.Windows.Forms.Label();
            this.txtExplain = new System.Windows.Forms.Label();
            this.btnAutoPlay = new System.Windows.Forms.Button();
            this.timPlayer = new System.Windows.Forms.Timer(this.components);
            this.sssWord = new System.Windows.Forms.StatusStrip();
            this.tsslMessage = new System.Windows.Forms.ToolStripStatusLabel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.txtHelp = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();

            // lstWordList
            this.lstWordList.FormattingEnabled = true;
            this.lstWordList.ItemHeight = 12;
            this.lstWordList.Location = new System.Drawing.Point(12, 12);
            this.lstWordList.Name = "lstWordList";
            this.lstWordList.Size = new System.Drawing.Size(150, 400);
            this.lstWordList.TabIndex = 0;
            this.lstWordList.Click += new System.EventHandler(this.lstWordList_Click);
            this.lstWordList.DoubleClick += new System.EventHandler(this.lstWordList_DoubleClick);

            // txtWord
            this.txtWord.AutoSize = true;
            this.txtWord.Font = new System.Drawing.Font("Arial", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtWord.ForeColor = System.Drawing.Color.Blue;
            this.txtWord.Location = new System.Drawing.Point(200, 30);
            this.txtWord.Name = "txtWord";
            this.txtWord.Size = new System.Drawing.Size(0, 37);
            this.txtWord.TabIndex = 1;

            // txtPhonogram
            this.txtPhonogram.AutoSize = true;
            this.txtPhonogram.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPhonogram.ForeColor = System.Drawing.Color.Green;
            this.txtPhonogram.Location = new System.Drawing.Point(200, 80);
            this.txtPhonogram.Name = "txtPhonogram";
            this.txtPhonogram.Size = new System.Drawing.Size(0, 22);
            this.txtPhonogram.TabIndex = 2;

            // txtExplain
            this.txtExplain.AutoSize = true;
            this.txtExplain.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtExplain.Location = new System.Drawing.Point(200, 120);
            this.txtExplain.Name = "txtExplain";
            this.txtExplain.Size = new System.Drawing.Size(0, 20);
            this.txtExplain.TabIndex = 3;

            // btnAutoPlay
            this.btnAutoPlay.Location = new System.Drawing.Point(650, 250);
            this.btnAutoPlay.Name = "btnAutoPlay";
            this.btnAutoPlay.Size = new System.Drawing.Size(75, 40);
            this.btnAutoPlay.TabIndex = 4;
            this.btnAutoPlay.Text = "Play";
            this.btnAutoPlay.UseVisualStyleBackColor = true;
            this.btnAutoPlay.Click += new System.EventHandler(this.btnAutoPlay_Click);

            // timPlayer
            this.timPlayer.Interval = 2000;
            this.timPlayer.Tick += new System.EventHandler(this.timPlayer_Tick);

            // sssWord
            this.sssWord.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslMessage});
            this.sssWord.Location = new System.Drawing.Point(0, 428);
            this.sssWord.Name = "sssWord";
            this.sssWord.Size = new System.Drawing.Size(800, 22);
            this.sssWord.TabIndex = 5;
            this.sssWord.Text = "statusStrip1";

            // tsslMessage
            this.tsslMessage.Name = "tsslMessage";
            this.tsslMessage.Size = new System.Drawing.Size(55, 17);
            this.tsslMessage.Text = "Message";
            this.tsslMessage.ForeColor = System.Drawing.Color.Red;

            // picLogo
            this.picLogo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picLogo.Location = new System.Drawing.Point(623, 80);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(120, 150);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 6;
            this.picLogo.TabStop = false;

            // txtHelp
            this.txtHelp.AutoSize = true;
            this.txtHelp.ForeColor = System.Drawing.Color.Red;
            this.txtHelp.Location = new System.Drawing.Point(645, 300);
            this.txtHelp.Name = "txtHelp";
            this.txtHelp.Size = new System.Drawing.Size(89, 24);
            this.txtHelp.TabIndex = 7;
            this.txtHelp.Text = "Enter 下一個\r\nSpace 重複";

            // Form1
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.txtHelp);
            this.Controls.Add(this.picLogo);
            this.Controls.Add(this.sssWord);
            this.Controls.Add(this.btnAutoPlay);
            this.Controls.Add(this.txtExplain);
            this.Controls.Add(this.txtPhonogram);
            this.Controls.Add(this.txtWord);
            this.Controls.Add(this.lstWordList);
            this.Name = "Form1";
            this.Text = "單字卡";
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox lstWordList;
        private System.Windows.Forms.Label txtWord;
        private System.Windows.Forms.Label txtPhonogram;
        private System.Windows.Forms.Label txtExplain;
        private System.Windows.Forms.Button btnAutoPlay;
        private System.Windows.Forms.Timer timPlayer;
        private System.Windows.Forms.StatusStrip sssWord;
        private System.Windows.Forms.ToolStripStatusLabel tsslMessage;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label txtHelp;
    }
}

