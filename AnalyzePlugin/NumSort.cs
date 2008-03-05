using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Plugin
{
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
