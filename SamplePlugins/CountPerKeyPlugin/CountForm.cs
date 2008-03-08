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
            if (keyname == "")
            {
                keyname = "���̑�";
            }
            if (listView1.Items.ContainsKey(keyname))
            {
                listView1.Items[keyname].SubItems[1].Text = count.TotalKey[keycode].ToString();
                listView1.Items[keyname].SubItems[2].Text = count.TodayKey[keycode].ToString();
                listView1.Items[keyname].SubItems[3].Text = (count.TotalKey[keycode] / count.TotalDay).ToString();
            }
            else
            {
                listView1.Items.Add(keyname, keyname, "");
                listView1.Items[keyname].SubItems.Add(count.TotalKey[keycode].ToString());
                listView1.Items[keyname].SubItems.Add(count.TodayKey[keycode].ToString());
                listView1.Items[keyname].SubItems.Add((count.TotalKey[keycode] / count.TotalDay).ToString());
            }
        }

        public void FormDataLoad()
        {
            listView1.Items.Clear();
            textBox1.Text = count.StartDate.ToString("yyyy�NMM��dd��");
            foreach (int keycode in count.TotalKey.Keys)
            {
                string keyname = VirtualKeyName.GetKeyName(keycode);
                if (keyname == "")
                {
                    keyname = "���̑�";
                }
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

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("OK�������ƃL�[�ʑŌ����̃��O�����ׂď����܂�",
                    "���Z�b�g�m�F", MessageBoxButtons.OKCancel);
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
        /// ���X�g�r���[�Ɨ���w�肵�ă\�[�g����
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