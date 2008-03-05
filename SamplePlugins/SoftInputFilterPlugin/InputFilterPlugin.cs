using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Plugin;

namespace SoftInputFilterPlugin
{
    public class InputFilterPlugin : IFilterPlugin
    {
        public static string CONFIG_DIR = "";
        public static string LOG_DIR = "";

        private IFilterPluginController filter_controller;
        private IPluginController plugin_controller;
        private bool valid = false;
        private Form main_form = null;
        private bool form_open = false;
        private InputFilterPluginForm form;

        #region プロパティ...
        public bool FormOpen
        {
            get { return form_open; }
            set { form_open = value; }
        }
        #endregion

        #region IFilterPlugin メンバ
        public IFilterPluginController FilterController
        {
            get { return filter_controller; }
            set { filter_controller = value; }
        }
        #endregion

        #region IPluginBase メンバ
        public void Close()
        {
            
        }

        public IPluginController Controller
        {
            get { return plugin_controller; }
            set { plugin_controller = value; }
        }

        public string GetAccessName()
        {
            return "soft_filter";
        }

        public string GetAuthorName()
        {
            return "tomoemon";
        }

        public string GetComment()
        {
            return "ソフトウェアによる入力を推測してフィルタリングを行います";
        }

        public object GetInfo()
        {
            return null;
        }

        public string GetPluginName()
        {
            return "ソフト入力フィルター";
        }

        public string GetVersion()
        {
            return "0.0.1";
        }

        public List<ToolStripMenuItem> GetToolStripMenu()
        {
            List<ToolStripMenuItem> menu_item = new List<ToolStripMenuItem>();
            ToolStripMenuItem item = new ToolStripMenuItem("設定(&C)...");
            item.Click += new EventHandler(item_Click);
            menu_item.Add(item);
            return menu_item;
        }

        void item_Click(object sender, EventArgs e)
        {
            ShowConfigForm();
        }

        public void Init()
        {
            CONFIG_DIR = Controller.GetConfigDir(this.GetAccessName());
            LOG_DIR = Controller.GetSaveDir(this.GetAccessName());
        }

        public bool IsHasConfigForm()
        {
            return true;
        }

        public void KeyDown(IKeyState keystate, uint militime, string app_path, string app_title)
        {
            if (!keystate.IsShift)
            {
                filter_controller.FilteredKeyDown(this, keystate, militime, app_path, app_title);
            }
        }

        public void KeyUp(IKeyState keystate, uint militime, string app_path, string app_title)
        {
            if (keystate.IsShift || keystate.KeyCode == (int)Keys.LShiftKey || keystate.KeyCode == (int)Keys.RShiftKey)
            {
                return;
            }
            filter_controller.FilteredKeyUp(this, keystate, militime, app_path, app_title);
        }

        public Form MainForm
        {
            get { return main_form; }
            set { main_form = value; }
        }

        public void ShowConfigForm()
        {
            if (!FormOpen)
            {
                FormOpen = true;
                form = new InputFilterPluginForm(this);
                //Console.WriteLine("x={0}, y={1}", MainForm.Location.X, MainForm.Location.Y);
                form.Location = MainForm.Location;
                form.Show();
            }
        }

        public bool Valid
        {
            get { return valid; }
            set { valid = value; }
        }
        #endregion
    }
}
