using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace AnalyzePlugin
{
    public interface IStrokePluginController
    {
        /// <summary>
        /// �v���O�C������Ă΂��֐��C���̃v���O�C���Ɋւ������Ԃ��Ă�����
        /// </summary>
        /// <param name="name">���̗~�����v���O�C���̖��O</param>
        /// <returns></returns>
        object GetInfo(string name);
    }

    public interface IStrokePlugin
    {
        /// <summary>�v���O�C���̖��O��Ԃ�����</summary>
        string GetPluginName();

        /// <summary>�v���O�C���Ɋւ���ȒP�Ȑ�������������</summary>
        string GetComment();

        /// <summary>�v���O�C����҂̖��O��Ԃ�����</summary>
        string GetAuthorName();

        /// <summary>�v���O�C���̃o�[�W��������������</summary>
        string GetVersion();

        /// <summary>�L�[�������ꂽ�Ƃ��ɌĂяo�����</summary>
        /// <param name="keycode">�����ꂽ�L�[�̉��z�L�[�R�[�h</param>
        /// <param name="militime">�L�[�������ꂽ����[�~���b]�iOS���N�����Ă���̌o�ߎ��ԁj</param>
        /// <param name="app_path">�L�[�������ꂽ�A�v���P�[�V�����̃t���p�X</param>
        /// <param name="app_title">�L�[�������ꂽ�E�B���h�E�̃^�C�g��</param>
        void KeyDown(int keycode, int militime, string app_path, string app_title);

        /// <summary>�L�[���オ�����Ƃ��ɌĂяo�����</summary>
        void KeyUp(int keycode, int militime, string app_path, string app_title);
        
        /// <summary>
        /// ���̃v���O�C���ł��g�������n�������Ƃ��Ɏg��
        /// IStrokePluginController����Ă΂��
        /// </summary>
        /// <returns></returns>
        object GetInfo();

        /// <summary>���C�����j���[�ɉ����郁�j���[��Ԃ�</summary>
        List<Menu> GetMainMenu();

        IStrokePluginController Controller
        {
            get;set;
        }
    }
}
