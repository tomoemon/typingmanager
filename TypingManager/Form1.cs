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
        /// ���t�ʑŌ����X�g�r���[�ƃv���Z�X�ʑŌ����X�g�r���[��
        /// �����΂�E�̍��ڂ��猩�ĉE���̗]�������̕�
        /// ���ڐ��������ďc�̃X�N���[���o�[�������ƕ��������Ȃ���
        /// ���܂��C���̃X�N���[���o�[���o�Ă���D�����h����l
        /// </summary>
        public const int LISTVIEW_RMARGIN = 25;
        public const int LISTVEW_SMALL_ICON_SIZE = 16;

        #region �v���p�e�B...
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
            get { return ��Ɏ�O�ɕ\������ToolStripMenuItem; }
        }
        public ToolStripMenuItem IsSaveTitleStroke
        {
            get { return �E�B���h�E�^�C�g���ʂ̑Ō�����ۑ�TToolStripMenuItem; }
        }
        public ToolStripMenuItem GraphMarkMenu
        {
            get { return �����O���t�̃}�[�NMToolStripMenuItem; }
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
            get { return �v���O�C��PToolStripMenuItem; }
        }
        #endregion

        public Form1()
        {
            InitializeComponent();

            // �N�����̃E�B���h�E�̏�Ԃ͐ݒ�ŕς�����悤�ɂ��悤
            //this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            // ���O�ۑ��p�̃f�B���N�g���̑��݃`�F�b�N�Ɛ���
            Plugin.LogDir.LogDirectoryCheck();

            // �A�v���P�[�V�����̐ݒ�t�@�C���ǂݍ���
            AppConfig.Load();
            IsTopMostMenu.Checked = AppConfig.TopMost;
            if (AppConfig.TabIndex >= tabControl1.TabCount)
            {
                AppConfig.TabIndex = 0;
            }
            tabControl1.TabIndex = AppConfig.TabIndex;
            this.Text = Properties.Resources.ApplicationName;

            // �L�[�{�[�h�t�b�N�̊J�n
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

            // �e���X�g�r���[�̕������Ȃ�
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
            
            // �X�e�[�^�X�o�[�Ɍ��ݎ����𔽉f
            DateTimeStatus.Text = DateTime.Now.ToString();

            // �Ō����J�E���g��S������N���X
            strokeNumLog = new StrokeNumLog();
            
            // �L�[�̏�Ԃ�ۑ�����N���X
            keyState = new KeyState();
            
            // �v���O�C���R���g���[���̍쐬�ƃv���O�C���̓ǂݍ���
            pluginController = new PluginController(this);
            pluginController.AddStrokePlugin(strokeNumLog);
            pluginController.AddStrokePlugin(strokeNumLog.ProcessName);
            pluginController.Load();
            pluginController.AddMenu(PluginMenu);
            
            // �Ō����x���v�Z����N���X���쐬�i�T���v����20�j
            typingSpeed = new TypingSpeed(20);

            // �L�[���͂ɉ�����GUI�̒l���X�V����N���X
            keyStrokeView = new KeyStrokeView(this, strokeNumLog);

            // �ǂݍ���AppConfig����GUI�ɔ��f������
            SetProcessViewType();       // �v���Z�X�ʑŌ����̕\���^�C�v�̃`�F�b�N
            SetSaveTitleStrokeMenu();   // �^�C�g���ʂ̑Ō�����ۑ����邩�̃`�F�b�N
            SetGraphMarkCheck(AppConfig.MarkType); // �O���t�ɂ���}�[�N

            // NotifyIcon�i�^�X�N�g���C�A�C�R���j�̃e�L�X�g��ύX����
            notifyIcon1.Text = string.Format("����:{0}{1}���:{2}{3}���v:{4}",
                strokeNumLog.TodayTotalType, Environment.NewLine, 
                strokeNumLog.YesterdayTotalType, Environment.NewLine,
                strokeNumLog.TotalType);

            // �Ō����O��GUI�ɔ��f������
            keyStrokeView.DayStrokeViewLoad();
            keyStrokeView.ProcessViewLoad();
            keyStrokeView.MainTabLoad();

            // �����O���t�Ɋւ���ݒ�
            graphChanger = new GraphChanger(HistoryPicture, HistoryMaxValue, HistoryMinValue,
                TypeSpeedPicture, TypeSpeedText);
            graphChanger.SetMargin(HistoryMaxValue.Width+3, 1, 5, 3);
            graphChanger.SetGraph(LineGraphType.StrokeNumPerMinute, "�Ō����̗���(1������)", 60, 200);
            graphChanger.SetGraph(LineGraphType.StrokeNumPerHour, "�Ō����̗���(1���Ԃ���)", 24, 5000);
            graphChanger.SetGraph(LineGraphType.StrokeNumPerDay, "�Ō����̗���(1������)", 30, 5000);
            graphChanger.SetGraph(LineGraphType.TypeSpeedPerStroke, "�Ō����x�̗����i��/���j", 60, 200);
            graphChanger.SetMarkSize(LineGraphMarkType.Plus, 4);
            graphChanger.SetMarkSize(LineGraphMarkType.Square, 3);
            graphChanger.SetMarkSize(LineGraphMarkType.VerticalBar, 4);
            graphChanger.SetMarkSize(LineGraphMarkType.HorizonBar, 4);
            graphChanger.SetDataSource(strokeNumLog, typingSpeed);

            // �����O���t�ɂ���}�[�N�̕ύX
            graphChanger.SetGraphMark(AppConfig.MarkType);

            // �����O���t�ɒl�̐ݒ�
            graphChanger.SetGraphValue();

            // �����O���t���̍X�V
            HistoryGraphName.Text = graphChanger[AppConfig.LineGraphType].GraphName;

            // Timer���g�p����^�X�N�̓o�^
            timerTaskController = new TimerTaskController();
            timerTaskController.AddTask(strokeNumLog, StrokeNumLog.TIMER_ID_NEWDAY, 1, 0, 0);
            timerTaskController.AddTask(strokeNumLog, StrokeNumLog.TIMER_ID_SAVE, 0, AppConfig.ScheduleTiming, 0);
            timerTaskController.AddTask(graphChanger, GraphChanger.TIMER_ID_UPDATE, 0, 0, 1);
        }

        /// <summary>
        /// �L�[�̏グ���������������Ƃ��ɌĂяo�����t�b�N�v���V�[�W��
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
            notifyIcon1.Text = string.Format("����:{0}{1}���:{2}{3}���v:{4}",
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
        /// �I�����̃��O�ۑ��Ȃ�
        /// �A�v���P�[�V����������I���������s����������
        /// ���̊֐��𒼐ڎg��Ȃ���this.Close()���g�����ƁD
        /// Form1_FormClosing���Ă΂�āC�������炳���AppFinalize()���Ă΂��D
        /// </summary>
        /// <returns></returns>
        private bool AppFinalize()
        {
            if (AppConfig.ShowExitMessage)
            {
                DialogResult res = MessageBox.Show("�I�����܂����H", "�I���m�F",
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

        private void �I��ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // FormClosing�̒��ŏI�����̏������s��
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
        /// �f�t�H���g��1�b���ƂɃ^�C�}�[����Ă΂��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            timerTaskController.CallTask(DateTime.Now);
            DateTimeStatus.Text = DateTime.Now.ToString();
        }

        /// <summary>
        /// �Ō����x�̏u�Ԓl�O���t�̕`��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            graphChanger.DrawGauge(e.Graphics);
        }

        /// <summary>
        /// �u���C���v�^�u�̗����O���t���N���b�N���ꂽ�Ƃ��ɌĂяo�����
        /// �\������O���t�̎�ނ�ύX����
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

        #region ���O���t�ɂ���}�[�N�̕ύX
        private void �Ȃ�ToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void �����O���t�̃}�[�NMToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (ToolStripMenuItem menu in GraphMarkMenu.DropDownItems)
            {
                menu.Checked = false;
            }
        }

        /// <summary>
        /// �w�肵���^�C�v�̃O���t�}�[�N�����j���[�̃`�F�b�N�ɔ��f������
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

        #region �e���X�g�r���[�̃\�[�g
        /// <summary>
        /// �v���Z�X�ʑŌ������X�g�r���[�̃\�[�g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView(ProcessStrokeView, e.Column);
        }

        /// <summary>
        /// ���ʑŌ������X�g�r���[�̃\�[�g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView(DayStrokeView, e.Column);
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

        private void �X�^�[�g�A�b�v�ɓo�^ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShortCut.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                "�Ō���tomo 2.0.lnk"), "TypingManager.exe", "�Ō���tomo 2.0�ւ̃V���[�g�J�b�g");
            MessageBox.Show(Properties.Resources.StartupRegist);
        }

        private void �X�^�[�g�A�b�v������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                             "�Ō���tomo 2.0.lnk");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            MessageBox.Show(Properties.Resources.StartupUnregist);
        }

        /// <summary>
        /// �u�v���Z�X�ʑŌ����v�^�u�ō����̃v���Z�X�ʑŌ������N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            AppConfig.ProcessViewType = ProcessStrokeViewType.Today;
            keyStrokeView.ProcessViewLoad();
        }

        /// <summary>
        /// �u�v���Z�X�ʑŌ����v�^�u�ł��ׂẴv���Z�X�ʑŌ������N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            AppConfig.ProcessViewType = ProcessStrokeViewType.All;
            keyStrokeView.ProcessViewLoad();
        }

        #region AppConfig�̒l��GUI���j���[�̃`�F�b�N��Ԃɔ��f������
        /// <summary>
        /// AppConfig�̒l��GUI���j���[�̃`�F�b�N��Ԃɔ��f������
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
        /// AppConfig�̒l��GUI���j���[�̃`�F�b�N��Ԃɔ��f������
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
        /// GUI�̕ύX��AppConfig�ɔ��f������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void �E�B���h�E�^�C�g���ʂ̑Ō�����ۑ�TToolStripMenuItem_Click(object sender, EventArgs e)
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
        /// �v���Z�X�ʑŌ����X�g�r���[�ō��ڂ��E�N���b�N�����Ƃ��ɌĂяo�����
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
                // num_log.AppLog[app_id]��app_id�����݂��Ȃ��Ƃ���O���������鋰�ꂪ����
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
            menu.MenuItems.Add("�N���b�v�{�[�h�ɃR�s�[����(&C)");

            // �u�N���b�v�{�[�h�ɃR�s�[����v���j���[���N���b�N�����Ƃ���
            // �e�̃��j���[���Q�Ƃ��邽�߁CTag�ɓo�^���Ă���
            menu.MenuItems[menu.MenuItems.Count - 1].Tag = menu;
            menu.MenuItems[menu.MenuItems.Count - 1].Click += new EventHandler(Title_Menu_Click);

            return menu;
        }

        /// <summary>
        /// �v���Z�X�ʑŌ����X�g�r���[�ŕ\���������j���[��
        /// �u�N���b�v�{�[�h�ɃR�s�[����v���N���b�N�����Ƃ��ɌĂяo�����
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
                // �N���b�v�{�[�h�ɕ�������R�s�[����
                // �A�v���P�[�V�����I������N���b�v�{�[�h�Ɏc��
                Clipboard.SetText(copy_text);
            }
        }

        /// <summary>
        /// ���ʑŌ������X�g�r���[�̑I�����ڂ��ύX���ꂽ�Ƃ��ɌĂяo�����
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
            
            // �I���������ڂ��Ȃ��ꍇ�ł��\����0�ɖ߂����߂�return���Ȃ�
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
        /// ���ʑŌ������X�g�r���[�̍��ڂ�"�E"�N���b�N�����Ƃ��ɌĂяo�����
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

                // �����̃f�[�^�ȊO�̓��O�ɕۑ�����Ă�����̂�ǂݍ���
                if (date.ToString(Plugin.LogDir.DAY_FORMAT) != DateTime.Now.ToString(Plugin.LogDir.DAY_FORMAT))
                {
                    // �t�@�C�������݂��Ȃ��Ă����v
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
                Console.WriteLine("���ёւ�����");
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
                        
                        //  sub_menu.MenuItems.CopyTo(menu_items,0)����������Ƃ�
                        // �ȉ��̃X�e�b�v�����s����ƁC�Ȃ���sub_menu��MenuItems��������ۂɂȂ�
                        // ��̂悤��ClonMenu()���g���Ƒ��v�D�D�D�Ȃ����낤�H
                        menu.MenuItems[menu.MenuItems.Count - 1].MenuItems.AddRange(menu_items);

                        menu.MenuItems[menu.MenuItems.Count - 1].MenuItems[menu_items.Length - 1].Tag = sub_menu;
                    }
                }
                menu.MenuItems.Add("-");
                menu.MenuItems.Add("�N���b�v�{�[�h�ɃR�s�[����(&C)");
                menu.MenuItems[menu.MenuItems.Count - 1].Tag = menu;
                menu.MenuItems[menu.MenuItems.Count - 1].Click += new EventHandler(Title_Menu_Click);

                menu.Show(listView2, new Point(e.X + 10, e.Y + 20));
            }
        }

        /// <summary>
        /// ���ʑŌ������X�g�r���[���ō��ڂ̏���}�E�X�h���b�O�����Ƃ��ɌĂяo�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView2_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = listView2.HitTest(e.X, e.Y);
            if (info.SubItem != null && e.Button == MouseButtons.Left)
            {
                info.Item.Selected = true;
                // 24�̓��X�g�r���[�w�b�_�[�̍����C17�̓A�C�e���̍���
                // �萔�ɂ���̂��Ȃ񂩂��ꂾ�����̂ŁD�D
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
        /// �v���Z�X�ʑŌ������X�g�r���[����Ctrl+A���������Ƃ��ɌĂяo�����
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
        /// ���ʑŌ������X�g�r���[����Ctrl+A���������Ƃ��ɌĂяo�����
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
        /// ���X�g�r���[���̃A�C�e�������ׂđI������
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

            // ���O���ύX���ꂽ�Ƃ��������X�g�r���[�̏��������ƕۑ����s��
            if (old_name != e.NewName)
            {
                strokeNumLog.ProcessName.SetName(e.Path, e.NewName);
                strokeNumLog.ProcessName.Save();
                keyStrokeView.ProcessViewNameUpdate(e.Path, e.NewName);
            }
        }

        private void ��Ɏ�O�ɕ\������ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            AppConfig.TopMost = ��Ɏ�O�ɕ\������ToolStripMenuItem.Checked;
            this.TopMost = AppConfig.TopMost;
        }

        private void �v���O�C���ꗗVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViewPluginForm form = new ViewPluginForm(pluginController);
            form.ShowDialog(this);
        }

        #region �^�X�N�g���C�֌W...
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
        /// �^�X�N�g���C�A�C�R���̃R���e�L�X�g���j���[���N���b�N�����Ƃ��ɌĂяo�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void �J��OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowOpen();
        }

        /// <summary>
        /// �^�X�N�g���C�A�C�R���̃R���e�L�X�g���j���[���N���b�N�����Ƃ��ɌĂяo�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ����CToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void �ݒ�CToolStripMenuItem_Click(object sender, EventArgs e)
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
                        "�L�[���͊Ď����@�̐ݒ肪�ύX����܂����B\n" +
                        "�ݒ�͍ċN����ɗL���ɂȂ�܂��B");
                }
            }
        }

        /// <summary>
        /// �v���Z�X�ʑŌ����r���[�ō��ڏ���h���b�O�����Ƃ��ɌĂ΂��
        /// ���{�^���������Ȃ���}�E�X���ړ����������̍��ڂ�I������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = listView1.HitTest(e.X, e.Y);
            if (e.Button == MouseButtons.Left && info.SubItem != null)
            {
                info.Item.Selected = true;
                // 24�̓��X�g�r���[�w�b�_�[�̍����C17�̓A�C�e���̍���
                // �萔�ɂ���̂��Ȃ񂩂��ꂾ�����̂ŁD�D
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

        private void �I���������ڂ��R�s�[CToolStripMenuItem_Click(object sender, EventArgs e)
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
                // �N���b�v�{�[�h�ɕ�������R�s�[����
                // �A�v���P�[�V�����I������N���b�v�{�[�h�Ɏc��
                Clipboard.SetText(copy_text.ToString());
            }
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
