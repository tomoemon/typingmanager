using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Plugin;

namespace DetailLogPlugin
{
    public partial class DetailLogForm : Form
    {
        public const string WINDOW_TITLE = "詳細ログ設定";

        private DetailLogViewer detailLogViewer;
        private StrokeTimeLog strokeTimeLog;
        private DetailTrigger new_trigger = new DetailTrigger();
        private ViewStroke viewStroke;

        // ショートカットキーの登録時に押しっぱなしのキーを
        // 複数回登録しないために，一度押したキーを辞書に登録する
        // そのキーが離されたら辞書から消す
        private Dictionary<Keys, int> keydown_dic = new Dictionary<Keys,int>();

        public const int MAX_COMBOBOX_HISTORY = 20;
        private const int LISTVIEW_RMARGIN = 25;
        private const int LISTVEW_SMALL_ICON_SIZE = 16;
        private const int TRIGGER_VIEW_LMARGIN = 15;
        private const int TRIGGER_VIEW_RMARGIN = 21;
        private const int TRIGGER_VIEW_BMARGIN = 35;
        private const int DETAIL_VIEW_BMARGIN = 10;
        private const int DETAIL_VIEW_TEXT_MARGIN = 5;

        public DetailLogForm(StrokeTimeLog log)
        {
            InitializeComponent();

            viewStroke = new ViewStroke(textBox5, textBox4, (int)numericUpDown1.Value);
            detailLogViewer = new DetailLogViewer();
            strokeTimeLog = log;

            // メインウィンドウで使っているアイコンを流用する
            this.Icon = strokeTimeLog.MainForm.Icon;

            // リストビューの幅調整など
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

            TriggerView.Width = tabControl1.Width - TRIGGER_VIEW_LMARGIN - TRIGGER_VIEW_RMARGIN;
            TriggerView.Height = tabControl1.Height - TriggerView.Location.Y - TRIGGER_VIEW_BMARGIN;
            TriggerView.Location = new Point(TRIGGER_VIEW_LMARGIN, TriggerView.Location.Y);
            TriggerView.SmallImageList = new ImageList();
            TriggerView.SmallImageList.ImageSize = new Size(1, LISTVEW_SMALL_ICON_SIZE);
            TriggerView.ListViewItemSorter = new NumSort();
            TriggerView.ShowItemToolTips = true;
            TriggerView.Columns[3].Width = TriggerView.Width - TriggerView.Columns[0].Width -
                TriggerView.Columns[1].Width - TriggerView.Columns[2].Width - LISTVIEW_RMARGIN;

            // コメントの履歴を追加
            if (strokeTimeLog.Comment.Count > 0)
            {
                comboBox1.Items.Clear();
            }
            foreach (string str in strokeTimeLog.Comment)
            {
                comboBox1.Items.Add(str);
            }

            // 作成済みの詳細ログトリガを詳細ログトリガのリストビューに表示
            foreach (DetailTrigger trigger in strokeTimeLog.TriggerCtrl.GetAllTrigers())
            {
                string name = strokeTimeLog.ProcessName.GetName(trigger.Path);
                if (trigger.Path == TriggerController.TARGET_ALL_PROCESS)
                {
                    name = TriggerController.TARGET_ALL_PROCESS;
                }
                ListViewItem item = new ListViewItem(name);
                item.SubItems.Add(trigger.Comment);
                item.SubItems.Add(trigger.Start.ToString());
                item.SubItems.Add(trigger.End.ToString());
                item.Tag = trigger;
                TriggerView.Items.Add(item);
            }

            // アプリケーションIDを登録しておく
            List<int> id_list = strokeTimeLog.ProcessName.GetProcessList();
            
            // すべてのアプリケーションを対象とする場合を先に登録しておく
            comboBox2.Items.Add(TriggerController.TARGET_ALL_PROCESS + " … すべてのプロセスを対象とする");

            // 0番目にはnullプロセスが入っているので，これをすべてのプロセスと置き換える
            id_list.RemoveAt(0);

            foreach (int app_id in id_list)
            {
                string proc_name = strokeTimeLog.ProcessName.GetName(app_id);
                comboBox2.Items.Add(proc_name);
            }
            id_list.Insert(0, TriggerController.ALL_PROCESS_ID);
            comboBox2.Tag = id_list;

            // すでに詳細ロギングの最中の場合は開始ボタンを無効にする
            if (strokeTimeLog.Logging)
            {
                StartButton.Enabled = false;
                EndButon.Enabled = true;
            }
        }

        #region プロパティ...
        public Button StartButton
        {
            get { return button1; }
        }
        public Button EndButon
        {
            get { return button2; }
        }
        public ListView TriggerView
        {
            get { return listView1; }
        }
        public ListView DetailLogSelectView
        {
            get { return listView3; }
        }
        public ListView DetailLogView
        {
            get { return listView4; }
        }
        public RadioButton DateClassifyButton
        {
            get { return radioButton1; }
        }
        public RadioButton TagClassifyButton
        {
            get { return radioButton2; }
        }
        #endregion


        private void DetailLogForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            strokeTimeLog.FormOpen = false;
        }

        /// <summary>
        /// 詳細ログの取得開始ボタンが押されたときに呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            strokeTimeLog.LoggingStart(TriggerController.TARGET_ALL_PROCESS, comboBox1.Text);
            if (comboBox1.Items.IndexOf(comboBox1.Text) == -1)
            {
                comboBox1.Items.Add(comboBox1.Text);
                if (comboBox1.Items.Count > MAX_COMBOBOX_HISTORY)
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

        #region 詳細ログ表示タブ関連
        /// <summary>
        /// 「詳細ログ」タブの読み込みボタンを押したときに呼び出される
        /// 詳細ログファイルをすべて読み込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Load Button Clicked");
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

                string plugin_dir = Path.Combine(LogDir.LOG_DIR,StrokeTimeLog.PLUGIN_NAME);
                string csv_dir = Path.Combine(plugin_dir, StrokeTimeLog.CSV_DIR);
                string filename = Path.Combine(csv_dir, 
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
            Console.WriteLine(item.Text);
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
                Console.WriteLine("###" + DetailLogView.Items[add_item.Name].Text);
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
                Console.WriteLine(info.Date.ToString() + ":" + info.Comment);
            }
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
        /// 開始ショートカットシーケンス欄でキーを押したときに呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            // Shift, Control, Altキーが単独で押された時は無視する
            if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Menu
                || e.KeyCode == Keys.ControlKey)
            {
                return;
            }

            if (!keydown_dic.ContainsKey(e.KeyCode))
            {
                //Console.WriteLine("{0}:{1}:{2}:{3}",e.KeyCode, e.KeyValue,e.KeyData,e.Modifiers);
                keydown_dic[e.KeyCode] = 1;
                new_trigger.Start.Add(e.KeyCode, e.KeyData);
                textBox2.Text = new_trigger.Start.ToString();
            }
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (keydown_dic.ContainsKey(e.KeyCode))
            {
                keydown_dic.Remove(e.KeyCode);
            }
        }

        /// <summary>
        /// 終了ショートカットシーケンス欄でキーを押したときに呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            // Shift, Control, Altキーが単独で押された時は無視する
            if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Menu
                || e.KeyCode == Keys.ControlKey)
            {
                return;
            }

            if (!keydown_dic.ContainsKey(e.KeyCode))
            {
                //Console.WriteLine("{0}:{1}:{2}:{3}",e.KeyCode, e.KeyValue,e.KeyData,e.Modifiers);
                keydown_dic[e.KeyCode] = 1;
                new_trigger.End.Add(e.KeyCode, e.KeyData);
                textBox3.Text = new_trigger.End.ToString();
            }
        }

        private void textBox3_KeyUp(object sender, KeyEventArgs e)
        {
            if (keydown_dic.ContainsKey(e.KeyCode))
            {
                keydown_dic.Remove(e.KeyCode);
            }
        }

        /// <summary>
        /// トリガを追加ボタンを押したときに呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            keydown_dic.Clear();
            int index = comboBox2.SelectedIndex;
            
            if (index < 0)
            {
                MessageBox.Show("対象プロセスを選択してください");
                return;
            }
            else if(new_trigger.Start.ToString() == ""){
                MessageBox.Show("開始ショートカットキーを入力してください");
                return;
            }
            else if(new_trigger.End.ToString() == ""){
                MessageBox.Show("終了ショートカットキーを入力してください");
                return;
            }

            int app_id = ((List<int>)comboBox2.Tag)[index];
            string app_path = strokeTimeLog.ProcessName.GetPath(app_id);
            string app_name = strokeTimeLog.ProcessName.GetName(app_id);
            if (app_id == TriggerController.ALL_PROCESS_ID)
            {
                app_path = TriggerController.TARGET_ALL_PROCESS;
                app_name = TriggerController.TARGET_ALL_PROCESS;
            }

            new_trigger.Path = app_path;
            new_trigger.Comment = textBox1.Text;
            //Console.WriteLine("before regist プロセスID:{0}, プロセス名:{1}, コメント:{2}, 開始:{3}, 終了:{4}",
            //    app_id, app_path, new_trigger.Comment, new_trigger.Start.ToString(), new_trigger.End.ToString());

            if (!strokeTimeLog.TriggerCtrl.Add(new_trigger))
            {
                MessageBox.Show("そのトリガはすでに登録済みです");
                return;
            }

            //Console.WriteLine("after regist プロセスID:{0}, プロセス名:{1}, コメント:{2}, 開始:{3}, 終了:{4}",
            //    app_id, app_path, new_trigger.Comment, new_trigger.Start.ToString(), new_trigger.End.ToString());

            // リストビューに追加する
            ListViewItem item = new ListViewItem(app_name);
            item.SubItems.Add(new_trigger.Comment);
            item.SubItems.Add(new_trigger.Start.ToString());
            item.SubItems.Add(new_trigger.End.ToString());
            item.Tag = new_trigger;
            TriggerView.Items.Add(item);

            // GUIを初期化
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";

            // 次の入力用に新しいインスタンスを作成する
            new_trigger = new DetailTrigger();
            
        }

        /// <summary>
        /// トリガを削除ボタンを押したときに呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            keydown_dic.Clear();
            if (TriggerView.SelectedIndices.Count == 0)
            {
                return;
            }

            int index = TriggerView.SelectedIndices[0];

            DetailTrigger trigger = (DetailTrigger)TriggerView.SelectedItems[0].Tag;
            if (strokeTimeLog.TriggerCtrl.Remove(trigger))
            {
                TriggerView.Items.RemoveAt(index);
            }
        }

        /// <summary>
        /// タブコントロールのサイズが変わった時に呼び出される
        /// 詳細ログトリガのリストビューのサイズを変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitContainer1_SizeChanged(object sender, EventArgs e)
        {
            // 詳細トリガビューのサイズを変える
            TriggerView.Width = tabControl1.Width - TRIGGER_VIEW_LMARGIN - TRIGGER_VIEW_RMARGIN;
            TriggerView.Height = tabControl1.Height - TriggerView.Location.Y - TRIGGER_VIEW_BMARGIN;

            int new_width = TriggerView.Width - LISTVIEW_RMARGIN;
            ChangeListViewColumnWidth(TriggerView, new_width);
        }

        /// <summary>
        /// 詳細ログ表示ビューのサイズが変わった時に呼び出される
        /// 左のリストビューと右のリストビューのサイズを変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitContainer3_SizeChanged(object sender, EventArgs e)
        {
            DetailLogSelectView.Width = splitContainer3.Panel1.Width - TRIGGER_VIEW_LMARGIN;
            DetailLogSelectView.Height = splitContainer3.Panel1.Height - DetailLogSelectView.Location.Y - DETAIL_VIEW_BMARGIN;
            ChangeListViewColumnWidth(DetailLogSelectView, DetailLogSelectView.Width - LISTVIEW_RMARGIN);

            DetailLogView.Width = splitContainer3.Panel2.Width - TRIGGER_VIEW_RMARGIN;
            DetailLogView.Height = splitContainer3.Panel2.Height - DetailLogView.Location.Y - DETAIL_VIEW_BMARGIN;
            ChangeListViewColumnWidth(DetailLogView, DetailLogView.Width - LISTVIEW_RMARGIN);
        }

        /// <summary>
        /// リストビューのカラムサイズを今までの比率に合わせて変更する
        /// </summary>
        /// <param name="view"></param>
        /// <param name="new_width"></param>
        private void ChangeListViewColumnWidth(ListView view, int new_width)
        {
            // リストビューのサイズ変更に合わせてカラムの幅も変更する
            int last_total_width = 0;
            for (int i = 0; i < view.Columns.Count; i++)
            {
                last_total_width += view.Columns[i].Width;
            }

            float ratio = (float)new_width / last_total_width;
            for (int i = 0; i < view.Columns.Count; i++)
            {
                view.Columns[i].Width = (int)(view.Columns[i].Width * ratio);
            }
        }

        public void KeyStrokeDown(IKeyState key_state, uint militime)
        {
            viewStroke.KeyDown(key_state, militime);
        }

        public void KeyStrokeUp(IKeyState key_state, uint militime)
        {
            viewStroke.KeyUp(key_state, militime);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            viewStroke.MaxLogNum = (int)numericUpDown1.Value;
        }

        private void numericUpDown1_KeyDown(object sender, KeyEventArgs e)
        {
            viewStroke.MaxLogNum = (int)numericUpDown1.Value;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            viewStroke.Clear();
        }

        private void splitContainer5_SizeChanged(object sender, EventArgs e)
        {
            textBox4.Left = DETAIL_VIEW_TEXT_MARGIN;
            textBox4.Width = splitContainer5.Panel1.Width - DETAIL_VIEW_TEXT_MARGIN * 2;
            textBox4.Height = splitContainer5.Panel1.Height - textBox4.Top - DETAIL_VIEW_TEXT_MARGIN;

            textBox5.Left = DETAIL_VIEW_TEXT_MARGIN;
            textBox5.Width = splitContainer5.Panel2.Width - DETAIL_VIEW_TEXT_MARGIN * 2;
            textBox5.Height = splitContainer5.Panel2.Height - textBox5.Top - DETAIL_VIEW_TEXT_MARGIN;


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