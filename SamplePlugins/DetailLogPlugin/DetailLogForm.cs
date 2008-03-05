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
        public const string WINDOW_TITLE = "�ڍ׃��O�ݒ�";

        private DetailLogViewer detailLogViewer;
        private StrokeTimeLog strokeTimeLog;
        private DetailTrigger new_trigger = new DetailTrigger();
        private ViewStroke viewStroke;

        // �V���[�g�J�b�g�L�[�̓o�^���ɉ������ςȂ��̃L�[��
        // ������o�^���Ȃ����߂ɁC��x�������L�[�������ɓo�^����
        // ���̃L�[�������ꂽ�玫���������
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

            // ���C���E�B���h�E�Ŏg���Ă���A�C�R���𗬗p����
            this.Icon = strokeTimeLog.MainForm.Icon;

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

            TriggerView.Width = tabControl1.Width - TRIGGER_VIEW_LMARGIN - TRIGGER_VIEW_RMARGIN;
            TriggerView.Height = tabControl1.Height - TriggerView.Location.Y - TRIGGER_VIEW_BMARGIN;
            TriggerView.Location = new Point(TRIGGER_VIEW_LMARGIN, TriggerView.Location.Y);
            TriggerView.SmallImageList = new ImageList();
            TriggerView.SmallImageList.ImageSize = new Size(1, LISTVEW_SMALL_ICON_SIZE);
            TriggerView.ListViewItemSorter = new NumSort();
            TriggerView.ShowItemToolTips = true;
            TriggerView.Columns[3].Width = TriggerView.Width - TriggerView.Columns[0].Width -
                TriggerView.Columns[1].Width - TriggerView.Columns[2].Width - LISTVIEW_RMARGIN;

            // �R�����g�̗�����ǉ�
            if (strokeTimeLog.Comment.Count > 0)
            {
                comboBox1.Items.Clear();
            }
            foreach (string str in strokeTimeLog.Comment)
            {
                comboBox1.Items.Add(str);
            }

            // �쐬�ς݂̏ڍ׃��O�g���K���ڍ׃��O�g���K�̃��X�g�r���[�ɕ\��
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

            // �A�v���P�[�V����ID��o�^���Ă���
            List<int> id_list = strokeTimeLog.ProcessName.GetProcessList();
            
            // ���ׂẴA�v���P�[�V������ΏۂƂ���ꍇ���ɓo�^���Ă���
            comboBox2.Items.Add(TriggerController.TARGET_ALL_PROCESS + " �c ���ׂẴv���Z�X��ΏۂƂ���");

            // 0�Ԗڂɂ�null�v���Z�X�������Ă���̂ŁC��������ׂẴv���Z�X�ƒu��������
            id_list.RemoveAt(0);

            foreach (int app_id in id_list)
            {
                string proc_name = strokeTimeLog.ProcessName.GetName(app_id);
                comboBox2.Items.Add(proc_name);
            }
            id_list.Insert(0, TriggerController.ALL_PROCESS_ID);
            comboBox2.Tag = id_list;

            // ���łɏڍ׃��M���O�̍Œ��̏ꍇ�͊J�n�{�^���𖳌��ɂ���
            if (strokeTimeLog.Logging)
            {
                StartButton.Enabled = false;
                EndButon.Enabled = true;
            }
        }

        #region �v���p�e�B...
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
        /// �ڍ׃��O�̎擾�J�n�{�^���������ꂽ�Ƃ��ɌĂяo�����
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

        #region �ڍ׃��O�\���^�u�֘A
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
        #endregion

        /// <summary>
        /// �J�n�V���[�g�J�b�g�V�[�P���X���ŃL�[���������Ƃ��ɌĂ΂��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            // Shift, Control, Alt�L�[���P�Ƃŉ����ꂽ���͖�������
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
        /// �I���V���[�g�J�b�g�V�[�P���X���ŃL�[���������Ƃ��ɌĂ΂��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            // Shift, Control, Alt�L�[���P�Ƃŉ����ꂽ���͖�������
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
        /// �g���K��ǉ��{�^�����������Ƃ��ɌĂ΂��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            keydown_dic.Clear();
            int index = comboBox2.SelectedIndex;
            
            if (index < 0)
            {
                MessageBox.Show("�Ώۃv���Z�X��I�����Ă�������");
                return;
            }
            else if(new_trigger.Start.ToString() == ""){
                MessageBox.Show("�J�n�V���[�g�J�b�g�L�[����͂��Ă�������");
                return;
            }
            else if(new_trigger.End.ToString() == ""){
                MessageBox.Show("�I���V���[�g�J�b�g�L�[����͂��Ă�������");
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
            //Console.WriteLine("before regist �v���Z�XID:{0}, �v���Z�X��:{1}, �R�����g:{2}, �J�n:{3}, �I��:{4}",
            //    app_id, app_path, new_trigger.Comment, new_trigger.Start.ToString(), new_trigger.End.ToString());

            if (!strokeTimeLog.TriggerCtrl.Add(new_trigger))
            {
                MessageBox.Show("���̃g���K�͂��łɓo�^�ς݂ł�");
                return;
            }

            //Console.WriteLine("after regist �v���Z�XID:{0}, �v���Z�X��:{1}, �R�����g:{2}, �J�n:{3}, �I��:{4}",
            //    app_id, app_path, new_trigger.Comment, new_trigger.Start.ToString(), new_trigger.End.ToString());

            // ���X�g�r���[�ɒǉ�����
            ListViewItem item = new ListViewItem(app_name);
            item.SubItems.Add(new_trigger.Comment);
            item.SubItems.Add(new_trigger.Start.ToString());
            item.SubItems.Add(new_trigger.End.ToString());
            item.Tag = new_trigger;
            TriggerView.Items.Add(item);

            // GUI��������
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";

            // ���̓��͗p�ɐV�����C���X�^���X���쐬����
            new_trigger = new DetailTrigger();
            
        }

        /// <summary>
        /// �g���K���폜�{�^�����������Ƃ��ɌĂ΂��
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
        /// �^�u�R���g���[���̃T�C�Y���ς�������ɌĂяo�����
        /// �ڍ׃��O�g���K�̃��X�g�r���[�̃T�C�Y��ύX����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitContainer1_SizeChanged(object sender, EventArgs e)
        {
            // �ڍ׃g���K�r���[�̃T�C�Y��ς���
            TriggerView.Width = tabControl1.Width - TRIGGER_VIEW_LMARGIN - TRIGGER_VIEW_RMARGIN;
            TriggerView.Height = tabControl1.Height - TriggerView.Location.Y - TRIGGER_VIEW_BMARGIN;

            int new_width = TriggerView.Width - LISTVIEW_RMARGIN;
            ChangeListViewColumnWidth(TriggerView, new_width);
        }

        /// <summary>
        /// �ڍ׃��O�\���r���[�̃T�C�Y���ς�������ɌĂяo�����
        /// ���̃��X�g�r���[�ƉE�̃��X�g�r���[�̃T�C�Y��ύX����
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
        /// ���X�g�r���[�̃J�����T�C�Y�����܂ł̔䗦�ɍ��킹�ĕύX����
        /// </summary>
        /// <param name="view"></param>
        /// <param name="new_width"></param>
        private void ChangeListViewColumnWidth(ListView view, int new_width)
        {
            // ���X�g�r���[�̃T�C�Y�ύX�ɍ��킹�ăJ�����̕����ύX����
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