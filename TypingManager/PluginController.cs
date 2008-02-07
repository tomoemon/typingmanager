using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Plugin;
using System.Windows.Forms;

namespace TypingManager
{
    public class PluginController : IStrokePluginController
    {
        /// <summary>
        /// �v���O�C���̌Ăяo�������͂��܂�C�ɂ��Ȃ��Ă悢�̂�
        /// �����`���ŕێ����Ă���
        /// </summary>
        Dictionary<string, IStrokePlugin> plugin_dic;

        private Form main_form;

        public PluginController(Form form)
        {
            plugin_dic = new Dictionary<string, IStrokePlugin>();
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
                    plugin_dic[name] = plugin;
                    Console.WriteLine("Plugin[{0}]: {1}", i, name);
                }
            }

        }

        /// <summary>�L�[�������ꂽ�Ƃ��ɌĂяo�����</summary>
        /// <param name="keycode">�����ꂽ�L�[�̉��z�L�[�R�[�h</param>
        /// <param name="militime">�L�[�������ꂽ����[�~���b]�iOS���N�����Ă���̌o�ߎ��ԁj</param>
        /// <param name="app_path">�L�[�������ꂽ�A�v���P�[�V�����̃t���p�X</param>
        /// <param name="app_title">�L�[�������ꂽ�E�B���h�E�̃^�C�g��</param>
        public void KeyDown(int keycode, int militime, string app_path, string app_title)
        {
            foreach (IStrokePlugin plugin in plugin_dic.Values)
            {
                if (plugin.Valid == true)
                {
                    plugin.KeyDown(keycode, militime, app_path, app_title);
                }
            }
        }

        /// <summary>�L�[���オ�����Ƃ��ɌĂяo�����</summary>
        public void KeyUp(int keycode, int militime, string app_path, string app_title)
        {
            foreach (IStrokePlugin plugin in plugin_dic.Values)
            {
                if (plugin.Valid == true)
                {
                    plugin.KeyUp(keycode, militime, app_path, app_title);
                }
            }
        }

        public void Close()
        {
            foreach (IStrokePlugin plugin in plugin_dic.Values)
            {
                plugin.Close();
            }
        }

        public void Add(string name, IStrokePlugin plugin)
        {
            plugin_dic[name] = plugin;
        }

        /// <summary>
        /// ���C�����j���[�̃v���O�C�����ɕ\�����郁�j���[��Ԃ�
        /// </summary>
        /// <param name="parent_menu"></param>
        public void AddMenu(ToolStripMenuItem parent_menu)
        {
            int i = 1;
            foreach (IStrokePlugin plugin in plugin_dic.Values)
            {
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
            return new List<IStrokePlugin>(plugin_dic.Values);
        }

        public string GetSaveDir(string plugin_name)
        {
            if (plugin_dic.ContainsKey(plugin_name))
            {
                string dir = Path.GetDirectoryName(Application.ExecutablePath);
                string path = Path.Combine(Path.Combine(dir, LogDir.PLUGIN_DIR), plugin_name);
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
            if (plugin_dic.ContainsKey(name))
            {
                return plugin_dic[name].GetInfo();
            }
            return null;
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
