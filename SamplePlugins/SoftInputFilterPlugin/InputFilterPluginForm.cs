using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Plugin;

namespace SoftInputFilterPlugin
{
    public partial class InputFilterPluginForm : Form
    {
        private InputFilterPlugin plugin;

        public InputFilterPluginForm(InputFilterPlugin plugin)
        {
            InitializeComponent();

            this.Icon = plugin.MainForm.Icon;
            this.plugin = plugin;
        }

        private void InputFilterPluginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            plugin.FormOpen = false;
        }
    }
}