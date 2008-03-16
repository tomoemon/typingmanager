using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Plugin;

namespace Plugin
{
    public class BaseStrokePlugin : IStrokePlugin
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
        public virtual void KeyDown(IKeyState keycode, uint militime, string app_path, string app_title) { }

        /// <summary>�L�[���オ�����Ƃ��ɌĂяo�����</summary>
        public virtual void KeyUp(IKeyState keycode, uint militime, string app_path, string app_title) { }

        /// <summary>�f�B���N�g���`�F�b�N�Ȃǂ̏���������</summary>
        public virtual void Init() { }

        /// <summary>���O�ۑ��Ȃǂ̏I������</summary>
        public virtual void Close() { }

        /// <summary>�v���O�C���ݒ�p�̃t�H�[���������Ă��邩</summary>
        public virtual bool IsHasConfigForm() { return false; }

        /// <summary>�v���O�C���ݒ�p�̃t�H�[����\��</summary>
        public virtual void ShowConfigForm() { }

        /// <summary>���[�U���ݒ肵�������ۑ����̃^�C�~���O�ɌĂ΂��</summary>
        public virtual void AutoSave() { }

        /// <summary>
        /// ���̃v���O�C���ł��g�������n�������Ƃ��Ɏg��
        /// IStrokePluginController����Ă΂��
        /// </summary>
        /// <returns></returns>
        public virtual object GetInfo() { return null; }

        /// <summary>���C�����j���[�ɉ����郁�j���[��Ԃ�</summary>
        public virtual List<ToolStripMenuItem> GetToolStripMenu() { return null; }

        public IPluginController Controller
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

        private IPluginController _controller;
        private bool _valid;
        private Form _mainform;
    }
}
