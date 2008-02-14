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
            return "";
        }

        public List<ToolStripMenuItem> GetToolStripMenu()
        {
            return null;
        }

        public void Init()
        {
            CONFIG_DIR = Controller.GetConfigDir(this.GetAccessName());
            LOG_DIR = Controller.GetSaveDir(this.GetAccessName());
        }

        public bool IsHasConfigForm()
        {
            return false;
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
            
        }

        public bool Valid
        {
            get { return valid; }
            set { valid = value; }
        }
        #endregion
    }
}
