using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DetailLogPlugin
{
    public partial class DetailLogForm : Form
    {
        private DetailLogViewer detailLogViewer;
        private StrokeTimeLog strokeTimeLog;

        private const int LISTVIEW_RMARGIN = 25;
        private const int LISTVEW_SMALL_ICON_SIZE = 16;

        public DetailLogForm(StrokeTimeLog log)
        {
            InitializeComponent();

            detailLogViewer = new DetailLogViewer();
            strokeTimeLog = log;

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
            
        }

        #region プロパティ...
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

                string filename = Path.Combine(Plugin.LogDir.DETAIL_CSV_DIR,
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

        private void DetailLogForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            strokeTimeLog.FormOpen = false;
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