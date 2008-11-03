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

            // *�N�����̏���
            // �t�B���^��`�̓ǂݍ���
            // ���̃v���O�C���̐ݒ�t�@�C���̓ǂݍ���
            // �w�肳��Ă���t�B���^�̓K�p

            // *�t�B���^�K�p���̃L�[���͏���

            // *�t�B���^���ڍ׃��O�ɓK�p���鏈��
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