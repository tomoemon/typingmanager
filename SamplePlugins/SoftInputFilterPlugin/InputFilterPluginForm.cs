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
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            // *起動時の処理
            // フィルタ定義の読み込み
            // このプラグインの設定ファイルの読み込み
            // 指定されているフィルタの適用

            // *フィルタ適用時のキー入力処理

            // *フィルタを詳細ログに適用する処理
        }

        private void InputFilterPluginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            plugin.FormOpen = false;
        }

        private void InputFilterPluginForm_Load(object sender, EventArgs e)
        {

        }
    }
}