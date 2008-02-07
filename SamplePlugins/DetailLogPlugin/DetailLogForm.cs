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

            // ���X�g�r���[�̕������Ȃ�
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

        #region �v���p�e�B...
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
        /// �u���C���v�^�u�̏ڍ׃��O�̎擾�J�n�{�^���������ꂽ�Ƃ��ɌĂяo�����
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
        /// �ڍ׃��O�̎擾�I���{�^���������ꂽ�Ƃ��ɌĂяo�����
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
        /// �u�ڍ׃��O�v�^�u�̓ǂݍ��݃{�^�����������Ƃ��ɌĂяo�����
        /// �ڍ׃��O�t�@�C�������ׂēǂݍ���
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
        /// �u�ڍ׃��O�v�^�u�̕\������^�C�v��؂芷�����Ƃ��ɌĂяo�����
        /// �u�^�O�v��I�������ꍇ�͂��ׂẴ^�O��\������
        /// �u���t�v��I�������ꍇ�͏ڍ׃��O�̂��邷�ׂĂ̓��t��\������
        /// </summary>
        private void SetDetailLogSelect()
        {
            DetailLogSelectView.Items.Clear();
            if (TagClassifyButton.Checked)
            {
                DetailLogSelectView.Columns[0].Text = "�^�O";
                foreach (string tag in detailLogViewer.TagList)
                {
                    DetailLogSelectView.Items.Add(tag, tag, "");
                    DetailLogSelectView.Items[tag].SubItems.Add(detailLogViewer.GetTagSetNum(tag).ToString());
                }
            }
            else
            {
                DetailLogSelectView.Columns[0].Text = "���t";
                foreach (string date in detailLogViewer.DateList)
                {
                    DetailLogSelectView.Items.Add(date, date, "");
                    DetailLogSelectView.Items[date].SubItems.Add(detailLogViewer.GetDateSetNum(date).ToString());
                }
            }
        }

        /// <summary>
        /// �u�ڍ׃��O�v�^�u��CSV�o�̓{�^�����������Ƃ��ɌĂяo�����
        /// �I�������ڍ׃��O��CSV�t�@�C���֏o�͂���
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
        /// �u�ڍ׃��O�v�^�u�œ��t��^�O�̑I����ς����Ƃ��ɌĂяo�����
        /// �E���̃��X�g�r���[��ύX����
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
                 * ListView��Items.Add�����u�ԂɃ\�[�g���悤�Ƃ��邽�߁C
                 * ���ڈȊO�Ń\�[�g���悤�Ƃ���ƁC���̂��Ƃɒǉ�����\��̃A�C�e����
                 * �Ȃ����߁C�z��͈̔͊O�Ɨ�O����������
                 * ������������邽�߂ɂ͏�̂悤��ListViewItem����C�ɒǉ����邱��
                DetailLogView.Items.Add(key, date, "");
                DetailLogView.Items[key].Tag = info;
                DetailLogView.Items[key].SubItems.Add(info.Date.ToString(DetailLogViewer.TIME_FORMAT));
                DetailLogView.Items[key].SubItems.Add(info.TagConcat("", true) + " " + info.Comment);
                */
                Console.WriteLine(info.Date.ToString() + ":" + info.Comment);
            }
        }

        /// <summary>
        /// �ڍ׃��O�̕��ރ��X�g�r���[�̃\�[�g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView3_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView(DetailLogSelectView, e.Column);
        }

        /// <summary>
        /// �ڍ׃��O�̃��O�t�@�C�����X�g�r���[�̃\�[�g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView4_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView(DetailLogView, e.Column);
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

    public class NumSort : System.Collections.IComparer
    {
        private SortOrder sort_order = SortOrder.Ascending;	// �\�[�g��(�����E�~��)
        private int column = 0;	// �\�[�g��
        private List<int> num_column; // ���l�Ŕ�r�����

        #region �v���p�e�B...
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
        // ��r���ʂ�Ԃ�
        public int Compare(object x, object y)
        {
            int ret = 0;
            // ��r�p���X�g�A�C�e���i�[�ϐ�
            ListViewItem sx = (ListViewItem)x;
            ListViewItem sy = (ListViewItem)y;

            // ��������r���A�l���i�[
            if (num_column.Contains(column))
            {
                // ���̏��������Ă����Ȃ���column���͈͊O���Ɠ{���Ă��܂����Ƃ�����
                // �Č������F
                // �u�ڍ׃��O�v�^�u�̍��̃��X�g�r���[�Łu����2�ȏ�̍��ځv��I�����C
                // �E�̃��X�g�r���[��2��ڈȍ~�̂ǂꂩ�̗�ɂ��ă\�[�g���s�������
                // ���̃��X�g�r���[�́u����2�ȏ�̍��ځv��I�����悤�Ƃ���Ɨ�����D
                // �����F�A�C�e���̒ǉ��̎d���ɒ���
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

            // �~���̂Ƃ��͌��ʂ��t�]
            if (sort_order == SortOrder.Descending)
            {
                ret = -ret;
            }

            //���ʂ�Ԃ�
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