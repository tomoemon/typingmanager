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
        private IKeyboardHookBase keyHook;
        private StrokeNumLog strokeNumLog;
        private PluginController pluginController;
        private KeyStrokeView keyStrokeView;
        private KeyState keyState;
        private TypingSpeed typingSpeed;
        private GraphChanger graphChanger;
        private TimerTaskController timerTaskController;
        private ConfigForm configForm = new ConfigForm();
        
        /// <summary>
        /// 日付別打鍵リストビューとプロセス別打鍵リストビューの
        /// いちばん右の項目から見て右側の余白部分の幅
        /// 項目数が増えて縦のスクロールバーが現れると幅が狭くなって
        /// しまい，横のスクロールバーが出てくる．これを防げる値
        /// </summary>
        public const int LISTVIEW_RMARGIN = 25;
        public const int LISTVEW_SMALL_ICON_SIZE = 16;

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
        public RadioButton ProcessViewTypeToday
        {
            get { return radioButton3; }
        }
        public RadioButton ProcessViewTypeAll
        {
            get { return radioButton4; }
        }
        public ToolStripMenuItem PluginMenu
        {
            get { return プラグインPToolStripMenuItem; }
        }
        #endregion

        public Form1()
        {
            InitializeComponent();

            // 起動時のウィンドウの状態は設定で変えられるようにしよう
            //this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            // ログ保存用のディレクトリの存在チェックと生成
            Plugin.LogDir.LogDirectoryCheck();

            // アプリケーションの設定ファイル読み込み
            AppConfig.Load();
            IsTopMostMenu.Checked = AppConfig.TopMost;
            if (AppConfig.TabIndex >= tabControl1.TabCount)
            {
                AppConfig.TabIndex = 0;
            }
            tabControl1.TabIndex = AppConfig.TabIndex;
            this.Text = Properties.Resources.ApplicationName;

            // キーボードフックの開始
            if (AppConfig.HookLowLevel)
            {
                keyHook = new KeyboardHook();
                keyHook.KeyboardHooked += new KeyboardHookedEventHandler(keyHookProc);
            }
            else
            {
                keyHook = new KeyboardProxyHook();
                keyHook.KeyboardHooked += new KeyboardHookedEventHandler(keyHookProc);
            }

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
            
            // ステータスバーに現在時刻を反映
            DateTimeStatus.Text = DateTime.Now.ToString();

            // 打鍵数カウントを担当するクラス
            strokeNumLog = new StrokeNumLog();
            
            // キーの状態を保存するクラス
            keyState = new KeyState();
            
            // プラグインコントローラの作成とプラグインの読み込み
            pluginController = new PluginController(this);
            pluginController.AddStrokePlugin(strokeNumLog);
            pluginController.AddStrokePlugin(strokeNumLog.ProcessName);
            pluginController.Load();
            pluginController.AddMenu(PluginMenu);
            
            // 打鍵速度を計算するクラスを作成（サンプル数20）
            typingSpeed = new TypingSpeed(20);

            // キー入力に応じてGUIの値を更新するクラス
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
            timerTaskController.AddTask(strokeNumLog, StrokeNumLog.TIMER_ID_SAVE, 0, AppConfig.ScheduleTiming, 0);
            timerTaskController.AddTask(graphChanger, GraphChanger.TIMER_ID_UPDATE, 0, 0, 1);
        }

        /// <summary>
        /// キーの上げ下げが発生したときに呼び出されるフックプロシージャ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void keyHookProc(object sender, KeyboardHookedEventArgs e)
        {
            uint now = QueryTime.NowMiliSec;
            string app_path = ProcessWindowName.GetFrontProcessName();
            string app_title = ProcessWindowName.GetFrontWindowTitle();

            if (e.UpDown == KeyboardUpDown.Down)
            {
                keyState.KeyDown((int)e.KeyCode);
                pluginController.KeyDown(keyState, now, app_path, app_title);
                keyState.SetDownState();
                /*
                if (keyStatePlugin.IsPushKey(e.KeyCode))
                {
                    Console.WriteLine((char)e.KeyCode + ":" + e.KeyCode.ToString("d") + ":" + e.ScanCode.ToString("d") + ":push");
                }
                */
                //Console.WriteLine(e.KeyCode.ToString("d") + ":" + e.ScanCode.ToString("d") + ":down");
            }
            else
            {
                keyState.KeyUp((int)e.KeyCode);
                pluginController.KeyUp(keyState, now, app_path, app_title);
                //Debug.WriteLine(QueryTime.Now);
                //Console.WriteLine(e.KeyCode.ToString("d") + ":" + e.ScanCode.ToString("d") + ":up");
                typingSpeed.Stroke(now);
            }
            notifyIcon1.Text = string.Format("今日:{0}{1}昨日:{2}{3}合計:{4}",
                strokeNumLog.TodayTotalType, Environment.NewLine,
                strokeNumLog.YesterdayTotalType, Environment.NewLine,
                strokeNumLog.TotalType);
            UpdateKeyEventInfo(e.UpDown, (int)e.KeyCode, e.ScanCode, app_path);
        }

        private void UpdateKeyEventInfo(KeyboardUpDown updown, int vkey, int scan, string app_path)
        {
            LastEventType.Text = updown.ToString();
            LastKeyCode.Text = vkey.ToString();
            LastScanCode.Text = scan.ToString();
            LastAppPath.Text = Path.GetFileName(app_path);
        }

        /// <summary>
        /// 終了時のログ保存など
        /// アプリケーション側から終了処理を行いたい時は
        /// この関数を直接使わないでthis.Close()を使うこと．
        /// Form1_FormClosingが呼ばれて，そこからさらにAppFinalize()が呼ばれる．
        /// </summary>
        /// <returns></returns>
        private bool AppFinalize()
        {
            if (AppConfig.ShowExitMessage)
            {
                DialogResult res = MessageBox.Show("終了しますか？", "終了確認",
                    MessageBoxButtons.OKCancel);
                if (res == DialogResult.Cancel)
                {
                    return false;
                }
            }
            AppConfig.TabIndex = tabControl1.SelectedIndex;
            AppConfig.Save();
            pluginController.Close();
            keyHook.Dispose();
            return true;
        }

        private void 終了ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // FormClosingの中で終了時の処理を行う
            this.Close();
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
                string format = (string)AppConfig.RightClickCopyFormat.Clone();
                format = format.Replace("%1", title_total[order_list[i]].ToString());
                format = format.Replace("%2", descend_title[order_list[i]]);
                format = format.Replace("\\t", "\t");
                menu.MenuItems.Add(format);
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
            for (int i = 0; i < menu.MenuItems.Count - 2; i++)
            {
                copy_text += menu.MenuItems[i].Text + Environment.NewLine;
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
                if (date.ToString(Plugin.LogDir.DAY_FORMAT) != DateTime.Now.ToString(Plugin.LogDir.DAY_FORMAT))
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

                    string format = (string)AppConfig.RightClickCopyFormat.Clone();
                    format = format.Replace("%1", app_total.ToString());
                    format = format.Replace("%2", app_name);
                    format = format.Replace("\\t", "\t");
                    menu.MenuItems.Add(format);
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
                // 24はリストビューヘッダーの高さ，17はアイテムの高さ
                // 定数にするのもなんかあれだったので．．
                if (info.Item.Position.Y < 24 + 17 && info.Item.Index > 1)
                {
                    listView2.EnsureVisible(info.Item.Index - 1);
                }
                else if (info.Item.Position.Y > listView2.Height - 24 && info.Item.Index < listView2.Items.Count - 1)
                {
                    listView2.EnsureVisible(info.Item.Index);
                }
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
            else if (e.KeyCode == Keys.F2 && listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = listView1.SelectedItems[0];
                ListViewInputBox input = new ListViewInputBox(listView1, item, 1);
                input.FinishInput += new ListViewInputBox.InputEventHandler(input_FinishInput);
                input.Show();
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

        private void 常に手前に表示するToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            AppConfig.TopMost = 常に手前に表示するToolStripMenuItem.Checked;
            this.TopMost = AppConfig.TopMost;
        }

        private void プラグイン一覧VToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViewPluginForm form = new ViewPluginForm(pluginController);
            form.ShowDialog(this);
        }

        #region タスクトレイ関係...
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                WindowOpen();
            }
        }

        private void WindowOpen()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
            this.Activate();
        }

        /// <summary>
        /// タスクトレイアイコンのコンテキストメニューをクリックしたときに呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 開くOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowOpen();
        }

        /// <summary>
        /// タスクトレイアイコンのコンテキストメニューをクリックしたときに呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 閉じるCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.Icon = new Icon(Properties.Resources.TypingManager, new Size(16, 16));
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void 設定CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            configForm.ScheduledLogging = AppConfig.ScheduleLogging;
            configForm.ScheduleLogTiming = AppConfig.ScheduleTiming;
            configForm.UseLowLevelHook = AppConfig.HookLowLevel;
            configForm.ShowExitMessage = AppConfig.ShowExitMessage;
            configForm.NoStrokeLimitTime = AppConfig.NoStrokeLimitTime;
            configForm.SelectedItemCopyFormat = AppConfig.SelectedItemCopyFormat;
            configForm.RightClickCopyFormat = AppConfig.RightClickCopyFormat;
            DialogResult result = configForm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                AppConfig.ScheduleLogging = configForm.ScheduledLogging;
                AppConfig.ScheduleTiming = configForm.ScheduleLogTiming;
                AppConfig.ShowExitMessage = configForm.ShowExitMessage;
                AppConfig.NoStrokeLimitTime = configForm.NoStrokeLimitTime;
                timerTaskController.AddTask(strokeNumLog, StrokeNumLog.TIMER_ID_SAVE, 0, AppConfig.ScheduleTiming, 0);
                AppConfig.SelectedItemCopyFormat = configForm.SelectedItemCopyFormat;
                AppConfig.RightClickCopyFormat = configForm.RightClickCopyFormat;
                if (AppConfig.HookLowLevel != configForm.UseLowLevelHook)
                {
                    AppConfig.HookLowLevel = configForm.UseLowLevelHook;
                    MessageBox.Show(this,
                        "キー入力監視方法の設定が変更されました。\n" +
                        "設定は再起動後に有効になります。");
                }
            }
        }

        /// <summary>
        /// プロセス別打鍵数ビューで項目上をドラッグしたときに呼ばれる
        /// 左ボタンを押しながらマウスが移動した部分の項目を選択する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = listView1.HitTest(e.X, e.Y);
            if (e.Button == MouseButtons.Left && info.SubItem != null)
            {
                info.Item.Selected = true;
                // 24はリストビューヘッダーの高さ，17はアイテムの高さ
                // 定数にするのもなんかあれだったので．．
                if (info.Item.Position.Y < 24 + 17 && info.Item.Index > 1)
                {
                    listView1.EnsureVisible(info.Item.Index - 1);
                }
                else if (info.Item.Position.Y > listView1.Height - 24 && info.Item.Index < listView1.Items.Count - 1)
                {
                    listView1.EnsureVisible(info.Item.Index);
                }
            }
        }

        private void 選択した項目をコピーCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView view = null;
            StringBuilder copy_text = new StringBuilder();
            switch (tabControl1.SelectedIndex)
            {
                case 1:
                    view = ProcessStrokeView;
                    break;
                case 2:
                    view = DayStrokeView;
                    break;
            }
            if (view != null)
            {
                foreach (ListViewItem item in view.SelectedItems)
                {
                    string line_format = (string)AppConfig.SelectedItemCopyFormat.Clone();
                    for (int i = 0; i < item.SubItems.Count; i++)
                    {
                        string replace_mark = string.Format("%{0}",i+1);
                        line_format = line_format.Replace(replace_mark, item.SubItems[i].Text);
                    }
                    line_format = line_format.Replace("\\t", "\t");
                    copy_text.AppendLine(line_format);
                }
            }
            if (copy_text.Length != 0)
            {
                // クリップボードに文字列をコピーする
                // アプリケーション終了後もクリップボードに残る
                Clipboard.SetText(copy_text.ToString());
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
