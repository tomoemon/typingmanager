using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Plugin
{
    public interface IPluginController
    {
        /// <summary>
        /// ���̃v���O�C���Ɋւ������Ԃ�
        /// �v���O�C������Ă΂��֐�
        /// </summary>
        /// <param name="name">���̗~�����v���O�C���̖��O</param>
        /// <returns></returns>
        object GetInfo(string name);

        /// <summary>
        /// �e�v���O�C�������O�̕ۑ��Ɏg���f�B���N�g��
        /// �v���O�C������Ă΂��֐�
        /// </summary>
        /// <returns></returns>
        string GetSaveDir(string pluginname);

        /// <summary>
        /// �e�v���O�C�����ݒ�̕ۑ��Ɏg���f�B���N�g��
        /// �v���O�C������Ă΂��֐�
        /// </summary>
        /// <returns></returns>
        string GetConfigDir(string pluginname);
    }

    public interface IStrokePluginController : IPluginController
    {
    }

    public interface IFilterPluginController
    {
        void FilteredKeyUp(IFilterPlugin plugin, IKeyState keycode, uint militime,
            string app_path, string app_title);
        void FilteredKeyDown(IFilterPlugin plugin, IKeyState keycode, uint militime,
            string app_path, string app_title);
    }
}
