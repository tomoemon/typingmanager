using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace TypingManager
{
    /// <summary>
    /// �ڍ׃��M���O���s���Ă���Ƃ��ɉ�ʂ̍X�V��S������N���X
    /// </summary>
    public class KeyEventView
    {
        private Form1 main_form;

        public KeyEventView(Form1 main_form)
        {
            this.main_form = main_form;
        }

        /// <summary>
        /// �L�[�̉��������C�x���g�����������Ƃ���GUI�R���g���[���̒l��ύX����
        /// </summary>
        /// <param name="updown"></param>
        /// <param name="vkey"></param>
        /// <param name="scan"></param>
        /// <param name="app_path"></param>
        public void Update(KeyboardUpDown updown, int vkey, int scan, string app_path)
        {
            main_form.LastEventType.Text = updown.ToString();
            main_form.LastKeyCode.Text = vkey.ToString();
            main_form.LastScanCode.Text = scan.ToString();
            main_form.LastAppPath.Text = Path.GetFileName(app_path);
        }
    }
}
