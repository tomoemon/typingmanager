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
        /// 受け取った数にそれぞれ大きい順，小さい順の番号をつけてリストとして返す
        /// </summary>
        /// <param name="copy_num"></param>
        /// <returns></returns>
        public static List<int> SortOrder(List<int> data, bool descend)
        {
            // 元のに影響を与えないためにコピーする
            int[] copy_data = new int[data.Count];
            data.CopyTo(copy_data);

            List<int> result = new List<int>();
            for (int i = 0; i < copy_data.Length; i++)
            {
                result.Add(i);
            }
            
            // 大きい順に並び替え
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
    /// リストビュー上でアイテムを編集するためのテキストボックス
    /// </summary>
    public class ListViewInputBox : TextBox
    {
        public class InputEventArgs : EventArgs
        {
            public string Path = "";
            public string NewName = "";
        }

        public delegate void InputEventHandler(object sender, InputEventArgs e);

        //イベントデリゲートの宣言
        public event InputEventHandler FinishInput;

        private InputEventArgs args = new InputEventArgs();
        private bool finished = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">対象となるListViewコントロール</param>
        /// <param name="item">編集対象のアイテム</param>
        /// <param name="subitem_index">編集する対象の列</param>
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
            // Enterで入力を完了した場合はKeyDownが呼ばれた後に
            // さらにLostFocusが呼ばれるため，二回Finishが呼ばれる
            if (!finished)
            {
                // finished = true => textbox.Hide()の順に呼ぶと大丈夫だが
                // textbox.Hide() => finished = true と書くとここのブロックが
                // 二回呼ばれてしまう（ReleaseでもDebugでも）．．謎だ．．
                finished = true;
                this.Hide();
                args.NewName = new_name;
                FinishInput(this, args);
            }
        }

        void textbox_KeyDown(object sender, KeyEventArgs e)
        {
            // Enterが押されたら入力を確定
            // Escapeが押されたら入力をキャンセル
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
