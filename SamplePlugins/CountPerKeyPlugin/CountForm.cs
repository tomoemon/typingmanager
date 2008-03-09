using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Plugin;

namespace CountPerKey
{
    public partial class CountForm : Form
    {
        private CountMain count;
        private int total_other = 0;
        private int today_other = 0;

        public CountForm(CountMain _count)
        {
            InitializeComponent();

            listView1.ListViewItemSorter = new NumSort(1,2,3);
            count = _count;
            this.Icon = count.MainForm.Icon;
            FormDataLoad();
        }

        public void FormDataUpdate(int keycode)
        {
            string keyname = VirtualKeyName.GetKeyName(keycode);

            string total = count.TotalKey[keycode].ToString();
            string today = count.TodayKey[keycode].ToString();
            string average = (count.TotalKey[keycode] / count.TotalDay).ToString();
            if (keyname == "")
            {
                keyname = "その他";
                total = (++total_other).ToString();
                today = (++today_other).ToString();
                average = (total_other / count.TotalDay).ToString();
            }
            if (listView1.Items.ContainsKey(keyname))
            {
                listView1.Items[keyname].SubItems[1].Text = total;
                listView1.Items[keyname].SubItems[2].Text = today;
                listView1.Items[keyname].SubItems[3].Text = average;
            }
            else
            {
                listView1.Items.Add(keyname, keyname, "");
                listView1.Items[keyname].SubItems.Add(total);
                listView1.Items[keyname].SubItems.Add(today);
                listView1.Items[keyname].SubItems.Add(average);
            }
        }

        public void FormDataLoad()
        {
            listView1.Items.Clear();
            textBox1.Text = count.StartDate.ToString("yyyy年MM月dd日");
            foreach (int keycode in count.TotalKey.Keys)
            {
                string keyname = VirtualKeyName.GetKeyName(keycode);
                if (keyname == "")
                {
                    keyname = "その他";
                    total_other += count.TotalKey[keycode];
                    if (count.TodayKey.ContainsKey(keycode))
                    {
                        today_other += count.TodayKey[keycode];
                    }
                    if (listView1.Items.ContainsKey(keyname))
                    {
                        listView1.Items[keyname].SubItems[1].Text = total_other.ToString();
                        listView1.Items[keyname].SubItems[2].Text = today_other.ToString();
                        listView1.Items[keyname].SubItems[3].Text = (total_other / count.TotalDay).ToString();
                    }
                    else
                    {
                        listView1.Items.Add(keyname, keyname, "");
                        listView1.Items[keyname].SubItems.Add(total_other.ToString());
                        listView1.Items[keyname].SubItems.Add(today_other.ToString());
                        listView1.Items[keyname].SubItems.Add((total_other / count.TotalDay).ToString());
                    }
                }
                else
                {
                    listView1.Items.Add(keyname, keyname, "");
                    listView1.Items[keyname].SubItems.Add(count.TotalKey[keycode].ToString());
                    if (count.TodayKey.ContainsKey(keycode))
                    {
                        listView1.Items[keyname].SubItems.Add(count.TodayKey[keycode].ToString());
                    }
                    else
                    {
                        listView1.Items[keyname].SubItems.Add("0");
                    }
                    listView1.Items[keyname].SubItems.Add((count.TotalKey[keycode] / count.TotalDay).ToString());
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("OKを押すとキー別打鍵数のログがすべて消えます",
                    "リセット確認", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                count.Reset();
                FormDataLoad();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CountForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            count.FormOpen = false;
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView(listView1, e.Column);
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
}