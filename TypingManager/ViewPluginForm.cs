using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Plugin;

namespace TypingManager
{
    public partial class ViewPluginForm : Form
    {
        private PluginController controller;
        private Color INVALID_COLOR = Color.Pink;
        private Color VALID_COLOR = Color.White;

        public ViewPluginForm(PluginController plugin_ctrl)
        {
            InitializeComponent();

            controller = plugin_ctrl;

            listView1.SmallImageList = new ImageList();
            listView1.SmallImageList.ImageSize = new Size(1, Form1.LISTVEW_SMALL_ICON_SIZE);
            listView1.Columns[0].Width = listView1.Width - Form1.LISTVIEW_RMARGIN;
            listView1.ListViewItemSorter = new NumSort();

            LoadPluginName();
        }

        private void LoadPluginName()
        {
            foreach (IStrokePlugin plugin in controller.GetPluginList())
            {
                string plugin_name = plugin.GetPluginName();
                string access_name = plugin.GetAccessName();
                listView1.Items.Add(access_name, plugin_name, "");
                listView1.Items[listView1.Items.Count - 1].Tag = plugin;
                listView1.Items[listView1.Items.Count - 1].Checked = plugin.Valid;
                if (plugin.Valid)
                {
                    listView1.Items[listView1.Items.Count - 1].BackColor = VALID_COLOR;
                }
                else
                {
                    listView1.Items[listView1.Items.Count - 1].BackColor = INVALID_COLOR;
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
            {
                textBox1.Text = "";
                textBox5.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
                button3.Enabled = false;
            }
            else
            {
                int selected = listView1.SelectedIndices[0];
                ListViewItem item = listView1.SelectedItems[0];
                IStrokePlugin plugin = (IStrokePlugin)item.Tag;
                textBox1.Text = plugin.GetPluginName();
                textBox5.Text = plugin.GetAccessName();
                textBox2.Text = plugin.GetAuthorName();
                textBox3.Text = plugin.GetVersion();
                textBox4.Text = plugin.GetComment();
                if (plugin.IsHasConfigForm())
                {
                    button3.Enabled = true;
                }
                else
                {
                    button3.Enabled = false;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0) return;

            int selected = listView1.SelectedIndices[0];
            ListViewItem item = listView1.SelectedItems[0];
            IStrokePlugin plugin = (IStrokePlugin)item.Tag;
            plugin.ShowConfigForm();
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

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ListViewItem item = e.Item;
            IStrokePlugin plugin = (IStrokePlugin)item.Tag;
            plugin.Valid = item.Checked;
            if (plugin.Valid)
            {
                item.BackColor = VALID_COLOR;
            }
            else
            {
                item.BackColor = INVALID_COLOR;
            }
        }
    }
}
