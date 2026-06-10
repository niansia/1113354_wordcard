using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace s1113354_陳冠瑋
{
    public partial class Form1 : Form
    {
        private WordCollection _words = new WordCollection();
        private AudioEngine _audio = new AudioEngine();
        private Random _random = new Random();
        private Timer _autoTimer = new Timer();

        private string _wordFile = "WordCards.txt";
        private string _progressFile = "WordCards_Progress.txt";
        private bool _loaded = false;
        private bool _autoPlaying = false;
        private bool _darkMode = false;
        private bool _hideExplain = false;
        private WordItem _currentWord = null;
        private WordItem _quizAnswer = null;
        private int _dailyGoal = 30;

        private Panel pnlHeader;
        private Panel pnlSide;
        private Panel pnlMain;
        private Panel pnlFooter;
        private TabControl tabMain;

        private TextBox txtSearch;
        private ComboBox cboFilter;
        private ComboBox cboSort;
        private ComboBox cboReviewMode;
        private ListBox lstWords;

        private Label lblStatus;
        private Label lblCount;
        private Label lblWord;
        private Label lblPhonogram;
        private Label lblPosition;
        private Label lblWordStats;
        private RichTextBox rtbMeaning;
        private RichTextBox rtbSource;
        private RichTextBox rtbNote;

        private Button btnPlay;
        private Button btnAuto;
        private Button btnPrev;
        private Button btnNext;
        private Button btnRandom;
        private Button btnFavorite;
        private Button btnKnown;
        private Button btnHideExplain;
        private Button btnDarkMode;

        private NumericUpDown nudInterval;
        private TextBox txtSpell;
        private Label lblSpellResult;
        private RichTextBox rtbPracticeMeaning;
        private Label lblGoal;
        private ProgressBar prgGoal;

        private Label lblQuizQuestion;
        private Button[] btnChoices = new Button[4];
        private Label lblQuizResult;

        private Label lblTotalWords;
        private Label lblKnownWords;
        private Label lblFavoriteWords;
        private Label lblAccuracy;
        private Label lblMissingSound;
        private ListView lvStats;
        private ListView lvInfo;

        public Form1()
        {
            InitializeComponent();
            BuildInterface();
            this.Load += frmWordCards_Load;
            this.FormClosing += Form1_FormClosing;
            this.KeyPreview = true;
            this.KeyPress += frmWordCards_KeyPress;
            this.KeyDown += Form1_KeyDown;
            _autoTimer.Tick += timPlayer_Tick;
        }

        private void BuildInterface()
        {
            SuspendLayout();
            Controls.Clear();

            Text = "單字卡";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1180, 760);
            Size = new Size(1280, 820);
            Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Regular);
            BackColor = Color.FromArgb(242, 246, 252);

            pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 92;
            pnlHeader.Paint += pnlHeader_Paint;

            Label icon = new Label();
            icon.Text = "🧠";
            icon.Font = new Font("Segoe UI Emoji", 28F, FontStyle.Regular);
            icon.ForeColor = Color.White;
            icon.TextAlign = ContentAlignment.MiddleCenter;
            icon.Location = new Point(22, 17);
            icon.Size = new Size(62, 62);

            Label title = new Label();
            title.Text = "單字卡";
            title.Font = new Font("Microsoft JhengHei UI", 24F, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.AutoSize = true;
            title.Location = new Point(94, 25);

            Label subtitle = new Label();
            subtitle.Text = "";
            subtitle.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Regular);
            subtitle.ForeColor = Color.FromArgb(235, 240, 255);
            subtitle.AutoSize = true;
            subtitle.Location = new Point(98, 56);

            btnDarkMode = MakeTopButton("深色模式");
            btnDarkMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDarkMode.Location = new Point(Width - 166, 28);
            btnDarkMode.Click += delegate { ToggleDarkMode(); };
            pnlHeader.Resize += delegate { btnDarkMode.Location = new Point(pnlHeader.ClientSize.Width - 146, 28); };

            pnlHeader.Controls.Add(icon);
            pnlHeader.Controls.Add(title);
            pnlHeader.Controls.Add(subtitle);
            pnlHeader.Controls.Add(btnDarkMode);

            pnlFooter = new Panel();
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Height = 34;
            pnlFooter.Padding = new Padding(12, 5, 12, 4);
            pnlFooter.BackColor = Color.White;

            lblStatus = new Label();
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Text = "準備就緒";
            lblStatus.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(224, 68, 68);

            lblCount = new Label();
            lblCount.Dock = DockStyle.Right;
            lblCount.Width = 320;
            lblCount.TextAlign = ContentAlignment.MiddleRight;
            lblCount.ForeColor = Color.FromArgb(98, 108, 130);

            pnlFooter.Controls.Add(lblStatus);
            pnlFooter.Controls.Add(lblCount);

            pnlSide = new Panel();
            pnlSide.Dock = DockStyle.Left;
            pnlSide.Width = 320;
            pnlSide.Padding = new Padding(18);
            pnlSide.BackColor = Color.FromArgb(250, 252, 255);

            Label lblSearchTitle = MakeSmallTitle("搜尋與篩選");
            lblSearchTitle.Location = new Point(20, 18);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(20, 44);
            txtSearch.Size = new Size(280, 27);
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.Font = new Font("Microsoft JhengHei UI", 11F);
            txtSearch.TextChanged += delegate { RefreshWordList(true); };

            cboFilter = new ComboBox();
            cboFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFilter.Location = new Point(20, 84);
            cboFilter.Size = new Size(132, 27);
            cboFilter.Items.AddRange(new object[] { "全部", "收藏", "熟悉", "未熟悉", "有音檔", "缺音檔", "答錯過", "今日未複習" });
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += delegate { RefreshWordList(true); };

            cboSort = new ComboBox();
            cboSort.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSort.Location = new Point(168, 84);
            cboSort.Size = new Size(132, 27);
            cboSort.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cboSort.Items.AddRange(new object[] { "原始順序", "A 到 Z", "Z 到 A", "錯誤較多", "正確率較低", "最近複習", "隨機" });
            cboSort.SelectedIndex = 0;
            cboSort.SelectedIndexChanged += delegate { RefreshWordList(true); };

            cboReviewMode = new ComboBox();
            cboReviewMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboReviewMode.Location = new Point(20, 124);
            cboReviewMode.Size = new Size(280, 27);
            cboReviewMode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboReviewMode.Items.AddRange(new object[] { "一般複習", "智慧複習", "間隔複習", "錯題優先", "收藏複習", "未熟悉複習", "隨機複習" });
            cboReviewMode.SelectedIndex = 0;

            lstWords = new ListBox();
            lstWords.Location = new Point(20, 168);
            lstWords.Size = new Size(280, 520);
            lstWords.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstWords.BorderStyle = BorderStyle.None;
            lstWords.DrawMode = DrawMode.OwnerDrawFixed;
            lstWords.ItemHeight = 34;
            lstWords.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            lstWords.SelectedIndexChanged += delegate { ShowSelectedWord(false); };
            lstWords.Click += lstWordList_Click;
            lstWords.DoubleClick += lstWordList_DoubleClick;
            lstWords.DrawItem += lstWords_DrawItem;

            pnlSide.Controls.Add(lblSearchTitle);
            pnlSide.Controls.Add(txtSearch);
            pnlSide.Controls.Add(cboFilter);
            pnlSide.Controls.Add(cboSort);
            pnlSide.Controls.Add(cboReviewMode);
            pnlSide.Controls.Add(lstWords);

            pnlMain = new Panel();
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Padding = new Padding(18);
            pnlMain.BackColor = Color.FromArgb(242, 246, 252);

            tabMain = new TabControl();
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Bold);
            tabMain.ItemSize = new Size(110, 34);
            tabMain.SizeMode = TabSizeMode.Fixed;

            tabMain.TabPages.Add(BuildCardPage());
            tabMain.TabPages.Add(BuildPracticePage());
            tabMain.TabPages.Add(BuildQuizPage());
            tabMain.TabPages.Add(BuildStatsPage());
            tabMain.TabPages.Add(BuildManagePage());

            pnlMain.Controls.Add(tabMain);

            Controls.Add(pnlMain);
            Controls.Add(pnlSide);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);

            ApplyTheme();
            ResumeLayout(false);
        }

        private TabPage BuildCardPage()
        {
            TabPage page = new TabPage("單字卡");
            page.Padding = new Padding(10);
            page.BackColor = Color.FromArgb(242, 246, 252);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.RowCount = 4;
            root.ColumnCount = 1;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));

            FlowLayoutPanel toolbar = new FlowLayoutPanel();
            toolbar.Dock = DockStyle.Fill;
            toolbar.WrapContents = false;
            toolbar.AutoScroll = true;
            toolbar.Padding = new Padding(0, 4, 0, 4);

            btnPlay = MakeActionButton("播放", 74);
            btnAuto = MakeActionButton("自動播放", 98);
            nudInterval = new NumericUpDown();
            nudInterval.Minimum = 1;
            nudInterval.Maximum = 60;
            nudInterval.Value = 3;
            nudInterval.Width = 54;
            nudInterval.Margin = new Padding(4, 10, 0, 0);

            Label sec = new Label();
            sec.Text = "秒";
            sec.AutoSize = true;
            sec.Padding = new Padding(0, 13, 8, 0);

            btnPrev = MakeActionButton("上一個", 80);
            btnNext = MakeActionButton("下一個", 80);
            btnRandom = MakeActionButton("隨機", 68);
            btnFavorite = MakeActionButton("☆ 收藏", 88);
            btnKnown = MakeActionButton("未熟悉", 84);
            btnHideExplain = MakeActionButton("隱藏解釋", 98);

            btnPlay.Click += delegate { PlaySelectedWord(); };
            btnAuto.Click += btnAutoPlay_Click;
            btnPrev.Click += delegate { MovePrevious(true); };
            btnNext.Click += delegate { MoveNext(true); };
            btnRandom.Click += delegate { SelectRandomWord(true); };
            btnFavorite.Click += delegate { ToggleFavorite(); };
            btnKnown.Click += delegate { ToggleKnown(); };
            btnHideExplain.Click += delegate { ToggleExplain(); };

            toolbar.Controls.Add(btnPlay);
            toolbar.Controls.Add(btnAuto);
            toolbar.Controls.Add(nudInterval);
            toolbar.Controls.Add(sec);
            toolbar.Controls.Add(btnPrev);
            toolbar.Controls.Add(btnNext);
            toolbar.Controls.Add(btnRandom);
            toolbar.Controls.Add(btnFavorite);
            toolbar.Controls.Add(btnKnown);
            toolbar.Controls.Add(btnHideExplain);

            Panel wordCard = MakeCardPanel();
            wordCard.Padding = new Padding(24);

            TableLayoutPanel wordLayout = new TableLayoutPanel();
            wordLayout.Dock = DockStyle.Fill;
            wordLayout.RowCount = 3;
            wordLayout.ColumnCount = 1;
            wordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            wordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
            wordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            TableLayoutPanel infoRow = new TableLayoutPanel();
            infoRow.Dock = DockStyle.Fill;
            infoRow.ColumnCount = 2;
            infoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            infoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            lblPosition = new Label();
            lblPosition.Dock = DockStyle.Fill;
            lblPosition.TextAlign = ContentAlignment.MiddleLeft;
            lblPosition.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);

            lblWordStats = new Label();
            lblWordStats.Dock = DockStyle.Fill;
            lblWordStats.TextAlign = ContentAlignment.MiddleRight;
            lblWordStats.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);

            infoRow.Controls.Add(lblPosition, 0, 0);
            infoRow.Controls.Add(lblWordStats, 1, 0);

            lblWord = new Label();
            lblWord.Dock = DockStyle.Fill;
            lblWord.TextAlign = ContentAlignment.MiddleLeft;
            lblWord.Font = new Font("Segoe UI", 38F, FontStyle.Bold);
            lblWord.AutoEllipsis = true;

            lblPhonogram = new Label();
            lblPhonogram.Dock = DockStyle.Fill;
            lblPhonogram.TextAlign = ContentAlignment.MiddleLeft;
            lblPhonogram.Font = new Font("Georgia", 18F, FontStyle.Bold);
            lblPhonogram.AutoEllipsis = true;

            wordLayout.Controls.Add(infoRow, 0, 0);
            wordLayout.Controls.Add(lblWord, 0, 1);
            wordLayout.Controls.Add(lblPhonogram, 0, 2);

            wordCard.Controls.Add(wordLayout);

            TableLayoutPanel detailLayout = new TableLayoutPanel();
            detailLayout.Dock = DockStyle.Fill;
            detailLayout.RowCount = 1;
            detailLayout.ColumnCount = 2;
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));

            GroupBox grpMeaning = MakeGroupBox("解釋");
            rtbMeaning = new RichTextBox();
            rtbMeaning.Dock = DockStyle.Fill;
            rtbMeaning.ReadOnly = true;
            rtbMeaning.BorderStyle = BorderStyle.None;
            rtbMeaning.Font = new Font("Microsoft JhengHei UI", 14F, FontStyle.Regular);
            grpMeaning.Controls.Add(rtbMeaning);

            GroupBox grpSource = MakeGroupBox("字源與補充");
            rtbSource = new RichTextBox();
            rtbSource.Dock = DockStyle.Fill;
            rtbSource.ReadOnly = true;
            rtbSource.BorderStyle = BorderStyle.None;
            rtbSource.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Regular);
            grpSource.Controls.Add(rtbSource);

            detailLayout.Controls.Add(grpMeaning, 0, 0);
            detailLayout.Controls.Add(grpSource, 1, 0);

            GroupBox grpNote = MakeGroupBox("個人筆記");
            rtbNote = new RichTextBox();
            rtbNote.Dock = DockStyle.Fill;
            rtbNote.BorderStyle = BorderStyle.None;
            rtbNote.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Regular);
            grpNote.Controls.Add(rtbNote);

            root.Controls.Add(toolbar, 0, 0);
            root.Controls.Add(wordCard, 0, 1);
            root.Controls.Add(detailLayout, 0, 2);
            root.Controls.Add(grpNote, 0, 3);

            page.Controls.Add(root);
            return page;
        }

        private TabPage BuildPracticePage()
        {
            TabPage page = new TabPage("練習");
            page.Padding = new Padding(10);
            page.BackColor = Color.FromArgb(242, 246, 252);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 2;
            root.RowCount = 2;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 38));

            GroupBox grpSpell = MakeGroupBox("拼字練習");
            TableLayoutPanel spellLayout = new TableLayoutPanel();
            spellLayout.Dock = DockStyle.Fill;
            spellLayout.RowCount = 5;
            spellLayout.ColumnCount = 1;
            spellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            spellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            spellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            spellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            spellLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Label lblSpellHint = new Label();
            lblSpellHint.Dock = DockStyle.Fill;
            lblSpellHint.Text = "輸入目前單字後按 Enter 檢查。";
            lblSpellHint.TextAlign = ContentAlignment.MiddleLeft;

            txtSpell = new TextBox();
            txtSpell.Dock = DockStyle.Fill;
            txtSpell.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            txtSpell.KeyDown += delegate (object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    CheckSpelling();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            FlowLayoutPanel spellButtons = new FlowLayoutPanel();
            spellButtons.Dock = DockStyle.Fill;
            spellButtons.WrapContents = false;

            Button btnCheckSpell = MakeActionButton("檢查", 76);
            Button btnReveal = MakeActionButton("顯示答案", 100);
            Button btnSpellPlay = MakeActionButton("播放", 76);
            btnCheckSpell.Click += delegate { CheckSpelling(); };
            btnReveal.Click += delegate { RevealSpellingAnswer(); };
            btnSpellPlay.Click += delegate { PlaySelectedWord(); };

            spellButtons.Controls.Add(btnCheckSpell);
            spellButtons.Controls.Add(btnReveal);
            spellButtons.Controls.Add(btnSpellPlay);

            lblSpellResult = new Label();
            lblSpellResult.Dock = DockStyle.Fill;
            lblSpellResult.Font = new Font("Microsoft JhengHei UI", 13F, FontStyle.Bold);
            lblSpellResult.TextAlign = ContentAlignment.MiddleLeft;

            spellLayout.Controls.Add(lblSpellHint, 0, 0);
            spellLayout.Controls.Add(txtSpell, 0, 1);
            spellLayout.Controls.Add(spellButtons, 0, 2);
            spellLayout.Controls.Add(lblSpellResult, 0, 3);
            grpSpell.Controls.Add(spellLayout);

            GroupBox grpGuess = MakeGroupBox("看解釋猜單字");
            rtbPracticeMeaning = new RichTextBox();
            rtbPracticeMeaning.Dock = DockStyle.Fill;
            rtbPracticeMeaning.ReadOnly = true;
            rtbPracticeMeaning.BorderStyle = BorderStyle.None;
            rtbPracticeMeaning.Font = new Font("Microsoft JhengHei UI", 14F);
            grpGuess.Controls.Add(rtbPracticeMeaning);

            GroupBox grpGoal = MakeGroupBox("今日進度");
            TableLayoutPanel goalLayout = new TableLayoutPanel();
            goalLayout.Dock = DockStyle.Fill;
            goalLayout.RowCount = 3;
            goalLayout.ColumnCount = 1;
            goalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            goalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            goalLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            lblGoal = new Label();
            lblGoal.Dock = DockStyle.Fill;
            lblGoal.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);
            lblGoal.TextAlign = ContentAlignment.MiddleLeft;

            prgGoal = new ProgressBar();
            prgGoal.Dock = DockStyle.Fill;
            prgGoal.Maximum = _dailyGoal;

            FlowLayoutPanel goalButtons = new FlowLayoutPanel();
            goalButtons.Dock = DockStyle.Fill;
            Button btnGoalPlus = MakeActionButton("目標 +10", 96);
            Button btnGoalMinus = MakeActionButton("目標 -10", 96);
            btnGoalPlus.Click += delegate { _dailyGoal += 10; if (_dailyGoal > 200) _dailyGoal = 200; UpdateStats(); };
            btnGoalMinus.Click += delegate { _dailyGoal -= 10; if (_dailyGoal < 10) _dailyGoal = 10; UpdateStats(); };
            goalButtons.Controls.Add(btnGoalPlus);
            goalButtons.Controls.Add(btnGoalMinus);

            goalLayout.Controls.Add(lblGoal, 0, 0);
            goalLayout.Controls.Add(prgGoal, 0, 1);
            goalLayout.Controls.Add(goalButtons, 0, 2);
            grpGoal.Controls.Add(goalLayout);

            GroupBox grpKeys = MakeGroupBox("快捷鍵");
            Label lblKeys = new Label();
            lblKeys.Dock = DockStyle.Fill;
            lblKeys.Text = "Enter：下一個並播放\r\nSpace：重播\r\n← / →：上一個 / 下一個\r\nF：收藏　K：熟悉　H：隱藏解釋\r\nCtrl + S：儲存";
            lblKeys.Font = new Font("Microsoft JhengHei UI", 12F);
            lblKeys.TextAlign = ContentAlignment.MiddleLeft;
            grpKeys.Controls.Add(lblKeys);

            root.Controls.Add(grpSpell, 0, 0);
            root.Controls.Add(grpGuess, 1, 0);
            root.Controls.Add(grpGoal, 0, 1);
            root.Controls.Add(grpKeys, 1, 1);

            page.Controls.Add(root);
            return page;
        }

        private TabPage BuildQuizPage()
        {
            TabPage page = new TabPage("測驗");
            page.Padding = new Padding(10);
            page.BackColor = Color.FromArgb(242, 246, 252);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.RowCount = 4;
            root.ColumnCount = 1;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));

            lblQuizQuestion = new Label();
            lblQuizQuestion.Dock = DockStyle.Fill;
            lblQuizQuestion.Font = new Font("Microsoft JhengHei UI", 17F, FontStyle.Bold);
            lblQuizQuestion.TextAlign = ContentAlignment.MiddleLeft;
            lblQuizQuestion.AutoEllipsis = true;

            TableLayoutPanel choiceGrid = new TableLayoutPanel();
            choiceGrid.Dock = DockStyle.Fill;
            choiceGrid.ColumnCount = 2;
            choiceGrid.RowCount = 2;
            choiceGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            choiceGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            choiceGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            choiceGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            for (int i = 0; i < btnChoices.Length; i++)
            {
                Button btn = MakeChoiceButton("");
                btn.Dock = DockStyle.Fill;
                btn.Margin = new Padding(10);
                int index = i;
                btn.Click += delegate { CheckChoice(index); };
                btnChoices[i] = btn;
                choiceGrid.Controls.Add(btn, i % 2, i / 2);
            }

            FlowLayoutPanel quizButtons = new FlowLayoutPanel();
            quizButtons.Dock = DockStyle.Fill;
            quizButtons.WrapContents = false;

            Button btnNewQuiz = MakeActionButton("重新出題", 102);
            Button btnNextQuiz = MakeActionButton("下一題", 82);
            Button btnQuizPlay = MakeActionButton("播放", 76);
            btnNewQuiz.Click += delegate { MakeQuiz(); };
            btnNextQuiz.Click += delegate { MoveNext(false); MakeQuiz(); };
            btnQuizPlay.Click += delegate { PlaySelectedWord(); };

            quizButtons.Controls.Add(btnNewQuiz);
            quizButtons.Controls.Add(btnNextQuiz);
            quizButtons.Controls.Add(btnQuizPlay);

            lblQuizResult = new Label();
            lblQuizResult.Dock = DockStyle.Fill;
            lblQuizResult.Font = new Font("Microsoft JhengHei UI", 13F, FontStyle.Bold);
            lblQuizResult.TextAlign = ContentAlignment.MiddleLeft;

            GroupBox grpQuiz = MakeGroupBox("");
            grpQuiz.Dock = DockStyle.Fill;
            grpQuiz.Controls.Add(root);

            root.Controls.Add(lblQuizQuestion, 0, 0);
            root.Controls.Add(choiceGrid, 0, 1);
            root.Controls.Add(quizButtons, 0, 2);
            root.Controls.Add(lblQuizResult, 0, 3);

            page.Controls.Add(grpQuiz);
            return page;
        }

        private TabPage BuildStatsPage()
        {
            TabPage page = new TabPage("統計");
            page.Padding = new Padding(10);
            page.BackColor = Color.FromArgb(242, 246, 252);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.RowCount = 2;
            root.ColumnCount = 1;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            TableLayoutPanel cards = new TableLayoutPanel();
            cards.Dock = DockStyle.Fill;
            cards.ColumnCount = 5;
            cards.RowCount = 1;
            for (int i = 0; i < 5; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            lblTotalWords = MakeStatCard(cards, 0, "總單字");
            lblKnownWords = MakeStatCard(cards, 1, "已熟悉");
            lblFavoriteWords = MakeStatCard(cards, 2, "收藏");
            lblAccuracy = MakeStatCard(cards, 3, "答題正確率");
            lblMissingSound = MakeStatCard(cards, 4, "缺音檔");

            GroupBox grpList = MakeGroupBox("練習統計");
            lvStats = new ListView();
            lvStats.Dock = DockStyle.Fill;
            lvStats.View = View.Details;
            lvStats.FullRowSelect = true;
            lvStats.GridLines = false;
            lvStats.BorderStyle = BorderStyle.None;
            lvStats.Font = new Font("Microsoft JhengHei UI", 10F);
            lvStats.Columns.Add("單字", 160);
            lvStats.Columns.Add("狀態", 130);
            lvStats.Columns.Add("正確", 80);
            lvStats.Columns.Add("錯誤", 80);
            lvStats.Columns.Add("正確率", 90);
            lvStats.Columns.Add("複習建議", 110);
            lvStats.Columns.Add("最後複習", 180);
            lvStats.DoubleClick += delegate
            {
                if (lvStats.SelectedItems.Count == 0) return;
                WordItem w = lvStats.SelectedItems[0].Tag as WordItem;
                if (w != null) SelectWord(w, false);
            };
            grpList.Controls.Add(lvStats);

            root.Controls.Add(cards, 0, 0);
            root.Controls.Add(grpList, 0, 1);

            page.Controls.Add(root);
            return page;
        }

        private TabPage BuildManagePage()
        {
            TabPage page = new TabPage("管理");
            page.Padding = new Padding(10);
            page.BackColor = Color.FromArgb(242, 246, 252);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 2;
            root.RowCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            GroupBox grpTools = MakeGroupBox("工具");
            FlowLayoutPanel tools = new FlowLayoutPanel();
            tools.Dock = DockStyle.Fill;
            tools.AutoScroll = true;
            tools.WrapContents = true;

            Button btnAdd = MakeActionButton("新增單字", 132);
            Button btnEdit = MakeActionButton("編輯單字", 132);
            Button btnDelete = MakeActionButton("刪除單字", 132);
            Button btnSave = MakeActionButton("儲存全部", 132);
            Button btnImport = MakeActionButton("匯入單字", 132);
            Button btnExport = MakeActionButton("匯出目前清單", 132);
            Button btnExportWrong = MakeActionButton("匯出錯題", 132);
            Button btnExportFavorite = MakeActionButton("匯出收藏", 132);
            Button btnBackup = MakeActionButton("備份", 132);
            Button btnCheckSound = MakeActionButton("檢查音檔", 132);
            Button btnOpenFolder = MakeActionButton("開啟資料夾", 132);
            Button btnResetCurrent = MakeActionButton("重置目前進度", 132);
            Button btnResetAll = MakeActionButton("重置全部進度", 132);
            Button btnFillCurrent = MakeActionButton("補齊目前解釋", 132);
            Button btnFillAll = MakeActionButton("補齊全部解釋", 132);
            Button btnCleanMeanings = MakeActionButton("清理全部解釋", 132);
            Button btnSmartSort = MakeActionButton("智慧複習排序", 132);
            Button btnExportReport = MakeActionButton("匯出學習報告", 132);
            Button btnExportAnki = MakeActionButton("匯出Anki卡片", 132);
            Button btnDuplicates = MakeActionButton("檢查重複", 132);

            btnAdd.Click += delegate { AddWord(); };
            btnEdit.Click += delegate { EditWord(); };
            btnDelete.Click += delegate { DeleteWord(); };
            btnSave.Click += delegate { SaveAll(); };
            btnImport.Click += delegate { ImportWords(); };
            btnExport.Click += delegate { ExportWords(lstWords.Items.Cast<WordItem>(), "WordCards_Export.txt"); };
            btnExportWrong.Click += delegate { ExportWords(_words.Where(w => w.WrongCount > 0), "WordCards_Wrong.txt"); };
            btnExportFavorite.Click += delegate { ExportWords(_words.Where(w => w.IsFavorite), "WordCards_Favorite.txt"); };
            btnBackup.Click += delegate { BackupWords(); };
            btnCheckSound.Click += delegate { CheckSoundFiles(); };
            btnOpenFolder.Click += delegate { OpenCurrentFolder(); };
            btnResetCurrent.Click += delegate { ResetCurrentProgress(); };
            btnResetAll.Click += delegate { ResetAllProgress(); };
            btnFillCurrent.Click += delegate { FillCurrentMeaning(); };
            btnFillAll.Click += delegate { FillAllMissingMeanings(); };
            btnCleanMeanings.Click += delegate { CleanAllMeanings(); };
            btnSmartSort.Click += delegate { ApplySmartReviewSort(); };
            btnExportReport.Click += delegate { ExportStudyReport(); };
            btnExportAnki.Click += delegate { ExportAnkiCards(); };
            btnDuplicates.Click += delegate { CheckDuplicates(); };

            tools.Controls.Add(btnAdd);
            tools.Controls.Add(btnEdit);
            tools.Controls.Add(btnDelete);
            tools.Controls.Add(btnSave);
            tools.Controls.Add(btnImport);
            tools.Controls.Add(btnExport);
            tools.Controls.Add(btnExportWrong);
            tools.Controls.Add(btnExportFavorite);
            tools.Controls.Add(btnBackup);
            tools.Controls.Add(btnCheckSound);
            tools.Controls.Add(btnOpenFolder);
            tools.Controls.Add(btnResetCurrent);
            tools.Controls.Add(btnResetAll);
            tools.Controls.Add(btnFillCurrent);
            tools.Controls.Add(btnFillAll);
            tools.Controls.Add(btnCleanMeanings);
            tools.Controls.Add(btnSmartSort);
            tools.Controls.Add(btnExportReport);
            tools.Controls.Add(btnExportAnki);
            tools.Controls.Add(btnDuplicates);

            grpTools.Controls.Add(tools);

            GroupBox grpInfo = MakeGroupBox("資訊");
            lvInfo = new ListView();
            lvInfo.Dock = DockStyle.Fill;
            lvInfo.View = View.Details;
            lvInfo.FullRowSelect = true;
            lvInfo.GridLines = false;
            lvInfo.BorderStyle = BorderStyle.None;
            lvInfo.Font = new Font("Microsoft JhengHei UI", 10F);
            lvInfo.Columns.Add("項目", 150);
            lvInfo.Columns.Add("內容", 650);
            grpInfo.Controls.Add(lvInfo);

            root.Controls.Add(grpTools, 0, 0);
            root.Controls.Add(grpInfo, 1, 0);

            page.Controls.Add(root);
            return page;
        }

        private Button MakeTopButton(string text)
        {
            Button b = new Button();
            b.Text = text;
            b.Size = new Size(118, 38);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Color.White;
            b.ForeColor = Color.FromArgb(73, 82, 255);
            b.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            return b;
        }

        private Button MakeActionButton(string text, int width)
        {
            Button b = new Button();
            b.Text = text;
            b.Size = new Size(width, 38);
            b.Margin = new Padding(4, 6, 4, 6);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Color.FromArgb(232, 237, 255);
            b.ForeColor = Color.FromArgb(73, 82, 255);
            b.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            b.AutoEllipsis = true;
            return b;
        }

        private Button MakeChoiceButton(string text)
        {
            Button b = MakeActionButton(text, 200);
            b.Height = 86;
            b.TextAlign = ContentAlignment.MiddleLeft;
            b.Padding = new Padding(16, 0, 16, 0);
            b.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);
            b.ForeColor = Color.FromArgb(34, 44, 65);
            b.BackColor = Color.FromArgb(248, 250, 255);
            return b;
        }

        private Label MakeSmallTitle(string text)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(72, 82, 105);
            lbl.AutoSize = true;
            return lbl;
        }

        private GroupBox MakeGroupBox(string text)
        {
            GroupBox gb = new GroupBox();
            gb.Text = text;
            gb.Dock = DockStyle.Fill;
            gb.Padding = new Padding(14, 24, 14, 14);
            gb.Margin = new Padding(6);
            gb.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Bold);
            gb.BackColor = Color.White;
            gb.ForeColor = Color.FromArgb(72, 82, 105);
            return gb;
        }

        private Panel MakeCardPanel()
        {
            Panel p = new Panel();
            p.Dock = DockStyle.Fill;
            p.Margin = new Padding(6);
            p.BackColor = Color.White;
            p.Paint += delegate (object sender, PaintEventArgs e)
            {
                Rectangle r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                using (Pen pen = new Pen(_darkMode ? Color.FromArgb(55, 68, 94) : Color.FromArgb(224, 232, 246)))
                {
                    e.Graphics.DrawRectangle(pen, r);
                }
            };
            return p;
        }

        private Label MakeStatCard(TableLayoutPanel parent, int column, string title)
        {
            GroupBox gb = MakeGroupBox(title);
            gb.Margin = new Padding(6);
            Label value = new Label();
            value.Dock = DockStyle.Fill;
            value.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            value.TextAlign = ContentAlignment.MiddleCenter;
            value.ForeColor = Color.FromArgb(73, 82, 255);
            gb.Controls.Add(value);
            parent.Controls.Add(gb, column, 0);
            return value;
        }

        private void pnlHeader_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(pnlHeader.ClientRectangle, Color.FromArgb(105, 82, 255), Color.FromArgb(42, 146, 255), LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, pnlHeader.ClientRectangle);
            }
        }

        private void frmWordCards_Load(object sender, EventArgs e)
        {
            if (_loaded) return;
            _loaded = true;
            LoadIcon();
            LoadWordFile();
        }

        private void LoadIcon()
        {
            string iconFile = FindExistingFile(new string[]
            {
                "WordCards_Logo.png",
                Path.Combine(Application.StartupPath, "WordCards_Logo.png"),
                Path.Combine(GetProjectRoot(), "WordCards_Logo.png")
            });

            if (string.IsNullOrEmpty(iconFile)) return;

            try
            {
                using (Bitmap bmp = new Bitmap(iconFile))
                {
                    this.Icon = Icon.FromHandle(bmp.GetHicon());
                }
            }
            catch { }
        }

        private void LoadWordFile()
        {
            string found = FindExistingFile(new string[]
            {
                _wordFile,
                Path.Combine(Application.StartupPath, _wordFile),
                Path.Combine(GetProjectRoot(), _wordFile),
                Path.Combine(Environment.CurrentDirectory, _wordFile)
            });

            if (!string.IsNullOrEmpty(found)) _wordFile = found;

            if (!File.Exists(_wordFile))
            {
                MessageBox.Show("找不到單字檔：" + Environment.NewLine + _wordFile, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("找不到單字檔");
                return;
            }

            try
            {
                _words.Clear();
                _words.LoadFromStringArray(File.ReadAllLines(_wordFile, Encoding.UTF8));

                string dir = Path.GetDirectoryName(Path.GetFullPath(_wordFile));
                if (string.IsNullOrEmpty(dir)) dir = Application.StartupPath;
                _progressFile = Path.Combine(dir, "WordCards_Progress.txt");

                LoadProgress();
                RefreshWordList(false);
                if (lstWords.Items.Count > 0) lstWords.SelectedIndex = 0;
                UpdateStats();
                RefreshInfo();
                SetStatus("已載入：" + _wordFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("載入失敗：" + Environment.NewLine + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("載入失敗");
            }
        }

        private void LoadProgress()
        {
            if (!File.Exists(_progressFile)) return;

            try
            {
                Dictionary<string, string[]> map = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                foreach (string line in File.ReadAllLines(_progressFile, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] p = line.Split('\t');
                    if (p.Length >= 7 && !map.ContainsKey(p[0])) map.Add(p[0], p);
                }

                foreach (WordItem w in _words)
                {
                    if (!map.ContainsKey(w.Word)) continue;
                    string[] p = map[w.Word];
                    int n;
                    long ticks;
                    w.IsFavorite = p[1] == "1";
                    w.IsKnown = p[2] == "1";
                    if (int.TryParse(p[3], out n)) w.CorrectCount = n;
                    if (int.TryParse(p[4], out n)) w.WrongCount = n;
                    if (long.TryParse(p[5], out ticks) && ticks > 0) w.LastReviewed = new DateTime(ticks);
                    try { w.Note = Encoding.UTF8.GetString(Convert.FromBase64String(p[6])); } catch { w.Note = ""; }
                }
            }
            catch { }
        }

        private void SaveProgress()
        {
            try
            {
                List<string> lines = new List<string>();
                foreach (WordItem w in _words)
                {
                    string note = Convert.ToBase64String(Encoding.UTF8.GetBytes(w.Note ?? ""));
                    string ticks = w.LastReviewed.HasValue ? w.LastReviewed.Value.Ticks.ToString() : "0";
                    lines.Add(string.Join("\t", new string[]
                    {
                        w.Word ?? "",
                        w.IsFavorite ? "1" : "0",
                        w.IsKnown ? "1" : "0",
                        w.CorrectCount.ToString(),
                        w.WrongCount.ToString(),
                        ticks,
                        note
                    }));
                }
                File.WriteAllLines(_progressFile, lines.ToArray(), Encoding.UTF8);
            }
            catch { }
        }

        private void SaveAll()
        {
            SaveCurrentNote();
            try
            {
                _words.SaveToFile(_wordFile);
                SaveProgress();
                RefreshWordList(true);
                UpdateStats();
                RefreshInfo();
                SetStatus("已儲存全部資料");
            }
            catch (Exception ex)
            {
                MessageBox.Show("儲存失敗：" + Environment.NewLine + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("儲存失敗");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveCurrentNote();
            SaveProgress();
            _audio.Stop();
        }

        private void RefreshWordList(bool keepSelection)
        {
            if (lstWords == null) return;

            WordItem selected = keepSelection ? GetSelectedWord() : null;
            IEnumerable<WordItem> query = _words;

            string key = txtSearch.Text.Trim();
            if (key.Length > 0)
            {
                query = query.Where(w =>
                    ContainsText(w.Word, key) ||
                    ContainsText(w.Phonogram, key) ||
                    ContainsText(w.Explain, key) ||
                    ContainsText(w.Note, key) ||
                    ContainsText(ExplanationParser.Parse(w.Word, w.Explain).Meaning, key));
            }

            string filterText = cboFilter.SelectedItem == null ? "全部" : cboFilter.SelectedItem.ToString();
            if (filterText == "收藏") query = query.Where(w => w.IsFavorite);
            if (filterText == "熟悉") query = query.Where(w => w.IsKnown);
            if (filterText == "未熟悉") query = query.Where(w => !w.IsKnown);
            if (filterText == "有音檔") query = query.Where(w => ResolveSoundPath(w) != null);
            if (filterText == "缺音檔") query = query.Where(w => ResolveSoundPath(w) == null);
            if (filterText == "答錯過") query = query.Where(w => w.WrongCount > 0);
            if (filterText == "今日未複習") query = query.Where(w => !w.LastReviewed.HasValue || w.LastReviewed.Value.Date < DateTime.Today);

            string sortText = cboSort.SelectedItem == null ? "原始順序" : cboSort.SelectedItem.ToString();
            if (sortText == "A 到 Z") query = query.OrderBy(w => w.Word);
            if (sortText == "Z 到 A") query = query.OrderByDescending(w => w.Word);
            if (sortText == "錯誤較多") query = query.OrderByDescending(w => w.WrongCount).ThenBy(w => w.Word);
            if (sortText == "正確率較低") query = query.OrderBy(w => w.TotalAnswer == 0 ? 999 : w.Accuracy).ThenByDescending(w => w.WrongCount);
            if (sortText == "最近複習") query = query.OrderByDescending(w => w.LastReviewed.HasValue ? w.LastReviewed.Value : DateTime.MinValue);
            if (sortText == "隨機") query = query.OrderBy(w => _random.Next());

            lstWords.BeginUpdate();
            lstWords.Items.Clear();
            foreach (WordItem w in query) lstWords.Items.Add(w);
            lstWords.EndUpdate();

            lblCount.Text = "顯示 " + lstWords.Items.Count + " / 總共 " + _words.Count;

            if (selected != null && lstWords.Items.Contains(selected))
            {
                lstWords.SelectedItem = selected;
            }
            else if (lstWords.Items.Count > 0 && lstWords.SelectedIndex < 0)
            {
                lstWords.SelectedIndex = 0;
            }
            else if (lstWords.Items.Count == 0)
            {
                ShowWord(null);
            }

            UpdateStats();
        }

        private void ShowSelectedWord(bool play)
        {
            SaveCurrentNote();
            WordItem word = GetSelectedWord();
            ShowWord(word);
            if (play && word != null) PlayWord(word);
        }

        private void ShowWord(WordItem word)
        {
            _currentWord = word;

            if (word == null)
            {
                lblWord.Text = "";
                lblPhonogram.Text = "";
                lblPosition.Text = "0 / 0";
                lblWordStats.Text = "";
                rtbMeaning.Text = "";
                rtbSource.Text = "";
                rtbNote.Text = "";
                rtbPracticeMeaning.Text = "";
                lblQuizQuestion.Text = "";
                lblQuizResult.Text = "";
                return;
            }

            ExplanationView view = ExplanationParser.Parse(word.Word, word.Explain);

            lblWord.Text = word.Word;
            lblPhonogram.Text = word.Phonogram;
            rtbMeaning.Text = _hideExplain ? "解釋已隱藏，按「顯示解釋」即可查看。" : view.Meaning;
            rtbSource.Text = view.Detail.Length == 0 ? "無字源或補充資料。" : view.Detail;
            rtbNote.Text = word.Note ?? "";
            rtbPracticeMeaning.Text = view.Meaning;

            btnFavorite.Text = word.IsFavorite ? "★ 收藏" : "☆ 收藏";
            btnKnown.Text = word.IsKnown ? "已熟悉" : "未熟悉";
            btnHideExplain.Text = _hideExplain ? "顯示解釋" : "隱藏解釋";

            int listIndex = lstWords.SelectedIndex >= 0 ? lstWords.SelectedIndex + 1 : 0;
            int originalIndex = _words.IndexOf(word) + 1;
            lblPosition.Text = "清單 " + listIndex + " / " + lstWords.Items.Count + "　原始 " + originalIndex + " / " + _words.Count;

            RefreshWordStats(word);
            MakeQuiz();
            RefreshInfo();
            lstWords.Invalidate();
        }

        private void RefreshWordStats(WordItem word)
        {
            if (word == null) return;
            lblWordStats.Text = "正確 " + word.CorrectCount + "　錯誤 " + word.WrongCount + "　正確率 " + word.AccuracyText + "　" + (word.IsKnown ? "已熟悉" : "未熟悉");
        }

        private WordItem GetSelectedWord()
        {
            return lstWords.SelectedItem as WordItem;
        }

        private void SaveCurrentNote()
        {
            if (_currentWord != null && rtbNote != null) _currentWord.Note = rtbNote.Text;
        }

        private void PlaySelectedWord()
        {
            WordItem word = GetSelectedWord();
            if (word == null)
            {
                SetStatus("尚未選取單字");
                return;
            }
            PlayWord(word);
        }

        private void PlayWord(WordItem word)
        {
            string path = ResolveSoundPath(word);
            if (path == null)
            {
                SetStatus("找不到音檔：" + ExpectedSoundText(word));
                return;
            }

            string error;
            if (_audio.Play(path, out error))
            {
                word.LastReviewed = DateTime.Now;
                RefreshWordStats(word);
                UpdateStats();
                SetStatus("正在播放：" + path);
            }
            else
            {
                SetStatus("播放失敗：" + error + "　檔案：" + path);
            }
        }

        private string ResolveSoundPath(WordItem word)
        {
            if (word == null) return null;

            string raw = (word.SoundPath ?? "").Trim().Trim('"');
            string wordName = SafeFileName(word.Word);
            string letter = FirstLetterFolder(word.Word);
            string app = Application.StartupPath;
            string project = GetProjectRoot();
            string current = Environment.CurrentDirectory;

            List<string> bases = new List<string>();

            if (raw.Length > 0)
            {
                bases.Add(raw);
                if (!Path.IsPathRooted(raw))
                {
                    bases.Add(Path.Combine(app, raw));
                    bases.Add(Path.Combine(project, raw));
                    bases.Add(Path.Combine(current, raw));
                }
            }

            if (wordName.Length > 0)
            {
                bases.Add(Path.Combine(app, "Sound", letter, wordName));
                bases.Add(Path.Combine(project, "Sound", letter, wordName));
                bases.Add(Path.Combine(current, "Sound", letter, wordName));
                bases.Add(Path.Combine(app, "Sound", letter.ToLower(), wordName));
                bases.Add(Path.Combine(project, "Sound", letter.ToLower(), wordName));
                bases.Add(Path.Combine(current, "Sound", letter.ToLower(), wordName));
                bases.Add(Path.Combine(app, "Sound", wordName));
                bases.Add(Path.Combine(project, "Sound", wordName));
                bases.Add(Path.Combine(current, "Sound", wordName));
                bases.Add(Path.Combine("Sound", letter, wordName));
            }

            List<string> candidates = new List<string>();
            foreach (string b in bases) AddSoundCandidates(candidates, b);

            foreach (string c in candidates)
            {
                try
                {
                    string full = Path.GetFullPath(c);
                    if (File.Exists(full)) return full;
                }
                catch { }
            }

            foreach (string folder in SoundFolders(word))
            {
                try
                {
                    if (!Directory.Exists(folder)) continue;
                    foreach (string file in Directory.GetFiles(folder, "*.*"))
                    {
                        if (string.Equals(Path.GetFileNameWithoutExtension(file), wordName, StringComparison.OrdinalIgnoreCase)) return Path.GetFullPath(file);
                    }
                }
                catch { }
            }

            return null;
        }

        private void AddSoundCandidates(List<string> list, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            string p = path.Trim().Trim('"');
            string[] exts = new string[] { ".mp3", ".wav", ".wma", ".m4a", ".aac", ".mid", ".midi" };

            AddUnique(list, p);

            string ext = "";
            try { ext = Path.GetExtension(p); } catch { }

            if (string.IsNullOrEmpty(ext))
            {
                foreach (string e in exts) AddUnique(list, p + e);
            }
            else
            {
                string noExt = p.Substring(0, p.Length - ext.Length);
                foreach (string e in exts) AddUnique(list, noExt + e);
            }
        }

        private IEnumerable<string> SoundFolders(WordItem word)
        {
            string letter = FirstLetterFolder(word == null ? "" : word.Word);
            string[] roots = new string[] { Application.StartupPath, GetProjectRoot(), Environment.CurrentDirectory };

            foreach (string root in roots)
            {
                yield return Path.Combine(root, "Sound", letter);
                yield return Path.Combine(root, "Sound", letter.ToLower());
                yield return Path.Combine(root, "Sound");
            }

            string raw = word == null ? "" : (word.SoundPath ?? "").Trim();
            if (raw.Length > 0)
            {
                string dir = "";
                try { dir = Path.GetDirectoryName(raw); } catch { dir = ""; }
                if (!string.IsNullOrEmpty(dir))
                {
                    if (Path.IsPathRooted(dir))
                    {
                        yield return dir;
                    }
                    else
                    {
                        foreach (string root in roots) yield return Path.Combine(root, dir);
                    }
                }
            }
        }

        private string ExpectedSoundText(WordItem word)
        {
            if (word == null) return "";
            if (!string.IsNullOrWhiteSpace(word.SoundPath)) return word.SoundPath;
            return Path.Combine("Sound", FirstLetterFolder(word.Word), SafeFileName(word.Word) + ".mp3");
        }

        private string SafeFileName(string text)
        {
            if (text == null) return "";
            string s = text.Trim();
            foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c.ToString(), "");
            return s;
        }

        private string FirstLetterFolder(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return "A";
            char c = word.Trim()[0];
            return char.IsLetter(c) ? char.ToUpper(c).ToString() : "A";
        }

        private void MoveNext(bool play)
        {
            if (lstWords.Items.Count == 0) return;

            string mode = cboReviewMode.SelectedItem == null ? "一般複習" : cboReviewMode.SelectedItem.ToString();
            if (mode == "隨機複習")
            {
                SelectRandomWord(play);
                return;
            }

            List<WordItem> queue = lstWords.Items.Cast<WordItem>().ToList();
            if (mode == "智慧複習") queue = queue.OrderByDescending(w => GetReviewPriority(w)).ThenBy(w => w.Word).ToList();
            if (mode == "間隔複習") queue = queue.Where(w => IsDueForReview(w)).OrderByDescending(w => GetReviewPriority(w)).ThenBy(w => w.Word).ToList();
            if (mode == "錯題優先") queue = queue.Where(w => w.WrongCount > 0).OrderByDescending(w => w.WrongCount).ThenBy(w => w.Word).ToList();
            if (mode == "收藏複習") queue = queue.Where(w => w.IsFavorite).ToList();
            if (mode == "未熟悉複習") queue = queue.Where(w => !w.IsKnown).ToList();

            if (queue.Count > 0)
            {
                WordItem currentWord = GetSelectedWord();
                int currentIndex = currentWord == null ? -1 : queue.IndexOf(currentWord);
                WordItem next = queue[(currentIndex + 1 + queue.Count) % queue.Count];
                SelectWord(next, play);
                return;
            }

            lstWords.SelectedIndex = lstWords.SelectedIndex + 1 >= lstWords.Items.Count ? 0 : lstWords.SelectedIndex + 1;
            CenterSelectedItem();
            ShowSelectedWord(play);
        }

        private void MovePrevious(bool play)
        {
            if (lstWords.Items.Count == 0) return;
            lstWords.SelectedIndex = lstWords.SelectedIndex <= 0 ? lstWords.Items.Count - 1 : lstWords.SelectedIndex - 1;
            CenterSelectedItem();
            ShowSelectedWord(play);
        }

        private void SelectRandomWord(bool play)
        {
            if (lstWords.Items.Count == 0) return;
            lstWords.SelectedIndex = _random.Next(lstWords.Items.Count);
            CenterSelectedItem();
            ShowSelectedWord(play);
        }

        private void SelectWord(WordItem word, bool play)
        {
            if (word == null) return;

            if (!lstWords.Items.Contains(word))
            {
                cboFilter.SelectedItem = "全部";
                txtSearch.Clear();
                RefreshWordList(false);
            }

            if (lstWords.Items.Contains(word))
            {
                lstWords.SelectedItem = word;
                CenterSelectedItem();
                ShowSelectedWord(play);
            }
        }

        private void CenterSelectedItem()
        {
            if (lstWords.SelectedIndex < 0) return;
            int rows = Math.Max(1, lstWords.Height / Math.Max(1, lstWords.ItemHeight));
            lstWords.TopIndex = Math.Max(0, lstWords.SelectedIndex - rows / 2);
        }

        private void ToggleAutoPlay()
        {
            if (!_autoPlaying)
            {
                _autoPlaying = true;
                btnAuto.Text = "停止";
                _autoTimer.Interval = Math.Max(1, (int)nudInterval.Value) * 1000;
                _autoTimer.Start();
                PlaySelectedWord();
                SetStatus("自動播放中");
            }
            else
            {
                _autoPlaying = false;
                btnAuto.Text = "自動播放";
                _autoTimer.Stop();
                SetStatus("已停止自動播放");
            }
        }

        private void ToggleFavorite()
        {
            WordItem word = GetSelectedWord();
            if (word == null) return;
            word.IsFavorite = !word.IsFavorite;
            btnFavorite.Text = word.IsFavorite ? "★ 收藏" : "☆ 收藏";
            SaveProgress();
            UpdateStats();
            lstWords.Invalidate();
            SetStatus(word.IsFavorite ? "已加入收藏" : "已取消收藏");
        }

        private void ToggleKnown()
        {
            WordItem word = GetSelectedWord();
            if (word == null) return;
            word.IsKnown = !word.IsKnown;
            btnKnown.Text = word.IsKnown ? "已熟悉" : "未熟悉";
            SaveProgress();
            RefreshWordStats(word);
            UpdateStats();
            lstWords.Invalidate();
            SetStatus(word.IsKnown ? "已標記為熟悉" : "已標記為未熟悉");
        }

        private void ToggleExplain()
        {
            _hideExplain = !_hideExplain;
            ShowWord(GetSelectedWord());
        }

        private void CheckSpelling()
        {
            WordItem word = GetSelectedWord();
            if (word == null) return;

            string answer = txtSpell.Text.Trim();
            if (answer.Length == 0)
            {
                lblSpellResult.Text = "請先輸入答案。";
                return;
            }

            if (string.Equals(answer, word.Word, StringComparison.OrdinalIgnoreCase))
            {
                word.CorrectCount++;
                word.LastReviewed = DateTime.Now;
                lblSpellResult.Text = "答對了！";
                txtSpell.Clear();
                SetStatus("拼字答對：" + word.Word);
            }
            else
            {
                word.WrongCount++;
                word.LastReviewed = DateTime.Now;
                lblSpellResult.Text = "答錯了，正確答案是：" + word.Word;
                SetStatus("拼字答錯：" + word.Word);
            }

            SaveProgress();
            RefreshWordStats(word);
            UpdateStats();
        }

        private void RevealSpellingAnswer()
        {
            WordItem word = GetSelectedWord();
            if (word == null) return;
            txtSpell.Text = word.Word;
            txtSpell.SelectAll();
            txtSpell.Focus();
            lblSpellResult.Text = "答案：" + word.Word;
        }

        private void MakeQuiz()
        {
            if (lblQuizQuestion == null || btnChoices[0] == null) return;

            WordItem word = GetSelectedWord();
            lblQuizResult.Text = "";

            if (word == null)
            {
                lblQuizQuestion.Text = "";
                foreach (Button b in btnChoices)
                {
                    b.Text = "";
                    b.Enabled = false;
                    b.Tag = null;
                }
                return;
            }

            string correctText = ExplanationParser.Parse(word.Word, word.Explain).QuizText;
            if (correctText.Length == 0)
            {
                lblQuizQuestion.Text = "目前單字沒有可用於測驗的清楚解釋";
                DisableQuizButtons();
                return;
            }

            List<WordItem> pool = _words
                .Where(w => w != word)
                .Where(w => ExplanationParser.Parse(w.Word, w.Explain).QuizText.Length > 0)
                .GroupBy(w => ExplanationParser.Parse(w.Word, w.Explain).QuizText)
                .Select(g => g.First())
                .Where(w => ExplanationParser.Parse(w.Word, w.Explain).QuizText != correctText)
                .OrderBy(w => _random.Next())
                .Take(3)
                .ToList();

            if (pool.Count < 3)
            {
                lblQuizQuestion.Text = "選擇題需要至少 4 個有清楚解釋的單字";
                DisableQuizButtons();
                return;
            }

            _quizAnswer = word;
            lblQuizQuestion.Text = "請選出「" + word.Word + "」的正確解釋";

            List<QuizOption> options = new List<QuizOption>();
            options.Add(new QuizOption(word, correctText));
            foreach (WordItem decoy in pool) options.Add(new QuizOption(decoy, ExplanationParser.Parse(decoy.Word, decoy.Explain).QuizText));
            options = options.OrderBy(x => _random.Next()).ToList();

            for (int i = 0; i < btnChoices.Length; i++)
            {
                string text = options[i].Text;
                if (text.Length > 90) text = text.Substring(0, 90) + "…";
                btnChoices[i].Text = (i + 1).ToString() + ". " + text;
                btnChoices[i].Tag = options[i].Word;
                btnChoices[i].Enabled = true;
                btnChoices[i].BackColor = _darkMode ? Color.FromArgb(38, 48, 70) : Color.FromArgb(248, 250, 255);
                btnChoices[i].ForeColor = _darkMode ? Color.FromArgb(235, 240, 250) : Color.FromArgb(34, 44, 65);
            }
        }

        private void DisableQuizButtons()
        {
            _quizAnswer = null;
            foreach (Button b in btnChoices)
            {
                b.Text = "";
                b.Tag = null;
                b.Enabled = false;
            }
        }

        private void CheckChoice(int index)
        {
            if (_quizAnswer == null || index < 0 || index >= btnChoices.Length) return;

            WordItem chosen = btnChoices[index].Tag as WordItem;
            if (chosen == null) return;

            if (chosen == _quizAnswer)
            {
                _quizAnswer.CorrectCount++;
                _quizAnswer.LastReviewed = DateTime.Now;
                btnChoices[index].BackColor = Color.FromArgb(196, 246, 215);
                lblQuizResult.Text = "答對了！";
                SetStatus("選擇題答對：" + _quizAnswer.Word);
            }
            else
            {
                _quizAnswer.WrongCount++;
                _quizAnswer.LastReviewed = DateTime.Now;
                btnChoices[index].BackColor = Color.FromArgb(255, 214, 214);
                foreach (Button b in btnChoices)
                {
                    if ((b.Tag as WordItem) == _quizAnswer) b.BackColor = Color.FromArgb(196, 246, 215);
                }
                lblQuizResult.Text = "答錯了，正確答案是：" + ExplanationParser.Parse(_quizAnswer.Word, _quizAnswer.Explain).QuizText;
                SetStatus("選擇題答錯：" + _quizAnswer.Word);
            }

            foreach (Button b in btnChoices) b.Enabled = false;
            SaveProgress();
            RefreshWordStats(_quizAnswer);
            UpdateStats();
        }

        private void AddWord()
        {
            WordItem item = new WordItem();
            frmEditWord edit = new frmEditWord(item);
            if (edit.ShowDialog(this) == DialogResult.Yes)
            {
                _words.Add(item);
                SaveAll();
                RefreshWordList(false);
                SelectWord(item, false);
                SetStatus("已新增單字");
            }
        }

        private void EditWord()
        {
            WordItem word = GetSelectedWord();
            if (word == null) return;

            frmEditWord edit = new frmEditWord(word);
            if (edit.ShowDialog(this) == DialogResult.Yes)
            {
                SaveAll();
                RefreshWordList(true);
                ShowWord(word);
                SetStatus("已更新單字");
            }
        }

        private void DeleteWord()
        {
            WordItem word = GetSelectedWord();
            if (word == null) return;

            if (MessageBox.Show("確定刪除「" + word.Word + "」？", "確認刪除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            _words.Remove(word);
            SaveAll();
            RefreshWordList(false);
            SetStatus("已刪除單字");
        }

        private void ImportWords()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "文字檔|*.txt|所有檔案|*.*";

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                WordCollection temp = new WordCollection();
                temp.LoadFromStringArray(File.ReadAllLines(dlg.FileName, Encoding.UTF8));

                int added = 0;
                int updated = 0;

                foreach (WordItem item in temp)
                {
                    WordItem old = _words.FirstOrDefault(w => string.Equals(w.Word, item.Word, StringComparison.OrdinalIgnoreCase));
                    if (old == null)
                    {
                        _words.Add(item);
                        added++;
                    }
                    else
                    {
                        old.Phonogram = item.Phonogram;
                        old.SoundPath = item.SoundPath;
                        old.Explain = item.Explain;
                        updated++;
                    }
                }

                SaveAll();
                SetStatus("匯入完成，新增 " + added + " 個，更新 " + updated + " 個");
            }
            catch (Exception ex)
            {
                MessageBox.Show("匯入失敗：" + Environment.NewLine + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportWords(IEnumerable<WordItem> list, string defaultFileName)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "文字檔|*.txt";
            dlg.FileName = defaultFileName;

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                WordCollection export = new WordCollection();
                foreach (WordItem item in list) export.Add(item);
                export.SaveToFile(dlg.FileName);
                SetStatus("匯出完成：" + dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("匯出失敗：" + Environment.NewLine + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BackupWords()
        {
            try
            {
                string dir = Path.GetDirectoryName(Path.GetFullPath(_wordFile));
                if (string.IsNullOrEmpty(dir)) dir = Application.StartupPath;
                string file = Path.Combine(dir, "WordCards_Backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
                File.Copy(_wordFile, file, true);
                SaveProgress();
                SetStatus("已備份：" + file);
            }
            catch (Exception ex)
            {
                MessageBox.Show("備份失敗：" + Environment.NewLine + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckSoundFiles()
        {
            List<string> missing = new List<string>();

            foreach (WordItem item in _words)
            {
                if (ResolveSoundPath(item) == null) missing.Add(item.Word + " → " + ExpectedSoundText(item));
            }

            if (missing.Count == 0)
            {
                MessageBox.Show("全部音檔都找得到。", "音檔檢查", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus("全部音檔都找得到");
            }
            else
            {
                string msg = "缺少 " + missing.Count + " 個音檔：" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, missing.Take(20).ToArray());
                if (missing.Count > 20) msg += Environment.NewLine + "...";
                MessageBox.Show(msg, "音檔檢查", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SetStatus("缺少 " + missing.Count + " 個音檔");
            }

            UpdateStats();
        }

        private void OpenCurrentFolder()
        {
            try
            {
                WordItem word = GetSelectedWord();
                string sound = word == null ? null : ResolveSoundPath(word);

                if (!string.IsNullOrEmpty(sound) && File.Exists(sound))
                {
                    Process.Start("explorer.exe", "/select,\"" + sound + "\"");
                    return;
                }

                string dir = Path.GetDirectoryName(Path.GetFullPath(_wordFile));
                if (string.IsNullOrEmpty(dir)) dir = Application.StartupPath;
                Process.Start("explorer.exe", "\"" + dir + "\"");
            }
            catch { }
        }

        private void ResetCurrentProgress()
        {
            WordItem word = GetSelectedWord();
            if (word == null) return;

            if (MessageBox.Show("確定重置「" + word.Word + "」的練習進度？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            word.CorrectCount = 0;
            word.WrongCount = 0;
            word.LastReviewed = null;
            SaveProgress();
            RefreshWordStats(word);
            UpdateStats();
            SetStatus("已重置目前單字進度");
        }

        private void ResetAllProgress()
        {
            if (MessageBox.Show("確定重置全部單字的練習進度？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            foreach (WordItem w in _words)
            {
                w.CorrectCount = 0;
                w.WrongCount = 0;
                w.LastReviewed = null;
            }

            SaveProgress();
            RefreshWordStats(GetSelectedWord());
            UpdateStats();
            SetStatus("已重置全部進度");
        }


        private void FillCurrentMeaning()
        {
            WordItem word = GetSelectedWord();
            if (word == null) return;

            if (TryFillMeaning(word))
            {
                SaveAll();
                ShowWord(word);
                SetStatus("已補齊目前單字解釋：" + word.Word);
            }
            else
            {
                SetStatus("目前單字已有清楚解釋，或內建字典沒有此單字：" + word.Word);
            }
        }

        private void FillAllMissingMeanings()
        {
            int count = 0;

            foreach (WordItem word in _words)
            {
                if (TryFillMeaning(word)) count++;
            }

            if (count > 0)
            {
                SaveAll();
                ShowWord(GetSelectedWord());
                SetStatus("已補齊 " + count + " 個單字解釋，並已儲存");
                MessageBox.Show("已補齊 " + count + " 個單字解釋，並寫回 WordCards.txt。", "補齊完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                SetStatus("沒有需要補齊、或內建字典找不到可補齊的單字");
                MessageBox.Show("沒有需要補齊、或內建字典找不到可補齊的單字。", "補齊完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool TryFillMeaning(WordItem word)
        {
            if (word == null) return false;

            string fallback = BuiltInLexicon.Get(word.Word);
            if (string.IsNullOrWhiteSpace(fallback)) return false;

            if (!ExplanationParser.NeedsBuiltInMeaning(word.Word, word.Explain)) return false;

            string original = (word.Explain ?? "").Trim();
            if (original.Length == 0)
            {
                word.Explain = fallback;
            }
            else if (original.IndexOf(fallback, StringComparison.OrdinalIgnoreCase) < 0)
            {
                word.Explain = original + Environment.NewLine + fallback;
            }

            return true;
        }

        private void CleanAllMeanings()
        {
            int changed = 0;

            foreach (WordItem word in _words)
            {
                string before = word.Explain ?? "";
                ExplanationView view = ExplanationParser.Parse(before);
                List<string> lines = new List<string>();

                if (!string.IsNullOrWhiteSpace(view.Source)) lines.Add(view.Source);
                if (!string.IsNullOrWhiteSpace(view.Meaning) && view.Meaning != "尚無清楚解釋。") lines.Add(view.Meaning);
                if (!string.IsNullOrWhiteSpace(view.Supplement)) lines.Add(view.Supplement);

                string after = string.Join(Environment.NewLine, lines.Distinct().ToArray()).Trim();
                if (after.Length == 0) after = before.Trim();

                if (after != before.Trim())
                {
                    word.Explain = after;
                    changed++;
                }
            }

            if (changed > 0)
            {
                SaveAll();
                ShowWord(GetSelectedWord());
                MessageBox.Show("已清理 " + changed + " 筆解釋內容。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus("已清理 " + changed + " 筆解釋內容");
            }
            else
            {
                MessageBox.Show("目前沒有需要清理的解釋內容。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus("目前沒有需要清理的解釋內容");
            }
        }

        private void ApplySmartReviewSort()
        {
            cboFilter.SelectedItem = "全部";
            cboSort.SelectedItem = "原始順序";
            cboReviewMode.SelectedItem = "智慧複習";

            List<WordItem> ordered = _words.OrderByDescending(w => GetReviewPriority(w)).ThenBy(w => w.Word).ToList();

            lstWords.BeginUpdate();
            lstWords.Items.Clear();
            foreach (WordItem item in ordered) lstWords.Items.Add(item);
            lstWords.EndUpdate();

            if (lstWords.Items.Count > 0) lstWords.SelectedIndex = 0;
            SetStatus("已依智慧複習分數排序");
        }

        private void ExportStudyReport()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "CSV 檔|*.csv|文字檔|*.txt";
            dlg.FileName = "WordCards_StudyReport.csv";

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                List<string> lines = new List<string>();
                lines.Add("Word,Phonogram,Meaning,Favorite,Known,Correct,Wrong,Accuracy,ReviewAdvice,LastReviewed,SoundPath,SoundExists");

                foreach (WordItem word in _words.OrderByDescending(w => GetReviewPriority(w)).ThenBy(w => w.Word))
                {
                    ExplanationView view = ExplanationParser.Parse(word.Explain);
                    string sound = ResolveSoundPath(word);
                    lines.Add(string.Join(",", new string[]
                    {
                        Csv(word.Word),
                        Csv(word.Phonogram),
                        Csv(view.QuizText),
                        Csv(word.IsFavorite ? "Y" : "N"),
                        Csv(word.IsKnown ? "Y" : "N"),
                        Csv(word.CorrectCount.ToString()),
                        Csv(word.WrongCount.ToString()),
                        Csv(word.AccuracyText),
                        Csv(GetReviewAdvice(word)),
                        Csv(word.LastReviewed.HasValue ? word.LastReviewed.Value.ToString("yyyy/MM/dd HH:mm") : ""),
                        Csv(sound ?? ExpectedSoundText(word)),
                        Csv(sound == null ? "N" : "Y")
                    }));
                }

                File.WriteAllLines(dlg.FileName, lines.ToArray(), Encoding.UTF8);
                SetStatus("已匯出學習報告：" + dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("匯出失敗：" + Environment.NewLine + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportAnkiCards()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "TSV 檔|*.tsv|文字檔|*.txt";
            dlg.FileName = "WordCards_Anki.tsv";

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                List<string> lines = new List<string>();
                foreach (WordItem word in _words)
                {
                    ExplanationView view = ExplanationParser.Parse(word.Explain);
                    string front = word.Word + (string.IsNullOrWhiteSpace(word.Phonogram) ? "" : " " + word.Phonogram);
                    string back = view.Meaning;
                    if (!string.IsNullOrWhiteSpace(view.Detail)) back += "<br>" + view.Detail.Replace(Environment.NewLine, "<br>");
                    lines.Add(Tsv(front) + "\t" + Tsv(back));
                }

                File.WriteAllLines(dlg.FileName, lines.ToArray(), Encoding.UTF8);
                SetStatus("已匯出 Anki TSV：" + dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("匯出失敗：" + Environment.NewLine + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetReviewPriority(WordItem word)
        {
            if (word == null) return 0;

            int score = 0;
            ExplanationView view = ExplanationParser.Parse(word.Explain);

            if (!word.IsKnown) score += 18;
            if (word.IsFavorite) score += 4;
            if (view.QuizText.Length == 0) score += 14;
            if (ResolveSoundPath(word) == null) score += 6;
            score += Math.Min(40, word.WrongCount * 8);

            if (word.TotalAnswer == 0) score += 10;
            else
            {
                double missRate = 100.0 - word.Accuracy;
                score += (int)Math.Min(35, missRate / 3.0);
            }

            if (!word.LastReviewed.HasValue) score += 30;
            else
            {
                int days = (DateTime.Today - word.LastReviewed.Value.Date).Days;
                if (days > 0) score += Math.Min(35, days * 4);
            }

            return score;
        }

        private bool IsDueForReview(WordItem word)
        {
            if (word == null) return false;
            if (!word.LastReviewed.HasValue) return true;

            int interval = GetSuggestedIntervalDays(word);
            return word.LastReviewed.Value.Date.AddDays(interval) <= DateTime.Today;
        }

        private int GetSuggestedIntervalDays(WordItem word)
        {
            if (word == null) return 1;
            if (word.WrongCount > word.CorrectCount) return 1;
            if (!word.IsKnown) return 2;
            if (word.TotalAnswer == 0) return 1;
            if (word.Accuracy < 60) return 1;
            if (word.Accuracy < 80) return 3;
            if (word.Accuracy < 95) return 7;
            return 14;
        }

        private string GetReviewAdvice(WordItem word)
        {
            if (word == null) return "";
            if (ExplanationParser.Parse(word.Explain).QuizText.Length == 0) return "缺解釋";
            if (!word.LastReviewed.HasValue) return "新單字";
            if (IsDueForReview(word)) return "該複習";
            if (word.WrongCount > word.CorrectCount) return "加強";
            if (word.IsKnown && word.Accuracy >= 90) return "穩定";
            return "觀察";
        }

        private string Csv(string text)
        {
            text = text ?? "";
            text = text.Replace("\"", "\"\"");
            return "\"" + text + "\"";
        }

        private string Tsv(string text)
        {
            text = text ?? "";
            return text.Replace("\t", " ").Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>");
        }

        private void CheckDuplicates()
        {
            var duplicates = _words.GroupBy(w => w.Word, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).ToList();

            if (duplicates.Count == 0)
            {
                MessageBox.Show("沒有重複單字。", "檢查重複", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string msg = "找到 " + duplicates.Count + " 組重複：" + Environment.NewLine + string.Join(Environment.NewLine, duplicates.Take(20).Select(g => g.Key + " × " + g.Count()).ToArray());
                MessageBox.Show(msg, "檢查重複", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateStats()
        {
            int total = _words.Count;
            int known = _words.Count(w => w.IsKnown);
            int favorite = _words.Count(w => w.IsFavorite);
            int correct = _words.Sum(w => w.CorrectCount);
            int wrong = _words.Sum(w => w.WrongCount);
            int missing = _words.Count(w => ResolveSoundPath(w) == null);
            int reviewedToday = _words.Count(w => w.LastReviewed.HasValue && w.LastReviewed.Value.Date == DateTime.Today);
            string accuracy = correct + wrong == 0 ? "--" : (correct * 100.0 / (correct + wrong)).ToString("0") + "%";

            if (lblTotalWords != null) lblTotalWords.Text = total.ToString();
            if (lblKnownWords != null) lblKnownWords.Text = known.ToString();
            if (lblFavoriteWords != null) lblFavoriteWords.Text = favorite.ToString();
            if (lblAccuracy != null) lblAccuracy.Text = accuracy;
            if (lblMissingSound != null) lblMissingSound.Text = missing.ToString();

            if (lblGoal != null && prgGoal != null)
            {
                lblGoal.Text = "今日已複習 " + reviewedToday + " / " + _dailyGoal + " 個單字";
                prgGoal.Maximum = _dailyGoal;
                prgGoal.Value = Math.Min(_dailyGoal, reviewedToday);
            }

            RefreshStatsList();
        }

        private void RefreshStatsList()
        {
            if (lvStats == null) return;

            lvStats.BeginUpdate();
            lvStats.Items.Clear();

            foreach (WordItem w in _words.OrderByDescending(x => x.WrongCount).ThenBy(x => x.Word))
            {
                ListViewItem item = new ListViewItem(w.Word);
                item.SubItems.Add((w.IsFavorite ? "收藏 " : "") + (w.IsKnown ? "熟悉" : "未熟悉"));
                item.SubItems.Add(w.CorrectCount.ToString());
                item.SubItems.Add(w.WrongCount.ToString());
                item.SubItems.Add(w.AccuracyText);
                item.SubItems.Add(GetReviewAdvice(w));
                item.SubItems.Add(w.LastReviewed.HasValue ? w.LastReviewed.Value.ToString("yyyy/MM/dd HH:mm") : "尚未複習");
                item.Tag = w;
                lvStats.Items.Add(item);
            }

            lvStats.EndUpdate();
        }

        private void RefreshInfo()
        {
            if (lvInfo == null) return;

            lvInfo.Items.Clear();
            AddInfo("單字檔", _wordFile);
            AddInfo("進度檔", _progressFile);
            AddInfo("執行位置", Application.StartupPath);
            AddInfo("專案位置推測", GetProjectRoot());
            AddInfo("音檔搜尋", @"Sound\A\abacus.mp3，也支援 wav、wma、m4a、aac、mid");
            AddInfo("智慧複習", "依答錯次數、正確率、最後複習時間、是否缺解釋與音檔自動排序");
            AddInfo("間隔複習", "依熟悉度與答題表現估算下次應複習時間");

            WordItem word = GetSelectedWord();
            if (word != null)
            {
                AddInfo("目前單字", word.Word);
                AddInfo("目前音檔", ResolveSoundPath(word) ?? "找不到：" + ExpectedSoundText(word));
            }
        }

        private void AddInfo(string name, string value)
        {
            ListViewItem item = new ListViewItem(name);
            item.SubItems.Add(value);
            lvInfo.Items.Add(item);
        }

        private bool ContainsText(string source, string key)
        {
            return (source ?? "").IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void AddUnique(List<string> list, string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && !list.Any(x => string.Equals(x, path, StringComparison.OrdinalIgnoreCase))) list.Add(path);
        }

        private string GetProjectRoot()
        {
            try { return Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\")); }
            catch { return Application.StartupPath; }
        }

        private string FindExistingFile(string[] candidates)
        {
            foreach (string p in candidates)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(p) && File.Exists(p)) return Path.GetFullPath(p);
                }
                catch { }
            }
            return null;
        }

        private void SetStatus(string text)
        {
            if (lblStatus != null) lblStatus.Text = text;
            RefreshInfo();
        }

        private void lstWords_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            WordItem word = lstWords.Items[e.Index] as WordItem;
            if (word == null) return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bg = selected ? Color.FromArgb(73, 82, 255) : (_darkMode ? Color.FromArgb(30, 38, 55) : Color.White);
            Color fg = selected ? Color.White : (_darkMode ? Color.FromArgb(236, 240, 250) : Color.FromArgb(34, 44, 65));
            Color sub = selected ? Color.FromArgb(232, 238, 255) : Color.FromArgb(112, 124, 148);

            using (SolidBrush brush = new SolidBrush(bg)) e.Graphics.FillRectangle(brush, e.Bounds);

            string text = (word.IsFavorite ? "★  " : "   ") + word.Word;
            using (SolidBrush brush = new SolidBrush(fg))
            {
                e.Graphics.DrawString(text, lstWords.Font, brush, e.Bounds.Left + 8, e.Bounds.Top + 7);
            }

            string badge = word.WrongCount > 0 ? "錯 " + word.WrongCount : (word.IsKnown ? "✓" : "");
            SizeF size = e.Graphics.MeasureString(badge, lstWords.Font);
            using (SolidBrush brush = new SolidBrush(sub))
            {
                e.Graphics.DrawString(badge, lstWords.Font, brush, e.Bounds.Right - size.Width - 8, e.Bounds.Top + 7);
            }
        }

        private void lstWordList_Click(object sender, EventArgs e)
        {
            if (_autoPlaying) ToggleAutoPlay();
            PlaySelectedWord();
        }

        private void lstWordList_DoubleClick(object sender, EventArgs e)
        {
            EditWord();
        }

        private void timPlayer_Tick(object sender, EventArgs e)
        {
            MoveNext(true);
        }

        private void btnAutoPlay_Click(object sender, EventArgs e)
        {
            ToggleAutoPlay();
        }

        private void frmWordCards_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (_autoPlaying) return;
            if (txtSearch.Focused || txtSpell.Focused || rtbNote.Focused) return;

            if (e.KeyChar == (char)Keys.Return)
            {
                MoveNext(true);
                e.Handled = true;
            }
            else if (e.KeyChar == (char)Keys.Space)
            {
                PlaySelectedWord();
                e.Handled = true;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtSearch.Focused || txtSpell.Focused || rtbNote.Focused) return;

            if (e.KeyCode == Keys.Right)
            {
                MoveNext(true);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Left)
            {
                MovePrevious(true);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F)
            {
                ToggleFavorite();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.K)
            {
                ToggleKnown();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.H)
            {
                ToggleExplain();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                SaveAll();
                e.Handled = true;
            }
        }

        private void ToggleDarkMode()
        {
            _darkMode = !_darkMode;
            btnDarkMode.Text = _darkMode ? "淺色模式" : "深色模式";
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            if (pnlMain == null) return;

            Color bg = _darkMode ? Color.FromArgb(18, 24, 36) : Color.FromArgb(242, 246, 252);
            Color card = _darkMode ? Color.FromArgb(30, 38, 55) : Color.White;
            Color soft = _darkMode ? Color.FromArgb(38, 48, 70) : Color.FromArgb(248, 250, 255);
            Color text = _darkMode ? Color.FromArgb(236, 240, 250) : Color.FromArgb(34, 44, 65);
            Color accent = Color.FromArgb(73, 82, 255);

            BackColor = bg;
            pnlMain.BackColor = bg;
            pnlSide.BackColor = bg;
            pnlFooter.BackColor = card;

            foreach (TabPage page in tabMain.TabPages) page.BackColor = bg;

            ApplyThemeToControls(this, card, soft, text, accent);

            if (lblWord != null) lblWord.ForeColor = accent;
            if (lblPhonogram != null) lblPhonogram.ForeColor = Color.FromArgb(13, 152, 105);
            if (lblStatus != null) lblStatus.ForeColor = Color.FromArgb(224, 68, 68);
            if (lblSpellResult != null) lblSpellResult.ForeColor = accent;
            if (lblQuizResult != null) lblQuizResult.ForeColor = accent;
            if (btnDarkMode != null)
            {
                btnDarkMode.BackColor = Color.White;
                btnDarkMode.ForeColor = accent;
            }

            pnlHeader.Invalidate();
            lstWords.Invalidate();
            MakeQuiz();
        }

        private void ApplyThemeToControls(Control parent, Color card, Color soft, Color text, Color accent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Panel && c != pnlHeader && c != pnlMain && c != pnlSide && c != pnlFooter)
                {
                    c.BackColor = card;
                }

                if (c is GroupBox)
                {
                    c.BackColor = card;
                    c.ForeColor = text;
                }

                if (c is TextBox || c is RichTextBox || c is ComboBox || c is ListBox || c is ListView)
                {
                    c.BackColor = soft;
                    c.ForeColor = text;
                }

                if (c is Label)
                {
                    c.ForeColor = text;
                }

                if (c is Button && c != btnDarkMode)
                {
                    Button b = c as Button;
                    b.BackColor = soft;
                    b.ForeColor = _darkMode ? Color.FromArgb(214, 222, 255) : accent;
                }

                ApplyThemeToControls(c, card, soft, text, accent);
            }
        }

        private class QuizOption
        {
            public WordItem Word;
            public string Text;

            public QuizOption(WordItem word, string text)
            {
                Word = word;
                Text = text;
            }
        }
    }

    public class WordItem
    {
        public string Word { get; set; }
        public string Phonogram { get; set; }
        public string Explain { get; set; }
        public string SoundPath { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsKnown { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public DateTime? LastReviewed { get; set; }
        public string Note { get; set; }

        public WordItem()
        {
            Word = "";
            Phonogram = "";
            Explain = "";
            SoundPath = "";
            Note = "";
        }

        public int TotalAnswer
        {
            get { return CorrectCount + WrongCount; }
        }

        public double Accuracy
        {
            get { return TotalAnswer == 0 ? 0 : CorrectCount * 100.0 / TotalAnswer; }
        }

        public string AccuracyText
        {
            get { return TotalAnswer == 0 ? "--" : Accuracy.ToString("0") + "%"; }
        }

        public string ToLineString()
        {
            string ex = (Explain ?? "").Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\\n");
            return string.Join("\t", new string[] { Word ?? "", Phonogram ?? "", SoundPath ?? "", ex });
        }

        public override string ToString()
        {
            return Word;
        }
    }

    public class WordCollection : List<WordItem>
    {
        public void LoadFromStringArray(string[] lines)
        {
            foreach (string raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;

                string[] p = raw.Contains("\t") ? raw.Split('\t') : raw.Split(',');

                WordItem item = new WordItem();
                if (p.Length >= 1) item.Word = p[0].Trim();
                if (p.Length >= 2) item.Phonogram = p[1].Trim();
                if (p.Length >= 3) item.SoundPath = p[2].Trim();
                if (p.Length >= 4) item.Explain = string.Join(Environment.NewLine, p.Skip(3).ToArray()).Replace("\\n", Environment.NewLine);

                if (!string.IsNullOrWhiteSpace(item.Word)) Add(item);
            }
        }

        public void SaveToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                foreach (WordItem item in this)
                {
                    writer.WriteLine(item.ToLineString());
                }
            }
        }
    }

    public class ExplanationView
    {
        public string Meaning;
        public string Source;
        public string Supplement;
        public bool UsedBuiltIn;

        public string Detail
        {
            get
            {
                List<string> parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(Source)) parts.Add("字源：" + Source);
                if (!string.IsNullOrWhiteSpace(Supplement)) parts.Add("補充：" + Supplement);
                if (UsedBuiltIn) parts.Add("補充來源：內建解釋字典");
                return string.Join(Environment.NewLine, parts.ToArray());
            }
        }

        public string QuizText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Meaning)) return "";
                string[] lines = Meaning.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string s = CleanText(line);
                    if (s.Length >= 2 && s != "尚無清楚解釋。") return s;
                }
                return "";
            }
        }

        public static string CleanText(string s)
        {
            s = (s ?? "").Replace("　", " ");
            s = Regex.Replace(s, @"\s+", " ").Trim();
            return s;
        }
    }

    public static class ExplanationParser
    {
        public static ExplanationView Parse(string raw)
        {
            return Parse("", raw);
        }

        public static ExplanationView Parse(string word, string raw)
        {
            ParsedParts parts = ParseOriginal(raw);
            ExplanationView view = new ExplanationView();
            string builtIn = BuiltInLexicon.Get(word);

            if (parts.NormalMeanings.Count > 0)
            {
                view.Meaning = string.Join(Environment.NewLine, parts.NormalMeanings.Distinct().ToArray());
            }
            else if (!string.IsNullOrWhiteSpace(builtIn))
            {
                view.Meaning = builtIn;
                view.UsedBuiltIn = true;
            }
            else if (parts.TagMeanings.Count > 0)
            {
                view.Meaning = string.Join(Environment.NewLine, parts.TagMeanings.Distinct().ToArray());
            }
            else
            {
                string fallback = ExtractChinese(raw ?? "");
                view.Meaning = fallback.Length > 0 ? fallback : "尚無清楚解釋。";
            }

            view.Source = string.Join(Environment.NewLine, parts.Sources.Distinct().ToArray());
            view.Supplement = string.Join(Environment.NewLine, parts.Supplements.Distinct().ToArray());
            return view;
        }

        public static bool NeedsBuiltInMeaning(string word, string raw)
        {
            if (string.IsNullOrWhiteSpace(BuiltInLexicon.Get(word))) return false;
            ParsedParts parts = ParseOriginal(raw);
            return parts.NormalMeanings.Count == 0;
        }

        private static ParsedParts ParseOriginal(string raw)
        {
            ParsedParts result = new ParsedParts();
            string text = (raw ?? "").Replace("\\n", "\n").Replace("\r\n", "\n").Replace("\r", "\n").Replace("\t", "\n");
            string[] lines = text.Split('\n');

            foreach (string original in lines)
            {
                string line = Clean(original);
                if (line.Length == 0) continue;

                if (Regex.IsMatch(line, @"^補充\s*\d*[:：]?\s*$"))
                {
                    result.Supplements.Add(line);
                    continue;
                }

                if (Regex.IsMatch(line, @"^補充\s*\d*[:：]?"))
                {
                    string s = Regex.Replace(line, @"^補充\s*\d*[:：]?\s*", "");
                    if (s.Length > 0) result.Supplements.Add(s);
                    else result.Supplements.Add(line);
                    continue;
                }

                if (line.Contains("<") && line.Contains(">"))
                {
                    result.Sources.Add(Clean(line.Replace("+", " + ")));

                    MatchCollection tags = Regex.Matches(line, @"<([^>]*)>");
                    foreach (Match tag in tags)
                    {
                        string content = tag.Groups[1].Value;
                        string meaning = ExtractMeaningFromTag(content);
                        if (meaning.Length > 0) result.TagMeanings.Add(meaning);
                    }

                    string outside = Clean(Regex.Replace(line, @"<[^>]*>", " ").Replace("+", " "));
                    outside = CleanMeaning(outside);
                    if (outside.Length > 0 && HasCjk(outside) && LooksLikeMeaning(outside)) result.NormalMeanings.Add(outside);
                    continue;
                }

                string clean = CleanMeaning(line);
                if (clean.Length > 0 && LooksLikeMeaning(clean)) result.NormalMeanings.Add(clean);
            }

            return result;
        }

        private static bool LooksLikeMeaning(string s)
        {
            s = Clean(s);
            if (s.Length == 0) return false;
            if (!HasCjk(s)) return false;
            if (Regex.IsMatch(s, @"^補充\s*\d*$")) return false;
            if (Regex.IsMatch(s, @"^形聲")) return false;
            if (s.Length <= 1) return false;
            return true;
        }

        private static string ExtractMeaningFromTag(string tag)
        {
            string[] parts = tag.Split(new char[] { ';', '；' });
            for (int i = parts.Length - 1; i >= 0; i--)
            {
                string s = CleanMeaning(parts[i]);
                if (HasCjk(s))
                {
                    s = Regex.Replace(s, @"^[A-Za-z0-9_\-=/.:,\s]+", "").Trim();
                    if (s.Length >= 2) return s;
                }
            }
            return "";
        }

        private static string ExtractChinese(string text)
        {
            string s = text;
            s = Regex.Replace(s, @"<([^>]*)>", delegate (Match m)
            {
                string content = m.Groups[1].Value;
                string meaning = ExtractMeaningFromTag(content);
                return " " + meaning + " ";
            });

            s = Regex.Replace(s, @"[A-Za-z][A-Za-z0-9_\-=/.:;]*", " ");
            s = Regex.Replace(s, @"[`'""<>+\-=]", " ");
            MatchCollection matches = Regex.Matches(s, @"[\u3400-\u9FFF][\u3400-\u9FFF，。；、：:（）()「」『』\s]*");

            List<string> parts = new List<string>();
            foreach (Match match in matches)
            {
                string part = CleanMeaning(match.Value);
                if (part.Length >= 2 && !Regex.IsMatch(part, @"^補充\s*\d*$")) parts.Add(part);
            }

            return string.Join(Environment.NewLine, parts.Distinct().ToArray()).Trim();
        }

        private static string CleanMeaning(string s)
        {
            s = Clean(s);
            s = s.Replace("->", " ");
            s = s.Replace("=>", " ");
            s = Regex.Replace(s, @"^[\-+=>:：;；,\s]+", "");
            s = Regex.Replace(s, @"\s+", " ").Trim();
            return s;
        }

        private static string Clean(string s)
        {
            s = (s ?? "").Replace("　", " ");
            s = Regex.Replace(s, @"\s+", " ").Trim();
            return s;
        }

        private static bool HasCjk(string s)
        {
            return Regex.IsMatch(s ?? "", @"[\u3400-\u9FFF]");
        }

        private class ParsedParts
        {
            public List<string> NormalMeanings = new List<string>();
            public List<string> TagMeanings = new List<string>();
            public List<string> Sources = new List<string>();
            public List<string> Supplements = new List<string>();
        }
    }

    public static class BuiltInLexicon
    {
        private static Dictionary<string, string> _items = Create();

        public static string Get(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return "";
            string key = word.Trim();
            if (_items.ContainsKey(key)) return _items[key];
            string lower = key.ToLowerInvariant();
            if (_items.ContainsKey(lower)) return _items[lower];
            return "";
        }

        private static Dictionary<string, string> Create()
        {
            Dictionary<string, string> _items = new Dictionary<string, string>();
            _items["abacus"] = "算盤；用來計算的工具";
            _items["abandon"] = "放棄；拋棄；遺棄";
            _items["abase"] = "貶低；使卑微";
            _items["abate"] = "減少；減弱；緩和";
            _items["abbreviate"] = "縮寫；縮短";
            _items["abdicate"] = "退位；放棄職責";
            _items["abdomen"] = "腹部";
            _items["abduct"] = "綁架；誘拐";
            _items["aberrant"] = "異常的；偏離正道的";
            _items["abhor"] = "痛恨；憎惡";
            _items["abide"] = "忍受；遵守；居住";
            _items["abject"] = "悲慘的；卑微的";
            _items["abnormal"] = "異常的；不正常的";
            _items["aboard"] = "在船上；在飛機上；上車";
            _items["abolish"] = "廢除；取消";
            _items["aboriginal"] = "原住民的；土著的";
            _items["abort"] = "中止；流產";
            _items["abound"] = "大量存在；充滿";
            _items["abroad"] = "在國外；到處";
            _items["abrupt"] = "突然的；唐突的；陡峭的";
            _items["abscond"] = "潛逃；逃匿";
            _items["absence"] = "缺席；不存在";
            _items["absorb"] = "吸收；吸引注意";
            _items["abstract"] = "抽象的；摘要";
            _items["absurd"] = "荒謬的";
            _items["abundant"] = "豐富的；大量的";
            _items["abuse"] = "濫用；虐待";
            _items["abusive"] = "辱罵的；虐待的";
            _items["abut"] = "鄰接；毗連";
            _items["abysmal"] = "極糟的；深不可測的";
            _items["abyss"] = "深淵；無底洞";
            _items["academy"] = "學院；學會";
            _items["accede"] = "同意；加入；繼任";
            _items["accent"] = "口音；重音";
            _items["access"] = "進入；取得；通道";
            _items["accessible"] = "可接近的；可使用的";
            _items["accessory"] = "配件；附屬品；從犯";
            _items["accident"] = "意外；事故";
            _items["acclaim"] = "喝采；讚揚";
            _items["accompany"] = "陪伴；伴隨";
            _items["accord"] = "協議；一致；給予";
            _items["accrue"] = "累積；產生";
            _items["accumulate"] = "累積；積聚";
            _items["accurate"] = "準確的；精確的";
            _items["accuse"] = "指控；控告";
            _items["ace"] = "王牌；高手；么點牌";
            _items["acerbic"] = "尖酸的；刻薄的";
            _items["Achilles' heel"] = "致命弱點";
            _items["Achilles tendon"] = "阿基里斯腱；跟腱";
            _items["acid"] = "酸；酸性的；尖刻的";
            _items["acme"] = "頂點；巔峰";
            _items["acne"] = "痤瘡；青春痘";
            _items["acoustic"] = "聲音的；原聲的";
            _items["acoustics"] = "聲學；音響效果";
            _items["acquaint"] = "使熟悉；告知";
            _items["acquaintance"] = "熟人；認識";
            _items["acquire"] = "獲得；取得；學會";
            _items["acronym"] = "首字母縮略字";
            _items["actor"] = "男演員；行動者";
            _items["actress"] = "女演員";
            _items["acupuncture"] = "針灸";
            _items["acute"] = "急性的；敏銳的；嚴重的";
            _items["adamant"] = "堅決的；固執的";
            _items["adapt"] = "適應；改編";
            _items["addict"] = "成癮者；使上癮";
            _items["address"] = "地址；演說；處理";
            _items["adept"] = "熟練的；高手";
            _items["adequate"] = "足夠的；適當的";
            _items["adhere"] = "黏附；遵守；堅持";
            _items["adherent"] = "追隨者；支持者";
            _items["adjacent"] = "鄰近的；毗鄰的";
            _items["adjust"] = "調整；適應";
            _items["administer"] = "管理；施行；給予";
            _items["administration"] = "行政；管理；政府部門";
            _items["admire"] = "欣賞；欽佩";
            _items["adolescent"] = "青少年；青春期的";
            _items["adopt"] = "採納；收養";
            _items["adorable"] = "可愛的；討人喜歡的";
            _items["adore"] = "崇拜；非常喜愛";
            _items["adorn"] = "裝飾；點綴";
            _items["adultery"] = "通姦";
            _items["advent"] = "到來；出現";
            _items["adventure"] = "冒險；奇遇";
            _items["adversary"] = "對手；敵手";
            _items["adverse"] = "不利的；相反的";
            _items["adversity"] = "逆境；苦難";
            _items["advocate"] = "提倡；擁護者";
            _items["aegis"] = "保護；庇護";
            _items["aero-"] = "空氣的；航空的";
            _items["aerobic"] = "有氧的";
            _items["aesthetic"] = "審美的；美學的";
            _items["affable"] = "友善的；和藹的";
            _items["affair"] = "事件；事務；外遇";
            _items["affect"] = "影響；假裝";
            _items["affiliate"] = "使隸屬；分支機構";
            _items["afflict"] = "使痛苦；折磨";
            _items["affluent"] = "富裕的；富足的";
            _items["afford"] = "負擔得起；提供";
            _items["agenda"] = "議程；待辦事項";
            _items["agent"] = "代理人；媒介";
            _items["aggravate"] = "加重；惡化；激怒";
            _items["aggressive"] = "侵略性的；積極進取的";
            _items["agile"] = "敏捷的；靈活的";
            _items["agony"] = "劇痛；苦惱";
            _items["aisle"] = "走道；通道";
            _items["albeit"] = "雖然；儘管";
            _items["albino"] = "白化者；白化動物";
            _items["album"] = "相簿；專輯";
            _items["alert"] = "警覺的；警報";
            _items["alien"] = "外國的；外星人";
            _items["alienate"] = "使疏遠；離間";
            _items["alimentary canal"] = "消化道";
            _items["alimony"] = "贍養費";
            _items["A-list"] = "一線名單；最受歡迎者";
            _items["allergen"] = "過敏原";
            _items["allergic"] = "過敏的；厭惡的";
            _items["allergy"] = "過敏；反感";
            _items["allied"] = "結盟的；相關的";
            _items["alligator"] = "短吻鱷";
            _items["alliteration"] = "頭韻";
            _items["allocate"] = "分配；撥給";
            _items["allowance"] = "津貼；零用錢；允許量";
            _items["allude"] = "暗指；影射";
            _items["allusion"] = "典故；暗示";
            _items["ally"] = "盟友；結盟";
            _items["almighty"] = "全能的；萬能的";
            _items["alpaca"] = "羊駝；羊駝毛";
            _items["alphabet"] = "字母表";
            _items["also-ran"] = "落選者；失敗者";
            _items["alter"] = "改變；修改";
            _items["alternative"] = "替代方案；另類的";
            _items["altitude"] = "高度；海拔";
            _items["altruism"] = "利他主義";
            _items["aluminium"] = "鋁";
            _items["alumni"] = "校友";
            _items["alumnus"] = "校友；男校友";
            _items["amateur"] = "業餘者；外行";
            _items["amaze"] = "使驚訝";
            _items["amazing"] = "令人驚奇的";
            _items["Amazon"] = "亞馬遜；女戰士";
            _items["ambassador"] = "大使；代表";
            _items["ambiguous"] = "模稜兩可的；含糊的";
            _items["ambition"] = "野心；抱負";
            _items["ambivalent"] = "矛盾的；搖擺不定的";
            _items["amble"] = "漫步";
            _items["ambulance"] = "救護車";
            _items["ambush"] = "埋伏；伏擊";
            _items["amenity"] = "便利設施；舒適";
            _items["amiable"] = "親切的；和藹的";
            _items["amicable"] = "友好的；和平的";
            _items["amnesia"] = "失憶症";
            _items["amnesty"] = "大赦；赦免";
            _items["amphibian"] = "兩棲動物";
            _items["ample"] = "充足的；寬敞的";
            _items["amplify"] = "放大；增強";
            _items["amuse"] = "逗樂；使娛樂";
            _items["amusement park"] = "遊樂園";
            _items["anagram"] = "變位詞";
            _items["analogy"] = "類比；類推";
            _items["anarchy"] = "無政府狀態；混亂";
            _items["anatomy"] = "解剖學；身體構造";
            _items["ancestor"] = "祖先";
            _items["anchor"] = "錨；主播；固定";
            _items["anecdote"] = "軼事；趣聞";
            _items["anger"] = "憤怒";
            _items["angry"] = "生氣的";
            _items["anguish"] = "極度痛苦";
            _items["annex"] = "附加；併吞；附屬建築";
            _items["annihilate"] = "消滅；殲滅";
            _items["anniversary"] = "週年紀念日";
            _items["announce"] = "宣布";
            _items["announcement"] = "公告；宣布";
            _items["annual"] = "每年的；年刊";
            _items["antagonist"] = "對手；反派；拮抗物";
            _items["Antarctic"] = "南極的；南極地區";
            _items["ante-"] = "之前的；先於";
            _items["antecedent"] = "前情；先行詞；前身";
            _items["antelope"] = "羚羊";
            _items["anthem"] = "國歌；頌歌";
            _items["anthropo-"] = "人類的";
            _items["anthropoid"] = "類人猿；像人的";
            _items["anthropology"] = "人類學";
            _items["antibiotic"] = "抗生素";
            _items["antibody"] = "抗體";
            _items["anticipate"] = "預期；期待；先發制人";
            _items["antique"] = "古董；古老的";
            _items["antonym"] = "反義詞";
            _items["anus"] = "肛門";
            _items["aorta"] = "主動脈";
            _items["ape"] = "猿；模仿";
            _items["apparatus"] = "設備；器械";
            _items["apparel"] = "服裝；衣著";
            _items["apparent"] = "明顯的；表面上的";
            _items["appeal"] = "呼籲；吸引力；上訴";
            _items["appealing"] = "有吸引力的";
            _items["appendix"] = "附錄；闌尾";
            _items["appetite"] = "食慾；慾望";
            _items["appetizer"] = "開胃菜";
            _items["applaud"] = "鼓掌；稱讚";
            _items["appliance"] = "器具；家電";
            _items["appoint"] = "任命；約定";
            _items["appointment"] = "約會；任命；預約";
            _items["appreciate"] = "欣賞；感激；增值";
            _items["apprentice"] = "學徒";
            _items["appropriate"] = "適當的；挪用";
            _items["approximate"] = "近似的；大約";
            _items["apt"] = "易於的；適當的；聰明的";
            _items["aptitude"] = "天資；能力傾向";
            _items["aquarium"] = "水族館；水族箱";
            _items["archaeology"] = "考古學";
            _items["archaic"] = "古老的；過時的";
            _items["archive"] = "檔案；封存";
            _items["ardent"] = "熱情的；熱烈的";
            _items["arduous"] = "艱鉅的；費力的";
            _items["area"] = "區域；面積";
            _items["arena"] = "競技場；舞臺";
            _items["arid"] = "乾旱的；枯燥的";
            _items["aristocracy"] = "貴族階級";
            _items["aristocrat"] = "貴族";
            _items["armada"] = "艦隊";
            _items["Armageddon"] = "世界末日大戰；大災難";
            _items["armistice"] = "停戰協定";
            _items["aroma"] = "香氣";
            _items["array"] = "排列；大量；陣列";
            _items["arrest"] = "逮捕；阻止；吸引";
            _items["arrogant"] = "傲慢的";
            _items["arson"] = "縱火";
            _items["artery"] = "動脈；交通幹線";
            _items["arthritis"] = "關節炎";
            _items["ascend"] = "上升；攀登";
            _items["ascribe"] = "歸因於";
            _items["asparagus"] = "蘆筍";
            _items["aspect"] = "方面；面向；外觀";
            _items["aspire"] = "渴望；有志於";
            _items["assail"] = "攻擊；抨擊";
            _items["assassin"] = "刺客；暗殺者";
            _items["assassinate"] = "暗殺";
            _items["assault"] = "攻擊；侵犯";
            _items["assemble"] = "集合；組裝";
            _items["assembly"] = "集會；組裝；議會";
            _items["assembly line"] = "生產線；裝配線";
            _items["assent"] = "同意；贊成";
            _items["assert"] = "斷言；主張";
            _items["assess"] = "評估；估價";
            _items["asset"] = "資產；優點";
            _items["asshole"] = "混蛋；討厭的人";
            _items["assiduous"] = "勤勉的；刻苦的";
            _items["assimilate"] = "吸收；同化";
            _items["assist"] = "協助";
            _items["assistant"] = "助手；助理";
            _items["associate"] = "聯想；交往；夥伴";
            _items["association"] = "協會；關聯";
            _items["assorted"] = "各式各樣的";
            _items["assume"] = "假設；承擔；呈現";
            _items["asteroid"] = "小行星";
            _items["asthma"] = "氣喘";
            _items["astonish"] = "使驚訝";
            _items["astound"] = "使震驚";
            _items["astro-"] = "星的；太空的";
            _items["astrology"] = "占星術";
            _items["astronaut"] = "太空人";
            _items["astronomy"] = "天文學";
            _items["astute"] = "精明的；敏銳的";
            _items["asylum"] = "庇護；收容所";
            _items["atheism"] = "無神論";
            _items["atmosphere"] = "氣氛；大氣";
            _items["atom"] = "原子";
            _items["attach"] = "附上；繫上";
            _items["attachment"] = "附件；依附；情感依戀";
            _items["attain"] = "達到；獲得";
            _items["attend"] = "出席；照料";
            _items["attendant"] = "隨從；服務員；伴隨的";
            _items["attenuate"] = "使變弱；使稀薄";
            _items["attest"] = "證明；證實";
            _items["attribute"] = "屬性；歸因於";
            _items["auction"] = "拍賣";
            _items["audacious"] = "大膽的；魯莽的";
            _items["audio-"] = "聲音的；音訊的";
            _items["audit"] = "審計；查核";
            _items["auditorium"] = "禮堂；觀眾席";
            _items["augment"] = "增加；增強";
            _items["augur"] = "預示；占卜者";
            _items["august"] = "威嚴的；令人敬畏的";
            _items["August"] = "八月";
            _items["aura"] = "氣氛；光環";
            _items["auroraborealis"] = "北極光";
            _items["auspicious"] = "吉利的；有利的";
            _items["authentic"] = "真實的；正宗的";
            _items["author"] = "作者";
            _items["authority"] = "權威；權力；當局";
            _items["authorize"] = "授權；批准";
            _items["autism"] = "自閉症";
            _items["autography"] = "親筆書寫；手稿";
            _items["autonomy"] = "自治；自主權";
            _items["autumn"] = "秋天";
            _items["avid"] = "熱切的；渴望的";
            _items["award"] = "獎項；授予";
            _items["awe"] = "敬畏";
            _items["awesome"] = "很棒的；令人敬畏的";
            _items["awful"] = "糟糕的；可怕的";
            _items["awkward"] = "尷尬的；笨拙的";
            _items["axe"] = "斧頭；裁撤";
            return _items;
        }
    }

    public class frmEditWord : Form
    {
        public WordItem Word { get; set; }

        private TextBox txtWord;
        private TextBox txtPhonogram;
        private TextBox txtSoundPath;
        private TextBox txtExplain;

        public frmEditWord(WordItem word)
        {
            Word = word;
            BuildInterface();
            txtWord.Text = word.Word;
            txtPhonogram.Text = word.Phonogram;
            txtSoundPath.Text = word.SoundPath;
            txtExplain.Text = word.Explain;
        }

        private void BuildInterface()
        {
            Text = "編輯單字";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(560, 540);
            MinimumSize = new Size(560, 540);
            Font = new Font("Microsoft JhengHei UI", 10F);
            BackColor = Color.FromArgb(242, 246, 252);

            Label title = new Label();
            title.Text = "單字資料";
            title.Font = new Font("Microsoft JhengHei UI", 20F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(73, 82, 255);
            title.Location = new Point(28, 22);
            title.AutoSize = true;

            Label lblWord = MakeLabel("單字", 30, 84);
            txtWord = MakeTextBox(30, 108, 494);

            Label lblPhonogram = MakeLabel("音標", 30, 148);
            txtPhonogram = MakeTextBox(30, 172, 494);

            Label lblSound = MakeLabel("音檔路徑", 30, 212);
            txtSoundPath = MakeTextBox(30, 236, 340);

            Button btnBrowse = MakeButton("瀏覽", 70, 34);
            btnBrowse.Location = new Point(380, 234);
            btnBrowse.Click += delegate { BrowseSound(); };

            Button btnAuto = MakeButton("自動", 70, 34);
            btnAuto.Location = new Point(454, 234);
            btnAuto.Click += delegate { AutoSoundPath(); };

            Label lblExplain = MakeLabel("解釋", 30, 280);
            txtExplain = new TextBox();
            txtExplain.Location = new Point(30, 304);
            txtExplain.Size = new Size(494, 145);
            txtExplain.Multiline = true;
            txtExplain.ScrollBars = ScrollBars.Vertical;
            txtExplain.Font = new Font("Microsoft JhengHei UI", 11F);

            Button btnSave = MakeButton("儲存", 100, 40);
            btnSave.Location = new Point(314, 476);
            btnSave.Click += delegate { SaveAndClose(); };

            Button btnCancel = MakeButton("取消", 100, 40);
            btnCancel.Location = new Point(424, 476);
            btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(title);
            Controls.Add(lblWord);
            Controls.Add(txtWord);
            Controls.Add(lblPhonogram);
            Controls.Add(txtPhonogram);
            Controls.Add(lblSound);
            Controls.Add(txtSoundPath);
            Controls.Add(btnBrowse);
            Controls.Add(btnAuto);
            Controls.Add(lblExplain);
            Controls.Add(txtExplain);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
        }

        private Label MakeLabel(string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.AutoSize = true;
            lbl.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(72, 82, 105);
            return lbl;
        }

        private TextBox MakeTextBox(int x, int y, int width)
        {
            TextBox tb = new TextBox();
            tb.Location = new Point(x, y);
            tb.Width = width;
            tb.Font = new Font("Microsoft JhengHei UI", 11F);
            return tb;
        }

        private Button MakeButton(string text, int width, int height)
        {
            Button b = new Button();
            b.Text = text;
            b.Size = new Size(width, height);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Color.FromArgb(232, 237, 255);
            b.ForeColor = Color.FromArgb(73, 82, 255);
            b.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            return b;
        }

        private void BrowseSound()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "音檔|*.mp3;*.wav;*.wma;*.m4a;*.aac;*.mid;*.midi|所有檔案|*.*";
            if (dlg.ShowDialog(this) == DialogResult.OK) txtSoundPath.Text = dlg.FileName;
        }

        private void AutoSoundPath()
        {
            string word = txtWord.Text.Trim();
            if (word.Length == 0) return;

            foreach (char c in Path.GetInvalidFileNameChars()) word = word.Replace(c.ToString(), "");

            string letter = char.IsLetter(word[0]) ? char.ToUpper(word[0]).ToString() : "A";
            txtSoundPath.Text = Path.Combine("Sound", letter, word + ".mp3");
        }

        private void SaveAndClose()
        {
            Word.Word = txtWord.Text.Trim();
            Word.Phonogram = txtPhonogram.Text.Trim();
            Word.SoundPath = txtSoundPath.Text.Trim();
            Word.Explain = txtExplain.Text.Trim();
            DialogResult = DialogResult.Yes;
            Close();
        }
    }

    public class AudioEngine
    {
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern int mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern bool mciGetErrorString(int code, StringBuilder text, int size);

        private string _alias = "";
        private SoundPlayer _soundPlayer;
        private dynamic _wmp;

        public bool Play(string path, out string error)
        {
            error = "";
            Stop();

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                error = "找不到音檔";
                return false;
            }

            string mciError;
            if (TryMci(path, out mciError)) return true;

            string wmpError;
            if (TryWmp(path, out wmpError)) return true;

            string wavError;
            if (TryWav(path, out wavError)) return true;

            error = string.Join("；", new string[] { mciError, wmpError, wavError }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
            return false;
        }

        public void Stop()
        {
            try
            {
                if (!string.IsNullOrEmpty(_alias))
                {
                    mciSendString("stop " + _alias, null, 0, IntPtr.Zero);
                    mciSendString("close " + _alias, null, 0, IntPtr.Zero);
                    _alias = "";
                }
            }
            catch { }

            try
            {
                if (_soundPlayer != null) _soundPlayer.Stop();
            }
            catch { }

            try
            {
                if (_wmp != null) _wmp.controls.stop();
            }
            catch { }
        }

        private bool TryMci(string path, out string error)
        {
            error = "";
            string alias = "wc" + DateTime.Now.Ticks.ToString();
            string clean = path.Replace("\"", "");
            string ext = Path.GetExtension(path).ToLowerInvariant();

            List<string> commands = new List<string>();
            if (ext == ".wav") commands.Add("open \"" + clean + "\" type waveaudio alias " + alias);
            if (ext == ".mp3" || ext == ".wma" || ext == ".m4a" || ext == ".aac") commands.Add("open \"" + clean + "\" type mpegvideo alias " + alias);
            commands.Add("open \"" + clean + "\" alias " + alias);

            List<string> errors = new List<string>();

            foreach (string cmd in commands)
            {
                int open = mciSendString(cmd, null, 0, IntPtr.Zero);
                if (open == 0)
                {
                    int play = mciSendString("play " + alias + " from 0", null, 0, IntPtr.Zero);
                    if (play == 0)
                    {
                        _alias = alias;
                        return true;
                    }
                    errors.Add(MciError(play));
                    mciSendString("close " + alias, null, 0, IntPtr.Zero);
                }
                else
                {
                    errors.Add(MciError(open));
                }
            }

            error = "MCI：" + string.Join("；", errors.Distinct().ToArray());
            return false;
        }

        private bool TryWmp(string path, out string error)
        {
            error = "";
            try
            {
                if (_wmp == null)
                {
                    Type type = Type.GetTypeFromProgID("WMPlayer.OCX");
                    if (type == null)
                    {
                        error = "找不到 Windows Media Player 元件";
                        return false;
                    }
                    _wmp = Activator.CreateInstance(type);
                }

                _wmp.URL = path;
                _wmp.settings.autoStart = true;
                _wmp.settings.mute = false;
                _wmp.controls.play();
                return true;
            }
            catch (Exception ex)
            {
                error = "WMP：" + ex.Message;
                return false;
            }
        }

        private bool TryWav(string path, out string error)
        {
            error = "";
            try
            {
                if (!string.Equals(Path.GetExtension(path), ".wav", StringComparison.OrdinalIgnoreCase))
                {
                    error = "SoundPlayer 只支援 WAV";
                    return false;
                }

                _soundPlayer = new SoundPlayer(path);
                _soundPlayer.Play();
                return true;
            }
            catch (Exception ex)
            {
                error = "WAV：" + ex.Message;
                return false;
            }
        }

        private string MciError(int code)
        {
            try
            {
                StringBuilder sb = new StringBuilder(512);
                if (mciGetErrorString(code, sb, sb.Capacity)) return sb.ToString();
            }
            catch { }

            return "MCI 錯誤 " + code;
        }
    }
}
