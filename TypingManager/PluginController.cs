using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Drawing;
using Plugin;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

namespace TypingManager
{
    /// <summary>
    /// �t�B���^�����O���s���v���O�C�����Ǘ�����
    /// KeyDown��KeyUp���Ăяo���ꂽ���ɁC���ꂼ��̓��͏���
    /// ���Ԃɂ��ׂẴt�B���^�[�ɓK�p���Ă���
    /// </summary>
    public class FilterController : IFilterPluginController
    {
        List<IFilterPlugin> filter_plugin_list = new List<IFilterPlugin>();
        Dictionary<string, int> plugin_order = new Dictionary<string, int>();
        private PluginController controller;

        public FilterController(PluginController _controller)
        {
            controller = _controller;
        }

        #region �v���p�e�B...
        public int Count
        {
            get { return filter_plugin_list.Count; }
        }
        #endregion

        public List<IFilterPlugin> GetFilterPluginList()
        {
            return filter_plugin_list;
        }

        public void AddFilterPlugin(IFilterPlugin plugin)
        {
            plugin.FilterController = this;
            plugin_order[plugin.GetAccessName()] = filter_plugin_list.Count;
            filter_plugin_list.Add(plugin);
        }

        /// <summary>
        /// PluginController����Ă΂��֐�
        /// �ŏ��̃t�B���^�[�v���O�C�����N��������
        /// </summary>
        /// <param name="key_state"></param>
        /// <param name="militime"></param>
        /// <param name="app_path"></param>
        /// <param name="app_title"></param>
        public void KeyDown(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            int first_order = 0;
            while (first_order < Count && !filter_plugin_list[first_order].Valid)
            {
                ++first_order;
            }
            if (first_order < Count)
            {
                filter_plugin_list[first_order].KeyDown(key_state, militime, app_path, app_title);
            }
            else
            {
                controller.FilteredKeyDown(key_state, militime, app_path, app_title);
            }
        }
        
        /// <summary>
        /// PluginController����Ă΂��֐�
        /// �ŏ��̃t�B���^�[�v���O�C�����N��������
        /// </summary>
        /// <param name="key_state"></param>
        /// <param name="militime"></param>
        /// <param name="app_path"></param>
        /// <param name="app_title"></param>
        public void KeyUp(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            int first_order = 0;
            while (first_order < Count && !filter_plugin_list[first_order].Valid)
            {
                ++first_order;
            }
            if (first_order < Count)
            {
                filter_plugin_list[first_order].KeyUp(key_state, militime, app_path, app_title);
            }
            else
            {
                controller.FilteredKeyUp(key_state, militime, app_path, app_title);
            }
        }

        /// <summary>
        /// �e�t�B���^�[�v���O�C������Ă΂��
        /// �󂯎�����f�[�^�͎��̃t�B���^�[�����ۂ̃f�[�^�L�^�v���O�C���ɓn��
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key_state"></param>
        /// <param name="militime"></param>
        /// <param name="app_path"></param>
        /// <param name="app_title"></param>
        public void FilteredKeyDown(IFilterPlugin plugin, IKeyState key_state, uint militime,
            string app_path, string app_title)
        {
            int next_order = plugin_order[plugin.GetAccessName()] + 1;
            while (next_order < Count && !filter_plugin_list[next_order].Valid)
            {
                ++next_order;
            }
            if (next_order < Count)
            {
                filter_plugin_list[next_order].KeyDown(key_state, militime, app_path, app_title);
            }
            else
            {
                controller.FilteredKeyDown(key_state, militime, app_path, app_title);
            }
        }

        /// <summary>
        /// �e�t�B���^�[�v���O�C������Ă΂��
        /// �󂯎�����f�[�^�͎��̃t�B���^�[�����ۂ̃f�[�^�L�^�v���O�C���ɓn��
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key_state"></param>
        /// <param name="militime"></param>
        /// <param name="app_path"></param>
        /// <param name="app_title"></param>
        public void FilteredKeyUp(IFilterPlugin plugin, IKeyState key_state, uint militime,
            string app_path, string app_title)
        {
            int next_order = plugin_order[plugin.GetAccessName()] + 1;
            while (next_order < Count && !filter_plugin_list[next_order].Valid)
            {
                ++next_order;
            }
            if (next_order < Count)
            {
                filter_plugin_list[next_order].KeyUp(key_state, militime, app_path, app_title);
            }
            else
            {
                controller.FilteredKeyUp(key_state, militime, app_path, app_title);
            }
        }
    }

    public class PluginController : IStrokePluginController
    {
        /// <summary>
        /// ���v���O�C���̃A�N�Z�X��, ���X�g���̂ǂ��ɂ��邩��
        /// ���O�ŌĂяo����悤�Ɏ����`���ŕێ�
        /// </summary>
        Dictionary<string, IPluginBase> index_dic = new Dictionary<string, IPluginBase>();
        List<IStrokePlugin> stroke_plugin_list = new List<IStrokePlugin>();
        FilterController filter_controller;
        
        private Form main_form;

        public PluginController(Form form)
        {
            main_form = form;
            filter_controller = new FilterController(this);
        }


        public void Load()
        {
            LoadFilterPlugin();
            LoadStrokePlugin();
            LoadPluginConfig();
            Init();
        }

        private void LoadFilterPlugin()
        {
            // IFilterPlugin�^�̖��O���擾
            string ipluginName = typeof(IFilterPlugin).FullName;

            // �C���X�g�[������Ă���v���O�C���𒲂ׂ�
            PluginInfo[] pis = PluginInfo.FindPlugins(ipluginName);

            // ���ׂẴv���O�C���̃C���X�^���X���쐬����
            for (int i = 0; i < pis.Length; i++)
            {
                IFilterPlugin plugin = (IFilterPlugin)pis[i].CreateInstance();
                AddFilterPlugin(plugin);
            }
        }

        private void LoadStrokePlugin()
        {
            // IStrokePlugin�^�̖��O���擾
            string ipluginName = typeof(IStrokePlugin).FullName;

            // �C���X�g�[������Ă���v���O�C���𒲂ׂ�
            PluginInfo[] pis = PluginInfo.FindPlugins(ipluginName);

            // ���ׂẴv���O�C���̃C���X�^���X���쐬����
            for (int i = 0; i < pis.Length; i++)
            {
                IStrokePlugin plugin = (IStrokePlugin)pis[i].CreateInstance();
                AddStrokePlugin(plugin);
            }
        }

        /// <summary>�L�[�������ꂽ�Ƃ��ɌĂяo�����</summary>
        /// <param name="key_state">�����ꂽ�L�[�̉��z�L�[�R�[�h</param>
        /// <param name="militime">�L�[�������ꂽ����[�~���b]�iOS���N�����Ă���̌o�ߎ��ԁj</param>
        /// <param name="app_path">�L�[�������ꂽ�A�v���P�[�V�����̃t���p�X</param>
        /// <param name="app_title">�L�[�������ꂽ�E�B���h�E�̃^�C�g��</param>
        public void KeyDown(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            filter_controller.KeyDown(key_state, militime, app_path, app_title);
        }

        /// <summary>
        /// �L�[���オ�����Ƃ��ɌĂяo�����
        /// </summary>
        /// <param name="key_state"></param>
        /// <param name="militime"></param>
        /// <param name="app_path"></param>
        /// <param name="app_title"></param>
        public void KeyUp(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            filter_controller.KeyUp(key_state, militime, app_path, app_title);
        }

        public void FilteredKeyDown(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            foreach (IStrokePlugin plugin in stroke_plugin_list)
            {
                if (plugin.Valid == true)
                {
                    plugin.KeyDown(key_state, militime, app_path, app_title);
                }
            }
        }

        public void FilteredKeyUp(IKeyState key_state, uint militime, string app_path, string app_title)
        {
            foreach (IStrokePlugin plugin in stroke_plugin_list)
            {
                if (plugin.Valid == true)
                {
                    plugin.KeyUp(key_state, militime, app_path, app_title);
                }
            }
        }

        public void Init()
        {
            foreach (IPluginBase plugin in index_dic.Values)
            {
                plugin.Init();
            }
        }

        public void Close()
        {
            foreach (IPluginBase plugin in index_dic.Values)
            {
                plugin.Close();
            }
            SavePluginConfig();
        }

        public void AddFilterPlugin(IFilterPlugin plugin)
        {
            string name = plugin.GetAccessName();
            if (name != "" && !index_dic.ContainsKey(name))
            {
                plugin.Controller = this;
                plugin.MainForm = main_form;
                //plugin.Valid = false; // �N�����̗L���E�����̓v���O�C�������̐ݒ�ɔC����
                filter_controller.AddFilterPlugin(plugin);
                index_dic[name] = plugin;
                Debug.WriteLine(string.Format("FilterPlugin: {0}", name));
            }
        }

        public void AddStrokePlugin(IStrokePlugin plugin)
        {
            string name = plugin.GetAccessName();
            if (name != "" && !index_dic.ContainsKey(name))
            {
                plugin.Controller = this;
                plugin.MainForm = main_form;
                //plugin.Valid = true; // �N�����̗L���E�����̓v���O�C�������̐ݒ�ɔC����
                stroke_plugin_list.Add(plugin);
                index_dic[name] = plugin;
                Debug.WriteLine(string.Format("StrokePlugin: {0}", name));
            }
        }

        /// <summary>
        /// ���C�����j���[�̃v���O�C�����ɕ\�����郁�j���[��Ԃ�
        /// </summary>
        /// <param name="parent_menu"></param>
        public void AddMenu(ToolStripMenuItem parent_menu)
        {
            int i = 1;
            foreach (IPluginBase plugin in stroke_plugin_list)
            {
                if (plugin.GetToolStripMenu() == null) continue;

                string menu_name;
                if (i <= 9)
                {
                    menu_name = string.Format("{0}(&{1})", plugin.GetPluginName(), i);
                }
                else
                {
                    menu_name = plugin.GetPluginName();
                }
                ToolStripMenuItem item = new ToolStripMenuItem(menu_name);
                foreach (ToolStripMenuItem menu_item in plugin.GetToolStripMenu())
                {
                    item.DropDownItems.Add(menu_item);
                }
                parent_menu.DropDownItems.Add(item);
                i++;
            }
            foreach (IPluginBase plugin in filter_controller.GetFilterPluginList())
            {
                if (plugin.GetToolStripMenu() == null) continue;

                string menu_name;
                if (i <= 9)
                {
                    menu_name = string.Format("{0}(&{1})", plugin.GetPluginName(), i);
                }
                else
                {
                    menu_name = plugin.GetPluginName();
                }
                ToolStripMenuItem item = new ToolStripMenuItem(menu_name);
                foreach (ToolStripMenuItem menu_item in plugin.GetToolStripMenu())
                {
                    item.DropDownItems.Add(menu_item);
                }
                parent_menu.DropDownItems.Add(item);
                i++;
            }
        }

        public List<IStrokePlugin> GetStrokePluginList()
        {
            return stroke_plugin_list;
        }

        public List<IFilterPlugin> GetFilterPluginList()
        {
            return filter_controller.GetFilterPluginList();
        }

        /// <summary>
        /// �v���O�C���̃��O��ۑ�����f�B���N�g����Ԃ�
        /// ��{�I�ɂ�log/�t�H���_�̒��Ɂu�v���O�C�����v�t�H���_���쐬���Ă��̒���
        /// </summary>
        /// <param name="plugin_name"></param>
        /// <returns></returns>
        public string GetSaveDir(string plugin_name)
        {
            if (index_dic.ContainsKey(plugin_name))
            {
                string path = Path.Combine(LogDir.LOG_DIR, plugin_name);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
            return "";
        }

        /// <summary>
        /// �v���O�C���̐ݒ��ۑ�����f�B���N�g����Ԃ�
        /// ��{�I�ɂ�config/�t�H���_�̒��Ɂu�v���O�C�����v�t�H���_���쐬���Ă��̒���
        /// </summary>
        /// <param name="plugin_name"></param>
        /// <returns></returns>
        public string GetConfigDir(string plugin_name)
        {
            if (index_dic.ContainsKey(plugin_name))
            {
                string path = Path.Combine(LogDir.CONFIG_DIR, plugin_name);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
            return "";
        }


        /// <summary>
        /// ��{�I�ɑ��̃v���O�C������C�ʂ̃v���O�C���̏��𓾂邽�߂ɌĂ΂��
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetInfo(string name)
        {
            if (index_dic.ContainsKey(name))
            {
                return index_dic[name].GetInfo();
            }
            return null;
        }

        private void SavePluginConfig()
        {
            string filename = LogDir.PLUGIN_CONFIG_FILE;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            XmlWriter writer = XmlWriter.Create(filename, settings);
            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("PluginConfig");
                writer.WriteStartElement("StrokePluginList");
                foreach (IPluginBase plugin in stroke_plugin_list)
                {
                    writer.WriteStartElement("Plugin");
                    writer.WriteAttributeString("plugin_name", "", plugin.GetPluginName());
                    writer.WriteAttributeString("access_name", "", plugin.GetAccessName());
                    writer.WriteAttributeString("valid", "", plugin.Valid.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteStartElement("FilterPluginList");
                foreach (IPluginBase plugin in filter_controller.GetFilterPluginList())
                {
                    writer.WriteStartElement("Plugin");
                    writer.WriteAttributeString("plugin_name", "", plugin.GetPluginName());
                    writer.WriteAttributeString("access_name", "", plugin.GetAccessName());
                    writer.WriteAttributeString("valid", "", plugin.Valid.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            finally
            {
                writer.Close();
            }
        }

        private void LoadPluginConfig()
        {
            string filename = LogDir.PLUGIN_CONFIG_FILE;

            if (File.Exists(filename))
            {
                string xml = "";
                using (StreamReader sr = new StreamReader(filename))
                {
                    xml = sr.ReadToEnd();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                // ���݂�StrokePlugin�̏��Ԃ�ǂݏo��
                Dictionary<string, int> plugin_order = new Dictionary<string, int>();
                for (int i = 0; i < stroke_plugin_list.Count; i++)
                {
                    plugin_order[stroke_plugin_list[i].GetAccessName()] = i;
                }

                // StrokePlugin�Ɋւ���ݒ�̓ǂݍ���
                XmlNodeList node_list = doc.SelectNodes("PluginConfig/StrokePluginList/Plugin");
                Console.WriteLine("StrokePluginList �m�[�h��:{0}", node_list.Count);
                for(int i=0; i<node_list.Count; i++)
                {
                    XmlNode plugin_node = node_list[i];
                    XmlAttributeCollection plugin_attrs = plugin_node.Attributes;
                    bool valid = bool.Parse(plugin_attrs["valid"].Value);
                    string plugin_name = plugin_attrs["plugin_name"].Value;
                    string access_name = plugin_attrs["access_name"].Value;
                    
                    if (index_dic.ContainsKey(access_name))
                    {
                        index_dic[access_name].Valid = valid;
                        if (i < plugin_order[access_name])
                        {
                            IStrokePlugin temp = stroke_plugin_list[i];
                            stroke_plugin_list[i] = stroke_plugin_list[plugin_order[access_name]];
                            stroke_plugin_list[plugin_order[access_name]] = temp;
                            /*
                            Console.WriteLine("{0}��{1}�̌���", i, plugin_order[access_name]);
                            foreach (IStrokePlugin p in stroke_plugin_list)
                            {
                                Console.WriteLine(p.GetAccessName());
                            }
                             */
                        }
                    }
                }
                plugin_order.Clear();

                // ���݂�FilterPlugin�̏��Ԃ�ǂݏo��
                List<IFilterPlugin> filter_plugin_list = filter_controller.GetFilterPluginList();
                for (int i = 0; i < filter_plugin_list.Count; i++)
                {
                    plugin_order[filter_plugin_list[i].GetAccessName()] = i;
                }

                // FilterPlugin�Ɋւ���ݒ�̓ǂݍ���
                node_list = doc.SelectNodes("PluginConfig/FilterPluginList/Plugin");
                Console.WriteLine("FilterPluginList �m�[�h��:{0}", node_list.Count);
                for (int i = 0; i < node_list.Count; i++)
                {
                    XmlNode plugin_node = node_list[i];
                    XmlAttributeCollection plugin_attrs = plugin_node.Attributes;
                    bool valid = bool.Parse(plugin_attrs["valid"].Value);
                    string plugin_name = plugin_attrs["plugin_name"].Value;
                    string access_name = plugin_attrs["access_name"].Value;

                    if (index_dic.ContainsKey(access_name))
                    {
                        index_dic[access_name].Valid = valid;
                        if (i < plugin_order[access_name])
                        {
                            IFilterPlugin temp = filter_plugin_list[i];
                            filter_plugin_list[i] = filter_plugin_list[plugin_order[access_name]];
                            filter_plugin_list[plugin_order[access_name]] = temp;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// �v���O�C���Ɋւ�����
    /// </summary>
    public class PluginInfo
    {
        private string _location;
        private string _className;

        /// <summary>
        /// PluginInfo�N���X�̃R���X�g���N�^
        /// </summary>
        /// <param name="path">�A�Z���u���t�@�C���̃p�X</param>
        /// <param name="cls">�N���X�̖��O</param>
        private PluginInfo(string path, string cls)
        {
            _location = path;
            _className = cls;
        }

        /// <summary>
        /// �A�Z���u���t�@�C���̃p�X
        /// </summary>
        public string Location
        {
            get { return _location; }
        }

        /// <summary>
        /// �N���X�̖��O
        /// </summary>
        public string ClassName
        {
            get { return _className; }
        }

        /// <summary>
        /// �L���ȃv���O�C����T��
        /// </summary>
        /// <returns>�L���ȃv���O�C����PluginInfo�z��</returns>
        public static PluginInfo[] FindPlugins(string ipluginName)
        {
            ArrayList plugins = new ArrayList();

            //�v���O�C���t�H���_
            string folder = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath),
                                            LogDir.PLUGIN_DIR);

            if (!Directory.Exists(folder))
            {
                throw new ApplicationException(
                    "�v���O�C���t�H���_\"" + folder + "\"��������܂���ł����B"
                    );
            }
            //.dll�t�@�C����T��
            string[] dlls = Directory.GetFiles(folder, "*.dll");

            foreach (string dll in dlls)
            {
                try
                {
                    //�A�Z���u���Ƃ��ēǂݍ���
                    Assembly asm = Assembly.LoadFrom(dll);
                    foreach (Type t in asm.GetTypes())
                    {
                        //�A�Z���u�����̂��ׂĂ̌^�ɂ��āA
                        //�v���O�C���Ƃ��ėL�������ׂ�
                        if (t.IsClass && t.IsPublic && !t.IsAbstract &&
                            t.GetInterface(ipluginName) != null)
                        {
                            //PluginInfo���R���N�V�����ɒǉ�����
                            plugins.Add(new PluginInfo(dll, t.FullName));
                        }
                    }
                }
                catch
                {
                    throw new ApplicationException(
                        "�v���O�C���̓ǂݍ��ݒ��ɖ�肪�������܂����B"
                    );
                }
            }

            //�R���N�V������z��ɂ��ĕԂ�
            return (PluginInfo[])plugins.ToArray(typeof(PluginInfo));
        }

        /// <summary>
        /// �v���O�C���N���X�̃C���X�^���X���쐬����
        /// </summary>
        /// <returns>�v���O�C���N���X�̃C���X�^���X</returns>
        public object CreateInstance()
        {
            try
            {
                //�A�Z���u����ǂݍ���
                Assembly asm = Assembly.LoadFrom(this.Location);

                //�N���X������C���X�^���X���쐬����
                return asm.CreateInstance(this.ClassName);
            }
            catch
            {
                return null;
            }
        }
    }
}
