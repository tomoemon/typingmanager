using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TypingManager
{
    public class Misc
    {
        /// <summary>
        /// �󂯎�������ɂ��ꂼ��傫�����C���������̔ԍ������ă��X�g�Ƃ��ĕԂ�
        /// </summary>
        /// <param name="copy_num"></param>
        /// <returns></returns>
        public static List<int> SortOrder(List<int> data, bool descend)
        {
            // ���̂ɉe����^���Ȃ����߂ɃR�s�[����
            int[] copy_data = new int[data.Count];
            data.CopyTo(copy_data);

            List<int> result = new List<int>();
            for (int i = 0; i < copy_data.Length; i++)
            {
                result.Add(i);
            }
            
            // �傫�����ɕ��ёւ�
            for (int i = 0; i < copy_data.Length; i++)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (copy_data[j] < copy_data[j + 1])
                    {
                        int temp = copy_data[j];
                        copy_data[j] = copy_data[j + 1];
                        copy_data[j + 1] = temp;
                        int temp_order = result[j];
                        result[j] = result[j + 1];
                        result[j + 1] = temp_order;
                    }
                }
            }

            if (!descend)
            {
                result.Reverse();
            }
            return result;
        }
    }

    /// <summary>
    /// ���X�g�r���[��ŃA�C�e����ҏW���邽�߂̃e�L�X�g�{�b�N�X
    /// </summary>
    public class ListViewInputBox : TextBox
    {
        public class InputEventArgs : EventArgs
        {
            public string Path = "";
            public string NewName = "";
        }

        public delegate void InputEventHandler(object sender, InputEventArgs e);

        //�C�x���g�f���Q�[�g�̐錾
        public event InputEventHandler FinishInput;

        private InputEventArgs args = new InputEventArgs();
        private bool finished = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">�ΏۂƂȂ�ListView�R���g���[��</param>
        /// <param name="item">�ҏW�Ώۂ̃A�C�e��</param>
        /// <param name="subitem_index">�ҏW����Ώۂ̗�</param>
        public ListViewInputBox(ListView parent, ListViewItem item, int subitem_index) : base()
        {
            args.Path = item.SubItems[0].Text;
            args.NewName = item.SubItems[1].Text;

            int left = 0;
            for (int i = 0; i < subitem_index; i++)
            {
                left += parent.Columns[i].Width;
            }
            int width = item.SubItems[subitem_index].Bounds.Width;
            int height = item.SubItems[subitem_index].Bounds.Height - 4;

            this.Parent = parent;
            this.Size = new Size(width, height);
            this.Left = left;
            this.Top = item.Position.Y - 1;
            this.Text = item.SubItems[subitem_index].Text;
            this.LostFocus += new EventHandler(textbox_LostFocus);
            this.ImeMode = ImeMode.NoControl;
            this.Multiline = false;
            this.KeyDown += new KeyEventHandler(textbox_KeyDown);
            this.Focus();
        }

        void Finish(string new_name)
        {
            // Enter�œ��͂����������ꍇ��KeyDown���Ă΂ꂽ���
            // �����LostFocus���Ă΂�邽�߁C���Finish���Ă΂��
            if (!finished)
            {
                // finished = true => textbox.Hide()�̏��ɌĂԂƑ��v����
                // textbox.Hide() => finished = true �Ə����Ƃ����̃u���b�N��
                // ���Ă΂�Ă��܂��iRelease�ł�Debug�ł��j�D�D�䂾�D�D
                finished = true;
                this.Hide();
                args.NewName = new_name;
                FinishInput(this, args);
            }
        }

        void textbox_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter�������ꂽ����͂��m��
            // Escape�������ꂽ����͂��L�����Z��
            if (e.KeyCode == Keys.Enter)
            {
                Finish(this.Text);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Finish(args.NewName);
            }
        }

        void textbox_LostFocus(object sender, EventArgs e)
        {
            Finish(this.Text);
        }
    }
}
