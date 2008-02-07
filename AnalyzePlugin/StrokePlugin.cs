using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Plugin
{
    public interface IStrokePluginController
    {
        /// <summary>
        /// ���̃v���O�C���Ɋւ������Ԃ�
        /// �v���O�C������Ă΂��֐�
        /// </summary>
        /// <param name="name">���̗~�����v���O�C���̖��O</param>
        /// <returns></returns>
        object GetInfo(string name);

        /// <summary>
        /// �e�v���O�C�����ۑ��Ɏg���Ă��ǂ��f�B���N�g����Ԃ��D
        /// �v���O�C������Ă΂��֐�
        /// </summary>
        /// <returns></returns>
        string GetSaveDir(string pluginname);
    }

    public interface IStrokePlugin
    {
        /// <summary>�v���O�C���̖��O��Ԃ�����(�p����)</summary>
        string GetPluginName();

        /// <summary>���C���t�H�[���̃��j���[�ɒǉ����邽�߂̓��{��̖��O��Ԃ�����</summary>
        string GetAccessName();

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

        /// <summary>���O�ۑ��Ȃǂ̏I������</summary>
        void Close();

        /// <summary>�v���O�C���ݒ�p�̃t�H�[���������Ă��邩</summary>
        bool IsHasConfigForm();

        /// <summary>�v���O�C���ݒ�p�̃t�H�[����\��</summary>
        void ShowConfigForm();

        /// <summary>
        /// ���̃v���O�C���ł��g�������n�������Ƃ��Ɏg��
        /// IStrokePluginController����Ă΂��
        /// </summary>
        /// <returns></returns>
        object GetInfo();

        /// <summary>���C�����j���[�ɉ����郁�j���[��Ԃ�</summary>
        List<ToolStripMenuItem> GetToolStripMenu();

        IStrokePluginController Controller
        {
            get;
            set;
        }

        /// <summary>���݃v���O�C�����L���ɂȂ��Ă��邩�ǂ�����Ԃ�</summary>
        bool Valid
        {
            get;
            set;
        }

        /// <summary>���C���t�H�[���̏����󂯎��</summary>
        Form MainForm
        {
            get;
            set;
        }
    }

    public class BaseStrokePlugin :IStrokePlugin
    {
        /// <summary>�v���O�C���̖��O��Ԃ�����</summary>
        public virtual string GetPluginName() { return ""; }

        public virtual string GetAccessName() { return ""; }

        /// <summary>�v���O�C���Ɋւ���ȒP�Ȑ�������������</summary>
        public virtual string GetComment() { return ""; }

        /// <summary>�v���O�C����҂̖��O��Ԃ�����</summary>
        public virtual string GetAuthorName() { return ""; }

        /// <summary>�v���O�C���̃o�[�W��������������</summary>
        public virtual string GetVersion() { return ""; }

        /// <summary>�L�[�������ꂽ�Ƃ��ɌĂяo�����</summary>
        /// <param name="keycode">�����ꂽ�L�[�̉��z�L�[�R�[�h</param>
        /// <param name="militime">�L�[�������ꂽ����[�~���b]�iOS���N�����Ă���̌o�ߎ��ԁj</param>
        /// <param name="app_path">�L�[�������ꂽ�A�v���P�[�V�����̃t���p�X</param>
        /// <param name="app_title">�L�[�������ꂽ�E�B���h�E�̃^�C�g��</param>
        public virtual void KeyDown(int keycode, int militime, string app_path, string app_title) { }

        /// <summary>�L�[���オ�����Ƃ��ɌĂяo�����</summary>
        public virtual void KeyUp(int keycode, int militime, string app_path, string app_title) { }

        /// <summary>���O�ۑ��Ȃǂ̏I������</summary>
        public virtual void Close(){}

        /// <summary>�v���O�C���ݒ�p�̃t�H�[���������Ă��邩</summary>
        public virtual bool IsHasConfigForm() { return false; }

        /// <summary>�v���O�C���ݒ�p�̃t�H�[����\��</summary>
        public virtual void ShowConfigForm() { }

        /// <summary>
        /// ���̃v���O�C���ł��g�������n�������Ƃ��Ɏg��
        /// IStrokePluginController����Ă΂��
        /// </summary>
        /// <returns></returns>
        public virtual object GetInfo() { return null; }

        /// <summary>���C�����j���[�ɉ����郁�j���[��Ԃ�</summary>
        public virtual List<ToolStripMenuItem> GetToolStripMenu() { return null; }

        public IStrokePluginController Controller
        {
            get { return _controller; }
            set { _controller = value; }
        }

        public bool Valid
        {
            get { return _valid; }
            set { _valid = value; }
        }

        public Form MainForm
        {
            get { return _mainform; }
            set { _mainform = value; }
        }

        private IStrokePluginController _controller;
        private bool _valid;
        private Form _mainform;
    }
}
