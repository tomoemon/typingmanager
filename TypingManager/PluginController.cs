using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Plugin;
using System.Windows.Forms;
using System.Xml;

namespace TypingManager
{
    public class PluginController : IStrokePluginController
    {
        /// <summary>
        /// <�v���O�C���̃A�N�Z�X��, ���X�g���̂ǂ��ɂ��邩>
        /// ���O�ŌĂяo����悤�Ɏ����`���ŕێ�
        /// </summary>
        Dictionary<string, int> index_dic = new Dictionary<string, int>();

        List<IStrokePlugin> plugin_list = new List<IStrokePlugin>();

        private Form main_form;

        public PluginController(Form form)
        {
            main_form = form;
        }

        public void Load()
        {
            //IPlugin�^�̖��O���擾
            string ipluginName = typeof(IStrokePlugin).FullName;

            //�C���X�g�[������Ă���v���O�C���𒲂ׂ�
            PluginInfo[] pis = PluginInfo.FindPlugins(ipluginName);

            //���ׂẴv���O�C���̃C���X�^���X���쐬����
            for (int i = 0; i < pis.Length; i++)
            {
                IStrokePlugin plugin = (IStrokePlugin)pis[i].CreateInstance();
                string name = plugin.GetAccessName();
                string author = plugin.GetAuthorName();
                string version = plugin.GetVersion();
                if (name != "" && author != "" && version != "")
                {
                    plugin.Controller = this;
                    plugin.MainForm = main_form;
                    plugin.Valid = true;
                    plugin_list.Add(plugin);
                    index_dic[name] = plugin_list.Count - 1;
                    GetSaveDir(name);   // �v���O�C���p�f�B���N�g���̍쐬
                    Console.WriteLine("Plugin[{0}]: {1}", i, name);
                }
            }
            LoadPluginConfig();
        }

        /// <summary>�L�[�������ꂽ�Ƃ��ɌĂяo�����</summary>
        /// <param name="keycode">�����ꂽ�L�[�̉��z�L�[�R�[�h</param>
        /// <param name="militime">�L�[�������ꂽ����[�~���b]�iOS���N�����Ă���̌o�ߎ��ԁj</param>
        /// <param name="app_path">�L�[�������ꂽ�A�v���P�[�V�����̃t���p�X</param>
        /// <param name="app_title">�L�[�������ꂽ�E�B���h�E�̃^�C�g��</param>
        public void KeyDown(IKeyState key_state, int militime, string app_path, string app_title)
        {
            foreach (IStrokePlugin plugin in plugin_list)
            {
                if (plugin.Valid == true)
                {
                    plugin.KeyDown(key_state, militime, app_path, app_title);
                }
            }
        }

        /// <summary>�L�[���オ�����Ƃ��ɌĂяo�����</summary>
        public void KeyUp(IKeyState key_state, int militime, string app_path, string app_title)
        {
            foreach (IStrokePlugin plugin in plugin_list)
            {
                if (plugin.Valid == true)
                {
                    plugin.KeyUp(key_state, militime, app_path, app_title);
                }
            }
        }

        public void Init()
        {
            foreach (IStrokePlugin plugin in plugin_list)
            {
                plugin.Init();
            }
        }

        public void Close()
        {
            foreach (IStrokePlugin plugin in plugin_list)
            {
                plugin.Close();
            }
            SavePluginConfig();
        }

        public void Add(IStrokePlugin plugin)
        {
            string name = plugin.GetAccessName();
            if (name != "")
            {
                plugin.Valid = true;
                plugin_list.Add(plugin);
                index_dic[name] = plugin_list.Count - 1;
            }
        }

        /// <summary>
        /// ���C�����j���[�̃v���O�C�����ɕ\�����郁�j���[��Ԃ�
        /// </summary>
        /// <param name="parent_menu"></param>
        public void AddMenu(ToolStripMenuItem parent_menu)
        {
            int i = 1;
            foreach (IStrokePlugin plugin in plugin_list)
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

        public List<IStrokePlugin> GetPluginList()
        {
            return plugin_list;
        }

        /// <summary>
        /// �v���O�C���̃��O��ۑ�����f�B���N�g����Ԃ�
        /// ��{�I�ɂ�log�t�H���_�̒��Ɂu�v���O�C�����v�t�H���_���쐬���Ă��̒���
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
        /// ��{�I�ɑ��̃v���O�C������C�ʂ̃v���O�C���̏��𓾂邽�߂ɌĂ΂��
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetInfo(string name)
        {
            if (index_dic.ContainsKey(name))
            {
                return plugin_list[index_dic[name]].GetInfo();
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
                writer.WriteStartElement("PluginList");
                foreach (IStrokePlugin plugin in plugin_list)
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

                XmlNodeList node_list = doc.SelectNodes("//Plugin");
                foreach (XmlNode plugin_node in node_list)
                {
                    XmlAttributeCollection plugin_attrs = plugin_node.Attributes;
                    bool valid = bool.Parse(plugin_attrs["valid"].Value);
                    string plugin_name = plugin_attrs["plugin_name"].Value;
                    string access_name = plugin_attrs["access_name"].Value;
                    if (index_dic.ContainsKey(access_name))
                    {
                        plugin_list[index_dic[access_name]].Valid = valid;
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
