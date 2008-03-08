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
            set { NumericValueSet(numericUpDown1, value); }
        }
        public int NoStrokeLimitTime
        {
            get { return (int)numericUpDown2.Value; }
            set { NumericValueSet(numericUpDown2, value); }
        }
        public int MinStrokeTimeSpeed
        {
            get { return (int)numericUpDown3.Value; }
            set { NumericValueSet(numericUpDown3, value); }
        }
        public int MaxStrokeTimeSpeed
        {
            get { return (int)numericUpDown4.Value; }
            set { NumericValueSet(numericUpDown4, value); }
        }
        public bool UseStandardHook
        {
            get { return checkBox2.Checked; }
            set { checkBox2.Checked = value; }
        }
        public bool GetAppPathByTitleChange
        {
            get { return checkBox3.Checked; }
            set { checkBox3.Checked = value; }
        }
        public bool ShowExitMessage
        {
            get { return checkBox4.Checked; }
            set { checkBox4.Checked = value; }
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

        private void NumericValueSet(NumericUpDown updown, int value)
        {
            if (updown.Maximum < value)
            {
                updown.Value = updown.Maximum;
            }
            else if (updown.Minimum > value)
            {
                updown.Value = updown.Minimum;
            }
            else
            {
                updown.Value = value;
            }
        }
    }
}