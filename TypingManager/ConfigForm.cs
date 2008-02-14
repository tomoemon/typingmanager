using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TypingManager
{
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();
        }

        #region プロパティ...
        public bool ScheduledLogging
        {
            get { return checkBox1.Checked; }
            set { checkBox1.Checked = value; }
        }
        public int ScheduleLogTiming
        {
            get { return (int)numericUpDown1.Value; }
            set { numericUpDown1.Value = value; }
        }
        public int NoStrokeLimitTime
        {
            get { return (int)numericUpDown2.Value; }
            set { numericUpDown2.Value = value; }
        }
        public bool ShowExitMessage
        {
            get { return checkBox2.Checked; }
            set { checkBox2.Checked = value; }
        }
        public string SelectedItemCopyFormat
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }
        public string RightClickCopyFormat
        {
            get { return textBox2.Text; }
            set { textBox2.Text = value; }
        }
        #endregion
    }
}