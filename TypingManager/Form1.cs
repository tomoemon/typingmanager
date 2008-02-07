using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace TypingManager
{
    public partial class Form1 : Form
    {
        private KeyboardHook keyHook;
        private StrokeNumLog strokeNumLog;
        private StrokeTimeLog strokeTimeLog;
        private KeyEventView keyEventView;
        private KeyStrokeView keyStrokeView;
        private TypingSpeed typingSpeed;
        private GraphChanger graphChanger;
        private DetailLogViewer detailLogViewer;
        private TimerTaskController timerTaskController;

        /// <summary>
        /// 日付別打鍵リストビューとプロセス別打鍵リストビューの
        /// いちばん右の項目から見て右側の余白部分の幅
        /// 項目数が増えて縦のスクロールバーが現れると幅が狭くなって
        /// しまい，横のスクロールバーが出てくる．これを防げる値
        /// </summary>
        private const int LISTVIEW_RMARGIN = 25;
        private const int LISTVEW_SMALL_ICON_SIZE = 16;

        #region プロパティ...
        public TextBox LastEventType
        {
            get { return textBox1; }
        }
        public TextBox LastKeyCode
        {
            get { return textBox2; }
        }
        public TextBox LastScanCode
        {
            get { return textBox3; }
        }
        public TextBox LastAppPath
        {
            get { return textBox4; }
        }
        public TextBox TodayStrokeNum
        {
            get { return textBox5; }
        }
        public TextBox TodayAppNum
        {
            get { return textBox6; }
        }
        public TextBox YesterdayStrokeNum
        {
            get { return textBox7; }
        }
        public TextBox YesterdayAppNum
        {
            get { return textBox8; }
        }
        public TextBox TotalNum
        {
            get { return textBox9; }
        }
        public TextBox TotalAppNum
        {
            get { return textBox10; }
        }
        public TextBox TypeSpeedText
        {
            get { return textBox11; }
        }
        public TextBox HibetuTotalType
        {
            get { return textBox14; }
        }
        public TextBox HibetuAvgType
        {
            get { return textBox15; }
        }
        public TextBox HibetuSelectNum
        {
            get { return textBox16; }
        }
        public TextBox HibetuMaxType
        {
            get { return textBox17; }
        }
        public TextBox HibetuMinType
        {
            get { return textBox18; }
        }
        public PictureBox TypeSpeedPicture
        {
            get { return pictureBox1; }
        }
        public GroupBox HistoryGraphName
        {
            get { return groupBox2; }
        }
        public PictureBox HistoryPicture
        {
            get { return pictureBox2; }
        }
        public TextBox HistoryMaxValue
        {
            get { return textBox13; }
        }
        public TextBox HistoryMinValue
        {
            get { return textBox12; }
        }
        public ComboBox CommentBox
        {
            get { return comboBox1; }
        }
        public bool Logging
        {
            get { return strokeTimeLog.Logging; }
        }
        public ToolStripMenuItem IsTopMostMenu
        {
            get { return 常に手前に表示するToolStripMenuItem; }
        }
        public ToolStripMenuItem IsSaveTitleStroke
        {
            get { return ウィンドウタイトル別の打鍵数を保存TToolStripMenuItem; }
        }
        public ToolStripMenuItem GraphMarkMenu
        {
            get { return 履歴グラフのマークMToolStripMenuItem; }
        }
        public ToolStripStatusLabel DateTimeStatus
        {
            get { return toolStripStatusLabel1; }
        }
        public ToolStripStatusLabel TotalLogDayNum
        {
            get { return toolStripStatusLabel2; }
        }
        public ListView ProcessStrokeView
        {
            get { return listView1; }
        }
        public ListView DayStrokeView
        {
            get { return listView2; }
        }
        public ListView DetailLogView
        {
            get { return listView4; }
        }
        public ListView DetailLogSelectView
        {
            get { return listView3; }
        }
        public RadioButton DateClassifyButton
        {
            get { return radioButton1; }
        }
        public RadioButton TagClassifyButton
        {
            get { return radioButton2; }
        }
        public RadioButton ProcessViewTypeToday
        {
            get { return radioButton3; }
        }
        public RadioButton ProcessViewTypeAll
        {
            get { return radioButton4; }
        }
        #endregion

        public Form1()
        {
            InitializeComponent();

            // 起動時のウィンドウの状態は設定で変えられるようにしよう
            //this.WindowState = FormWindowState.Minimized;
            //this.ShowInTaskbar = false;

            // ログ保存用のディレクトリの存在チェックと生成
            LogDir.LogDirectoryCheck();

            // アプリケーションの設定ファイル読み込み
            AppConfig.Load();
            IsTopMostMenu.Checked = AppConfig.TopMost;
            if (AppConfig.TabIndex >= tabControl1.TabCount)
            {
                AppConfig.TabIndex = 0;
            }
            tabControl1.TabIndex = AppConfig.TabIndex;
            detailLogViewer = new DetailLogViewer();
            this.Text = Properties.Resources.ApplicationName;

            // 各リストビューの幅調整など
            ProcessStrokeView.SmallImageList = new ImageList();
            ProcessStrokeView.SmallImageList.ImageSize = new Size(LISTVEW_SMALL_ICON_SIZE, LISTVEW_SMALL_ICON_SIZE);
            ProcessStrokeView.Columns[2].Width = ProcessStrokeView.Width -
                ProcessStrokeView.Columns[1].Width - ProcessStrokeView.Columns[0].Width - LISTVIEW_RMARGIN;
            ProcessStrokeView.ListViewItemSorter = new NumSort(2);
            DayStrokeView.SmallImageList = new ImageList();
            DayStrokeView.SmallImageList.ImageSize = new Size(1, LISTVEW_SMALL_ICON_SIZE);
            DayStrokeView.Columns[2].Width = DayStrokeView.Width - DayStrokeView.Columns[0].Width - 
                DayStrokeView.Columns[1].Width - LISTVIEW_RMARGIN;
            DayStrokeView.ListViewItemSorter = new NumSort(1,2);
            DetailLogSelectView.SmallImageList = new ImageList();
            DetailLogSelectView.SmallImageList.ImageSize = new Size(1, LISTVEW_SMALL_ICON_SIZE);
            DetailLogSelectView.ListViewItemSorter = new NumSort(1);
            DetailLogSelectView.Columns[1].Width = DetailLogSelectView.Width -
                DetailLogSelectView.Columns[0].Width - LISTVIEW_RMARGIN;
            DetailLogView.SmallImageList = new ImageList();
            DetailLogView.SmallImageList.ImageSize = new Size(1, LISTVEW_SMALL_ICON_SIZE);
            DetailLogView.ListViewItemSorter = new NumSort();
            DetailLogView.ShowItemToolTips = true;
            DetailLogView.Columns[2].Width = DetailLogView.Width - DetailLogView.Columns[0].Width -
                DetailLogView.Columns[1].Width - LISTVIEW_RMARGIN;
            
            // ステータスバーに現在時刻を反映
            DateTimeStatus.Text = DateTime.Now.ToString();

            // キーボードフックの開始
            keyHook = new KeyboardHook();
            keyHook.KeyboardHooked += new KeyboardHookedEventHandler(keyHookProc);

            // 打鍵数カウントを担当するクラス
            strokeNumLog = new StrokeNumLog();

            // 詳細ロギングを担当するクラス
            strokeTimeLog = new StrokeTimeLog();            
            
            // 詳細ログの過去のコメントを取得
            LoadCommentHistory();

            // 打鍵速度を計算するクラスを作成（サンプル数20）
            typingSpeed = new TypingSpeed(20);

            // キー入力に応じてGUIの値を更新するクラス
            keyEventView = new KeyEventView(this, strokeTimeLog);
            keyStrokeView = new KeyStrokeView(this, strokeNumLog);

            // 読み込んだAppConfigからGUIに反映させる
            SetProcessViewType();       // プロセス別打鍵数の表示タイプのチェック
            SetSaveTitleStrokeMenu();   // タイトル別の打鍵数を保存するかのチェック
            SetGraphMarkCheck(AppConfig.MarkType); // グラフにつけるマーク

            // NotifyIcon（タスクトレイアイコン）のテキストを変更する
            notifyIcon1.Text = string.Format("今日:{0}{1}昨日:{2}{3}合計:{4}",
                strokeNumLog.TodayTotalType, Environment.NewLine, 
                strokeNumLog.YesterdayTotalType, Environment.NewLine,
                strokeNumLog.TotalType);

            // 打鍵ログをGUIに反映させる
            keyStrokeView.DayStrokeViewLoad();
            keyStrokeView.ProcessViewLoad();
            keyStrokeView.MainTabLoad();

            // 履歴グラフに関する設定
            graphChanger = new GraphChanger(HistoryPicture, HistoryMaxValue, HistoryMinValue,
                TypeSpeedPicture, TypeSpeedText);
            graphChanger.SetMargin(HistoryMaxValue.Width+3, 1, 5, 3);
            graphChanger.SetGraph(LineGraphType.StrokeNumPerMinute, "打鍵数の履歴(1分ごと)", 60, 200);
            graphChanger.SetGraph(LineGraphType.StrokeNumPerHour, "打鍵数の履歴(1時間ごと)", 24, 5000);
            graphChanger.SetGraph(LineGraphType.StrokeNumPerDay, "打鍵数の履歴(1日ごと)", 30, 5000);
            graphChanger.SetGraph(LineGraphType.TypeSpeedPerStroke, "打鍵速度の履歴（打/分）", 60, 200);
            graphChanger.SetMarkSize(LineGraphMarkType.Plus, 4);
            graphChanger.SetMarkSize(LineGraphMarkType.Square, 3);
            graphChanger.SetMarkSize(LineGraphMarkType.VerticalBar, 4);
            graphChanger.SetMarkSize(LineGraphMarkType.HorizonBar, 4);
            graphChanger.SetDataSource(strokeNumLog, typingSpeed);

            // 履歴グラフにつけるマークの変更
            graphChanger.SetGraphMark(AppConfig.MarkType);

            // 履歴グラフに値の設定
            graphChanger.SetGraphValue();

            // 履歴グラフ名の更新
            HistoryGraphName.Text = graphChanger[AppConfig.LineGraphType].GraphName;

            // Timerを使用するタスクの登録
            timerTaskController = new TimerTaskController();
            timerTaskController.AddTask(strokeNumLog, StrokeNumLog.TIMER_ID_NEWDAY, 1, 0, 0);
            timerTaskController.AddTask(strokeNumLog, StrokeNumLog.TIMER_ID_SAVE, 0, 10, 0);
            timerTaskController.AddTask(graphChanger, GraphChanger.TIMER_ID_UPDATE, 0, 0, 1);
        }

        /// <summary>
        /// キーの上げ下げが発生したときに呼び出されるフックプロシージャ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void keyHookProc(object sender, KeyboardHookedEventArgs e)
        {
            int now = QueryTime.NowMiliSec;
            string app_path = ProcessWindowName.GetFrontProcessName();
            string app_title = ProcessWindowName.GetFrontWindowTitle();

            if (e.UpDown == KeyboardUpDown.Down)
            {
                //Debug.WriteLine(e.KeyCode.ToString("d") + ":" + e.ScanCode.ToString("d") + ":down");
                strokeTimeLog.KeyDown((int)e.KeyCode, e.ScanCode, now);
            }
            else
            {
                //Debug.WriteLine(QueryTime.Now);
                //Debug.WriteLine(e.KeyCode.ToString("d") + ":" + e.ScanCode.ToString("d") + ":up");
                strokeTimeLog.KeyUp((int)e.KeyCode, e.ScanCode, now);
                if (AppConfig.SaveTitleStroke)
                {
                    strokeNumLog.KeyStroke(app_path, app_title);
                }
                else
                {
                    strokeNumLog.KeyStroke(app_path, "");
                }
                typingSpeed.Stroke(now);
            }
            notifyIcon1.Text = string.Format("今日:{0}{1}昨日:{2}{3}合計:{4}",
                strokeNumLog.TodayTotalType, Environment.NewLine,
                strokeNumLog.YesterdayTotalType, Environment.NewLine,
                strokeNumLog.TotalType);
            keyEventView.Update(e.UpDown, (int)e.KeyCode, e.ScanCode, app_path);
        }

        /// <summary>
        /// 終了時に呼び出される
        /// </summary>
        public void AppExit()
        {
            Application.Exit();
        }

        /// <summary>
        /// 終了時のログ保存など
        /// </summary>
        /// <returns></returns>
        public bool AppFinalize()
        {
            /*
            DialogResult res = MessageBox.Show("終了しますか？", "終了確認", MessageBoxButtons.OKCancel);
            if (res == DialogResult.Cancel)
            {
                return false;
            }
             * */
            AppConfig.TabIndex = tabControl1.SelectedIndex;
            AppConfig.Save();
            strokeTimeLog.LoggingEnd();
            strokeNumLog.Save();
            SaveCommentHistory();
            return true;
        }

        /// <summary>
        /// 詳細ログにつけたコメントの履歴を保存する
        /// </summary>
        private void SaveCommentHistory()
        {
            using (StreamWriter sw = new StreamWriter(LogDir.COMMENT_FILE))
            {
                foreach (string text in CommentBox.Items)
                {
                    sw.WriteLine(text);
                }
            }
        }

        /// <summary>
        /// 詳細ログにつけるコメントの履歴ファイルを読み込む
        /// </summary>
        private void LoadCommentHistory()
        {
            if (File.Exists(LogDir.COMMENT_FILE))
            {
                using (StreamReader sr = new StreamReader(LogDir.COMMENT_FILE))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null) // 1行ずつ読み出し。
                    {
                        CommentBox.Items.Add(line);
                    }
                }
            }
        }

        private void 終了ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ExitするときにForm1_FormClosingが自動的に呼ばれる
            // FormClosingの中で終了時の処理を行う
            AppExit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Debug.WriteLine("Form1_FormClosed");
            if (!AppFinalize())
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// デフォルトで1秒ごとにタイマーから呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            timerTaskController.CallTask(DateTime.Now);
            DateTimeStatus.Text = DateTime.Now.ToString();
        }

        /// <summary>
        /// 「メイン」タブの詳細ログの取得開始ボタンが押されたときに呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            strokeTimeLog.LoggingStart(comboBox1.Text);
            if (comboBox1.Items.IndexOf(comboBox1.Text) == -1)
            {
                comboBox1.Items.Add(comboBox1.Text);
                if (comboBox1.Items.Count > 10)
                {
                    comboBox1.Items.RemoveAt(0);
                }
            }
            button1.Enabled = false;
            button2.Enabled = true;
        }

        /// <summary>
        /// 詳細ログの取得終了ボタンが押されたときに呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            strokeTimeLog.LoggingEnd();
            button1.Enabled = true;
            button2.Enabled = false;
        }

        /// <summary>
        /// 打鍵速度の瞬間値グラフの描画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            graphChanger.DrawGauge(e.Graphics);
        }

        /// <summary>
        /// 「メイン」タブの履歴グラフをクリックされたときに呼び出される
        /// 表示するグラフの種類を変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Array type_array = Enum.GetValues(typeof(LineGraphType));
            int next = (int)AppConfig.LineGraphType + 1;
            next = next >= type_array.Length ? 0 : next;
            AppConfig.LineGraphType = (LineGraphType)type_array.GetValue(next);
            
            HistoryGraphName.Text = graphChanger[AppConfig.LineGraphType].GraphName;
            graphChanger.SetGraphValue();
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            graphChanger[AppConfig.LineGraphType].Draw(e.Graphics);
        }

        private void 常に手前に表示するToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            AppConfig.TopMost = 常に手前に表示するToolStripMenuItem.Checked;
            this.TopMost = AppConfig.TopMost;
        }

        #region 線グラフにつけるマークの変更
        private void なしToolStripMenuItem_Click(object sender, EventArgs e)
        {
            graphChanger.SetGraphMark(LineGraphMarkType.None);
            AppConfig.MarkType = LineGraphMarkType.None;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            graphChanger.SetGraphMark(LineGraphMarkType.Plus);
            AppConfig.MarkType = LineGraphMarkType.Plus;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            graphChanger.SetGraphMark(LineGraphMarkType.Square);
            AppConfig.MarkType = LineGraphMarkType.Square;
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            graphChanger.SetGraphMark(LineGraphMarkType.HorizonBar);
            AppConfig.MarkType = LineGraphMarkType.HorizonBar;
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            graphChanger.SetGraphMark(LineGraphMarkType.VerticalBar);
            AppConfig.MarkType = LineGraphMarkType.VerticalBar;
        }

        private void 履歴グラフのマークMToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (ToolStripMenuItem menu in GraphMarkMenu.DropDownItems)
            {
                menu.Checked = false;
            }
        }

        /// <summary>
        /// 指定したタイプのグラフマークをメニューのチェックに反映させる
        /// </summary>
        /// <param name="type"></param>
        private void SetGraphMarkCheck(LineGraphMarkType type)
        {
            foreach (ToolStripMenuItem menu in GraphMarkMenu.DropDownItems)
            {
                menu.Checked = false;
            }
            if (type == LineGraphMarkType.Plus)
            {
                ToolStripMenuItem item = (ToolStripMenuItem)(GraphMarkMenu.DropDownItems[1]);
                item.Checked = true;
            }
            else if (type == LineGraphMarkType.Square)
            {
                ToolStripMenuItem item = (ToolStripMenuItem)(GraphMarkMenu.DropDownItems[2]);
                item.Checked = true;
            }
            else if (type == LineGraphMarkType.HorizonBar)
            {
                ToolStripMenuItem item = (ToolStripMenuItem)(GraphMarkMenu.DropDownItems[3]);
                item.Checked = true;
            }
            else if (type == LineGraphMarkType.VerticalBar)
            {
                ToolStripMenuItem item = (ToolStripMenuItem)(GraphMarkMenu.DropDownItems[4]);
                item.Checked = true;
            }
        }
        #endregion

        #region 各リストビューのソート
        /// <summary>
        /// プロセス別打鍵数リストビューのソート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView(ProcessStrokeView, e.Column);
        }

        /// <summary>
        /// 日別打鍵数リストビューのソート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView(DayStrokeView, e.Column);
        }

        /// <summary>
        /// 詳細ログの分類リストビューのソート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView3_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView(DetailLogSelectView, e.Column);
        }

        /// <summary>
        /// 詳細ログのログファイルリストビューのソート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView4_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView(DetailLogView, e.Column);
        }

        /// <summary>
        /// リストビューと列を指定してソートする
        /// </summary>
        /// <param name="view"></param>
        /// <param name="column"></param>
        private void SortListView(ListView view, int column)
        {
            NumSort sorter = (NumSort)view.ListViewItemSorter;
            sorter.Column = column;
            view.Sort();
            sorter.ChangeSortOrder();
        }
        #endregion

        /// <summary>
        /// 「詳細ログ」タブの読み込みボタンを押したときに呼び出される
        /// 詳細ログファイルをすべて読み込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Load Button Clicked");
            detailLogViewer.LoadInfo();
            SetDetailLogSelect();
        }

        /// <summary>
        /// 「詳細ログ」タブの表示するタイプを切り換えたときに呼び出される
        /// 「タグ」を選択した場合はすべてのタグを表示する
        /// 「日付」を選択した場合は詳細ログのあるすべての日付を表示する
        /// </summary>
        private void SetDetailLogSelect()
        {
            DetailLogSelectView.Items.Clear();
            if (TagClassifyButton.Checked)
            {
                DetailLogSelectView.Columns[0].Text = "タグ";
                foreach (string tag in detailLogViewer.TagList)
                {
                    DetailLogSelectView.Items.Add(tag, tag, "");
                    DetailLogSelectView.Items[tag].SubItems.Add(detailLogViewer.GetTagSetNum(tag).ToString());
                }
            }
            else
            {
                DetailLogSelectView.Columns[0].Text = "日付";
                foreach (string date in detailLogViewer.DateList)
                {
                    DetailLogSelectView.Items.Add(date, date, "");
                    DetailLogSelectView.Items[date].SubItems.Add(detailLogViewer.GetDateSetNum(date).ToString());
                }
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            SetDetailLogSelect();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            SetDetailLogSelect();
        }

        /// <summary>
        /// 「詳細ログ」タブで日付やタグの選択を変えたときに呼び出される
        /// 右側のリストビューを変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DetailLogSelectView.SelectedItems.Count == 0)
            {
                return;
            }
            ListViewItem item = DetailLogSelectView.SelectedItems[0];
            Debug.WriteLine(item.Text);
            DetailLogView.Items.Clear();

            List<DetailLogInfo> info_list;
            if (TagClassifyButton.Checked)
            {
                info_list = detailLogViewer.GetTagInfo(item.Text);
            }
            else
            {
                info_list = detailLogViewer.GetDateInfo(item.Text);
            }
            foreach (DetailLogInfo info in info_list)
            {
                ListViewItem add_item = new ListViewItem();
                add_item.Name = info.Date.ToString();
                add_item.Text = info.Date.ToString(DetailLogViewer.DATE_FORMAT);
                add_item.Tag = info;
                add_item.SubItems.Add(info.Date.ToString(DetailLogViewer.TIME_FORMAT));
                add_item.SubItems.Add(info.TagConcat("", true) + " " + info.Comment);
                DetailLogView.Items.Add(add_item);
                Debug.WriteLine("###"+DetailLogView.Items[add_item.Name].Text);
                /*
                 * ListViewはItems.Addした瞬間にソートしようとするため，
                 * 一列目以外でソートしようとすると，そのあとに追加する予定のアイテムが
                 * ないため，配列の範囲外と例外が発生する
                 * これを解決するためには上のようにListViewItemを一気に追加すること
                DetailLogView.Items.Add(key, date, "");
                DetailLogView.Items[key].Tag = info;
                DetailLogView.Items[key].SubItems.Add(info.Date.ToString(DetailLogViewer.TIME_FORMAT));
                DetailLogView.Items[key].SubItems.Add(info.TagConcat("", true) + " " + info.Comment);
                */
                Debug.WriteLine(info.Date.ToString() + ":" + info.Comment);
            }
        }

        /// <summary>
        /// 「詳細ログ」タブでCSV出力ボタンを押したときに呼び出される
        /// 選択した詳細ログをCSVファイルへ出力する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            if (DetailLogView.SelectedItems.Count == 0)
            {
                MessageBox.Show(Properties.Resources.NoLogSelect,
                    Properties.Resources.CsvErrMsgTitle);
                return;
            }
            string file_list = "";
            foreach (ListViewItem item in DetailLogView.SelectedItems)
            {
                DetailLogInfo info = (DetailLogInfo)item.Tag;
                StrokeTimeLog log = new StrokeTimeLog();
                log.Load(info.FileName);

                string filename = Path.Combine(LogDir.DETAIL_CSV_DIR,
                    Path.GetFileNameWithoutExtension(info.FileName) + ".csv");
                if (!log.SaveCSV(filename))
                {
                    MessageBox.Show(Properties.Resources.FileSaveFailed,
                        Properties.Resources.CsvErrMsgTitle);
                    return;
                }
                file_list += filename + Environment.NewLine;
            }
            MessageBox.Show(file_list, Properties.Resources.FileOutputTitle);
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            /*
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
            }
             * */
        }

        private void スタートアップに登録ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShortCut.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                "打鍵のtomo 2.0.lnk"), "TypingManager.exe", "打鍵のtomo 2.0へのショートカット");
            MessageBox.Show(Properties.Resources.StartupRegist);
        }

        private void スタートアップを解除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                             "打鍵のtomo 2.0.lnk");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            MessageBox.Show(Properties.Resources.StartupUnregist);
        }

        /// <summary>
        /// 「プロセス別打鍵数」タブで今日のプロセス別打鍵数をクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            AppConfig.ProcessViewType = ProcessStrokeViewType.Today;
            keyStrokeView.ProcessViewLoad();
        }

        /// <summary>
        /// 「プロセス別打鍵数」タブですべてのプロセス別打鍵数をクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            AppConfig.ProcessViewType = ProcessStrokeViewType.All;
            keyStrokeView.ProcessViewLoad();
        }

        #region AppConfigの値をGUIメニューのチェック状態に反映させる
        /// <summary>
        /// AppConfigの値をGUIメニューのチェック状態に反映させる
        /// </summary>
        private void SetProcessViewType()
        {
            if (AppConfig.ProcessViewType == ProcessStrokeViewType.All)
            {
                ProcessViewTypeAll.Checked = true;
                ProcessViewTypeToday.Checked = false;
            }
            else
            {
                ProcessViewTypeToday.Checked = true;
                ProcessViewTypeAll.Checked = false;
            }
        }

        /// <summary>
        /// AppConfigの値をGUIメニューのチェック状態に反映させる
        /// </summary>
        private void SetSaveTitleStrokeMenu()
        {
            if (AppConfig.SaveTitleStroke)
            {
                IsSaveTitleStroke.Checked = true;
            }
            else
            {
                IsSaveTitleStroke.Checked = false;
            }
        }

        /// <summary>
        /// GUIの変更をAppConfigに反映させる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ウィンドウタイトル別の打鍵数を保存TToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsSaveTitleStroke.Checked)
            {
                AppConfig.SaveTitleStroke = true;
            }
            else
            {
                AppConfig.SaveTitleStroke = false;
            }
        }
        #endregion

        /// <summary>
        /// プロセス別打鍵リストビューで項目を右クリックしたときに呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = listView1.HitTest(e.X, e.Y);
            if (info.SubItem != null &&
                AppConfig.ProcessViewType == ProcessStrokeViewType.Today &&
                e.Button == MouseButtons.Right)
            {
                string path = info.Item.Text;
                ContextMenu menu = MakeTitleListContextMenu(strokeNumLog, path);
                menu.Show(listView1, new Point(e.X + 10, e.Y + 20));
                Console.WriteLine(info.Item.Text);
            }
        }

        private ContextMenu MakeTitleListContextMenu(StrokeNumLog num_log, string app_path)
        {
            ContextMenu menu = new ContextMenu();
            string item_path = app_path;
            int app_id = num_log.ProcessName.GetID(item_path);

            if (app_id == -1)
            {
                return menu;
            }

            List<string> descend_title = num_log.AppLog[app_id].GetTitleList();
            List<int> title_total = new List<int>();
            for (int i = 0; i < descend_title.Count; i++)
            {
                // num_log.AppLog[app_id]はapp_idが存在しないとき例外が発生する恐れがある
                title_total.Add(
                    num_log.AppLog[app_id].GetTitleTotal(descend_title[i])
                );
            }
            List<int> order_list = Misc.SortOrder(title_total, true);
            for (int i = 0; i < descend_title.Count; i++)
            {
                menu.MenuItems.Add(string.Format("{0} - [{1}]",
                    title_total[order_list[i]], descend_title[order_list[i]]));
            }
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("クリップボードにコピーする(&C)");

            // 「クリップボードにコピーする」メニューをクリックしたときに
            // 親のメニューを参照するため，Tagに登録しておく
            menu.MenuItems[menu.MenuItems.Count - 1].Tag = menu;
            menu.MenuItems[menu.MenuItems.Count - 1].Click += new EventHandler(Title_Menu_Click);

            return menu;
        }

        /// <summary>
        /// プロセス別打鍵リストビューで表示したメニューの
        /// 「クリップボードにコピーする」をクリックしたときに呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Title_Menu_Click(object sender, EventArgs e)
        {
            MenuItem sender_item = (MenuItem)sender;
            ContextMenu menu = (ContextMenu)sender_item.Tag;
            
            string copy_text = "";
            foreach (MenuItem item in menu.MenuItems)
            {
                if (item.Text.EndsWith("]"))
                {
                    copy_text += item.Text + Environment.NewLine;
                }
            }
            if (copy_text != "")
            {
                // クリップボードに文字列をコピーする
                // アプリケーション終了後もクリップボードに残る
                Clipboard.SetText(copy_text);
            }
        }

        /// <summary>
        /// 日別打鍵数リストビューの選択項目が変更されたときに呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int total = 0;
            int max = 0;
            int min = int.MaxValue;
            int item_num = listView2.SelectedItems.Count;
            float average = 0;

            foreach (ListViewItem item in listView2.SelectedItems)
            {
                int type_num = int.Parse(item.SubItems[2].Text);
                total += type_num;
                if(type_num > max) max = type_num;
                if(type_num < min) min = type_num;
            }
            
            // 選択した項目がない場合でも表示を0に戻すためにreturnしない
            if (item_num == 0)
            {
                min = 0;
                average = 0;
            }
            else
            {
                average = total / item_num;
            }
            HibetuTotalType.Text = total.ToString();
            HibetuMaxType.Text = string.Format("{0}", max);
            HibetuMinType.Text = string.Format("{0}", min);
            HibetuAvgType.Text = string.Format("{0}", average);
            HibetuSelectNum.Text = item_num.ToString();
        }

        /// <summary>
        /// 日別打鍵数リストビューの項目を"右"クリックしたときに呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView2_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = listView2.HitTest(e.X, e.Y);
            if (info.SubItem != null && e.Button == MouseButtons.Right)
            {
                DateTime date = (DateTime)info.Item.Tag;
                StrokeNumLog num_log = strokeNumLog;

                // 当日のデータ以外はログに保存されているものを読み込む
                if (date.ToString(AllDayLog.DAY_FORMAT) != DateTime.Now.ToString(AllDayLog.DAY_FORMAT))
                {
                    // ファイルが存在しなくても大丈夫
                    num_log = new StrokeNumLog(date);
                }

                ContextMenu menu = new ContextMenu();
                List<AppKeyLog> app_list = new List<AppKeyLog>(num_log.AppLog.Values);
                List<int> num_list = new List<int>();
                for(int i=0; i<app_list.Count;i++)
                {
                    num_list.Add(app_list[i].Total);
                    //Console.WriteLine("{0} - [{1}]", app_list[i].AppID, num_list[i]);
                }
                List<int> order_list = Misc.SortOrder(num_list, true);
                
                /*
                Console.WriteLine("並び替えあと");
                for (int i = 0; i < app_list.Count; i++)
                {
                    Console.WriteLine("{0} - {1}",order_list[i], num_list[i]);
                }
                */
                for(int i=0; i<app_list.Count; i++)
                {
                    string app_name = num_log.ProcessName.GetName(
                                        app_list[order_list[i]].AppID);
                    string app_path = num_log.ProcessName.GetPath(
                                        app_list[order_list[i]].AppID);
                    Console.WriteLine("id:{0}, name:{1}, path:{2}",
                        app_list[order_list[i]].AppID,app_name,app_path);
                    int app_total = num_list[order_list[i]];
                    menu.MenuItems.Add(string.Format("{0} - [{1}]",app_total,app_name));
                    ContextMenu sub_menu = MakeTitleListContextMenu(num_log, app_path);
                    
                    if (sub_menu.MenuItems.Count > 2)
                    {
                        MenuItem[] menu_items = new MenuItem[sub_menu.MenuItems.Count];
                        for (int j = 0; j < sub_menu.MenuItems.Count; j++)
                        {
                            menu_items[j] = sub_menu.MenuItems[j].CloneMenu();
                        }
                        
                        //  sub_menu.MenuItems.CopyTo(menu_items,0)をやったあとに
                        // 以下のステップを実行すると，なぜかsub_menuのMenuItemsがからっぽになる
                        // 上のようにClonMenu()を使うと大丈夫．．．なぜだろう？
                        menu.MenuItems[menu.MenuItems.Count - 1].MenuItems.AddRange(menu_items);

                        menu.MenuItems[menu.MenuItems.Count - 1].MenuItems[menu_items.Length - 1].Tag = sub_menu;
                    }
                }
                menu.MenuItems.Add("-");
                menu.MenuItems.Add("クリップボードにコピーする(&C)");
                menu.MenuItems[menu.MenuItems.Count - 1].Tag = menu;
                menu.MenuItems[menu.MenuItems.Count - 1].Click += new EventHandler(Title_Menu_Click);

                menu.Show(listView2, new Point(e.X + 10, e.Y + 20));
            }
        }

        /// <summary>
        /// 日別打鍵数リストビュー内で項目の上をマウスドラッグしたときに呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView2_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = listView2.HitTest(e.X, e.Y);
            if (info.SubItem != null && e.Button == MouseButtons.Left)
            {
                info.Item.Selected = true;
            }
        }

        /// <summary>
        /// プロセス別打鍵数リストビュー内でCtrl+Aを押したときに呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                SelectAllListViewItem(listView1);
            }
        }

        /// <summary>
        /// 日別打鍵数リストビュー内でCtrl+Aを押したときに呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                SelectAllListViewItem(listView2);
            }
        }

        /// <summary>
        /// リストビュー内のアイテムをすべて選択する
        /// </summary>
        /// <param name="view"></param>
        private void SelectAllListViewItem(ListView view)
        {
            foreach (ListViewItem item in view.Items)
            {
                item.Selected = true;
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = listView1.HitTest(e.X, e.Y);
            if (info.SubItem != null && e.Button == MouseButtons.Left)
            {
                ListViewInputBox input = new ListViewInputBox(listView1, info.Item, 1);
                input.FinishInput += new ListViewInputBox.InputEventHandler(input_FinishInput);
                input.Show();
            }
        }

        void input_FinishInput(object sender, ListViewInputBox.InputEventArgs e)
        {
            //Console.WriteLine(e.Path);
            //Console.WriteLine(e.NewName);
            string old_name = strokeNumLog.ProcessName.GetName(e.Path);

            // 名前が変更されたときだけリストビューの書き換えと保存を行う
            if (old_name != e.NewName)
            {
                strokeNumLog.ProcessName.SetName(e.Path, e.NewName);
                strokeNumLog.ProcessName.Save();
                keyStrokeView.ProcessViewNameUpdate(e.Path, e.NewName);
            }
        }
    }

    public class NumSort : System.Collections.IComparer
    {
        private SortOrder sort_order = SortOrder.Ascending;	// ソート順(昇順・降順)
        private int column = 0;	// ソート列
        private List<int> num_column; // 数値で比較する列

        #region プロパティ...
        public SortOrder Order
        {
            get { return sort_order; }
            set { sort_order = value; }
        }
        public int Column
        {
            get { return column; }
            set { column = value; }
        }
        #endregion

        public NumSort(params int[] list)
        {
            num_column = new List<int>(list);
        }
        // 比較結果を返す
        public int Compare(object x, object y)
        {
            int ret = 0;
            // 比較用リストアイテム格納変数
            ListViewItem sx = (ListViewItem)x;
            ListViewItem sy = (ListViewItem)y;

            // 文字列を比較し、値を格納
            if (num_column.Contains(column))
            {
                // この条件をつけておかないとcolumnが範囲外だと怒られてしまうことがある
                // 再現条件：
                // 「詳細ログ」タブの左のリストビューで「数が2以上の項目」を選択し，
                // 右のリストビューで2列目以降のどれかの列についてソートを行った後に
                // 左のリストビューの「数が2以上の項目」を選択しようとすると落ちる．
                // 解決：アイテムの追加の仕方に注意
                if (column < sx.SubItems.Count && column < sy.SubItems.Count)
                {
                    ret = int.Parse(sx.SubItems[column].Text) - int.Parse(sy.SubItems[column].Text);
                }
            }
            else
            {
                if (column < sx.SubItems.Count && column < sy.SubItems.Count)
                {
                    ret = string.Compare(sx.SubItems[column].Text, sy.SubItems[column].Text);
                }
            }

            // 降順のときは結果を逆転
            if (sort_order == SortOrder.Descending)
            {
                ret = -ret;
            }

            //結果を返す
            return ret;
        }

        public void ChangeSortOrder()
        {
            if (Order == SortOrder.Descending)
            {
                Order = SortOrder.Ascending;
            }
            else
            {
                Order = SortOrder.Descending;
            }
        }
    }
}
