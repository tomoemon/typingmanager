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
        private ListViewItem last_selected = null;
        private List<IFilterPlugin> filter_plugin_list;
        private List<IStrokePlugin> stroke_plugin_list;

        public ViewPluginForm(PluginController plugin_ctrl)
        {
            InitializeComponent();

            controller = plugin_ctrl;

            listView1.SmallImageList = new ImageList();
            listView1.SmallImageList.ImageSize = new Size(1, Form1.LISTVEW_SMALL_ICON_SIZE);
            listView1.Columns[1].Width = listView1.Width - listView1.Columns[0].Width - Form1.LISTVIEW_RMARGIN;
            listView1.ListViewItemSorter = new NumSort(0);
            listView2.SmallImageList = new ImageList();
            listView2.SmallImageList.ImageSize = new Size(1, Form1.LISTVEW_SMALL_ICON_SIZE);
            listView2.Columns[1].Width = listView2.Width - listView2.Columns[0].Width - Form1.LISTVIEW_RMARGIN;
            listView2.ListViewItemSorter = new NumSort(0);

            filter_plugin_list = controller.GetFilterPluginList();
            stroke_plugin_list = controller.GetStrokePluginList();

            LoadPluginName();
        }

        private void LoadPluginName()
        {
            int item_num = 0;
            foreach (IFilterPlugin plugin in filter_plugin_list)
            {
                string plugin_name = plugin.GetPluginName();
                string access_name = plugin.GetAccessName();
                listView1.Items.Add(access_name, (item_num+1).ToString(), "");
                listView1.Items[item_num].SubItems.Add(plugin_name);
                listView1.Items[item_num].Tag = plugin;
                listView1.Items[item_num].Checked = plugin.Valid;
                if (plugin.Valid)
                {
                    listView1.Items[item_num].BackColor = VALID_COLOR;
                }
                else
                {
                    listView1.Items[item_num].BackColor = INVALID_COLOR;
                }
                item_num++;
            }
            item_num = 0;
            foreach (IStrokePlugin plugin in stroke_plugin_list)
            {
                string plugin_name = plugin.GetPluginName();
                string access_name = plugin.GetAccessName();
                listView2.Items.Add(access_name, (item_num + 1).ToString(), "");
                listView2.Items[item_num].SubItems.Add(plugin_name);
                listView2.Items[item_num].Tag = plugin;
                listView2.Items[item_num].Checked = plugin.Valid;
                if (plugin.Valid)
                {
                    listView2.Items[item_num].BackColor = VALID_COLOR;
                }
                else
                {
                    listView2.Items[item_num].BackColor = INVALID_COLOR;
                }
                item_num++;
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewSelectChanged(listView1);
        }

        private void ListViewSelectChanged(ListView view)
        {
            if (view.SelectedIndices.Count == 0)
            {
                textBox1.Text = "";
                textBox5.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
                button1.Enabled = false;
                last_selected = null;
            }
            else
            {
                int selected = view.SelectedIndices[0];
                ListViewItem item = view.SelectedItems[0];
                last_selected = item;
                IPluginBase plugin = (IPluginBase)item.Tag;
                textBox1.Text = plugin.GetPluginName();
                textBox5.Text = plugin.GetAccessName();
                textBox2.Text = plugin.GetAuthorName();
                textBox3.Text = plugin.GetVersion();
                textBox4.Text = plugin.GetComment();
                if (plugin.IsHasConfigForm())
                {
                    button1.Enabled = true;
                }
                else
                {
                    button1.Enabled = false;
                }
            }
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
            ItemCheckedChange(e.Item);
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewSelectChanged(listView2);
        }

        private void ItemCheckedChange(ListViewItem item)
        {
            IPluginBase plugin = (IPluginBase)item.Tag;
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

        private void listView2_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ItemCheckedChange(e.Item);
        }

        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView(listView2, e.Column);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (last_selected == null) return;

            IPluginBase plugin = (IPluginBase)last_selected.Tag;
            plugin.ShowConfigForm();
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            ListView view = listView1;
            List<IFilterPlugin> plugin_list = filter_plugin_list;
            if (view.SelectedItems.Count == 0) return;

            ListViewItem select_item = view.SelectedItems[0];
            int select_order = int.Parse(select_item.SubItems[0].Text);

            if (e.KeyCode == Keys.Right)
            {
                if (select_order < plugin_list.Count)
                {
                    IFilterPlugin temp = plugin_list[select_order - 1];
                    plugin_list[select_order - 1] = plugin_list[select_order];
                    plugin_list[select_order] = temp;

                    foreach (ListViewItem item in view.Items)
                    {
                        int last_order = int.Parse(item.SubItems[0].Text);
                        if (select_order + 1 == last_order)
                        {
                            item.SubItems[0].Text = select_order.ToString();
                            select_item.SubItems[0].Text = (select_order + 1).ToString();
                            break;
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.Left)
            {
                if (select_order > 1)
                {
                    IFilterPlugin temp = plugin_list[select_order - 1];
                    plugin_list[select_order - 1] = plugin_list[select_order - 2];
                    plugin_list[select_order - 2] = temp;

                    foreach (ListViewItem item in view.Items)
                    {
                        int last_order = int.Parse(item.SubItems[0].Text);
                        if (select_order - 1 == last_order)
                        {
                            item.SubItems[0].Text = select_order.ToString();
                            select_item.SubItems[0].Text = (select_order - 1).ToString();
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < plugin_list.Count; i++)
            {
                Console.WriteLine(plugin_list[i].GetAccessName());
            }
        }

        private void listView2_KeyDown(object sender, KeyEventArgs e)
        {
            ListView view = listView2;
            List<IStrokePlugin> plugin_list = stroke_plugin_list;
            if (view.SelectedItems.Count == 0) return;

            ListViewItem select_item = view.SelectedItems[0];
            int select_order = int.Parse(select_item.SubItems[0].Text);

            if (e.KeyCode == Keys.Right)
            {
                if (select_order < plugin_list.Count)
                {
                    IStrokePlugin temp = plugin_list[select_order - 1];
                    plugin_list[select_order - 1] = plugin_list[select_order];
                    plugin_list[select_order] = temp;

                    foreach (ListViewItem item in view.Items)
                    {
                        int last_order = int.Parse(item.SubItems[0].Text);
                        if (select_order + 1 == last_order)
                        {
                            item.SubItems[0].Text = select_order.ToString();
                            select_item.SubItems[0].Text = (select_order + 1).ToString();
                            break;
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.Left)
            {
                if (select_order > 1)
                {
                    IStrokePlugin temp = plugin_list[select_order - 1];
                    plugin_list[select_order - 1] = plugin_list[select_order - 2];
                    plugin_list[select_order - 2] = temp;

                    foreach (ListViewItem item in view.Items)
                    {
                        int last_order = int.Parse(item.SubItems[0].Text);
                        if (select_order - 1 == last_order)
                        {
                            item.SubItems[0].Text = select_order.ToString();
                            select_item.SubItems[0].Text = (select_order - 1).ToString();
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < plugin_list.Count; i++)
            {
                Console.WriteLine(plugin_list[i].GetAccessName());
            }
        }

    }
}
