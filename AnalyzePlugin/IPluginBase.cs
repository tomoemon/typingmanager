using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Plugin
{
    public interface IPluginBase
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
        /// <param name="keycode">�����ꂽ�L�[�̏��</param>
        /// <param name="militime">�L�[�������ꂽ����[�~���b]�iOS���N�����Ă���̌o�ߎ��ԁj</param>
        /// <param name="app_path">�L�[�������ꂽ�A�v���P�[�V�����̃t���p�X</param>
        /// <param name="app_title">�L�[�������ꂽ�E�B���h�E�̃^�C�g��</param>
        void KeyDown(IKeyState keystate, uint militime, string app_path, string app_title);

        /// <summary>�L�[���オ�����Ƃ��ɌĂяo�����</summary>
        void KeyUp(IKeyState keystate, uint militime, string app_path, string app_title);

        /// <summary>
        /// �f�B���N�g���`�F�b�N�Ȃǂ̏���������
        /// Controller�CMainForm�̏�񂪃Z�b�g����Ă���Ă΂��
        /// �e�v���O�C�����Ă΂�鏇�Ԃ͌��܂��Ă��Ȃ�
        /// </summary>
        void Init();

        /// <summary>
        /// ���O�ۑ��Ȃǂ̏I������
        /// �A�v���P�[�V�����I�����O�ɌĂ΂��
        /// </summary>
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

        IPluginController Controller
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
}
