using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Plugin;

namespace DetailLogPlugin
{
    public class TriggerKey : IComparable<TriggerKey>, ICloneable
    {
        public int KeyCode;
        public bool Ctrl;
        public bool Shift;
        public bool Alt;

        public TriggerKey(Keys key, Keys modifier)
        {
            if ((modifier & Keys.Control) == Keys.Control)
            {
                Console.WriteLine("Control");
                Ctrl = true;
            }
            if ((modifier & Keys.Alt) == Keys.Alt)
            {
                Console.WriteLine("Alt");
                Alt = true;
            }
            if ((modifier & Keys.Shift) == Keys.Shift)
            {
                Console.WriteLine("Shift");
                Shift = true;
            }
            KeyCode = (int)key;
        }

        public TriggerKey(string format)
        {
            SetSaveFormat(format);
        }

        public override string ToString()
        {
            return GetViewFormat();
        }

        public void SetSaveFormat(string format)
        {
            string[] items = format.Split(new char[] { '+' });
            for (int i = 0; i < items.Length; i++)
            {
                int key = int.Parse(items[i]);
                if (key == (int)Keys.ControlKey)
                {
                    Ctrl = true;
                }
                else if (key == (int)Keys.ShiftKey)
                {
                    Shift = true;
                }
                else if (key == (int)Keys.Menu)
                {
                    Alt = true;
                }
                else
                {
                    KeyCode = key;
                }
            }
        }

        public string GetViewFormat()
        {
            string result = "";
            if (Ctrl) result += "Ctrl+";
            if (Shift) result += "Shift+";
            if (Alt) result += "Alt+";
            result += VirtualKeyName.GetKeyName(KeyCode).ToString();
            return result;
        }

        public string GetSaveFormat()
        {
            StringBuilder result = new StringBuilder();
            if (Ctrl)
            {
                result.Append(((int)Keys.ControlKey).ToString());
                result.Append("+");
            }
            if (Shift)
            {
                result.Append(((int)Keys.ShiftKey).ToString());
                result.Append("+");
            }
            if (Alt)
            {
                result.Append(((int)Keys.Menu).ToString());
                result.Append("+");
            }
            result.Append(KeyCode.ToString());
            return result.ToString();
        }

        public int CompareTo(TriggerKey other)
        {
            if (KeyCode == other.KeyCode && Ctrl == other.Ctrl
                && Shift == other.Shift && Alt == other.Alt)
            {
                return 0;
            }
            Console.WriteLine("false in TriggerKey");
            return -1;
        }

        public bool Same(IKeyState state)
        {
            if (KeyCode == state.KeyCode && Shift == state.IsShift
                && Alt == state.IsAlt && Ctrl == state.IsControl)
            {
                return true;
            }
            return false;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class TriggerSequence : IComparable<TriggerSequence>, ICloneable
    {
        private List<TriggerKey> trigger = new List<TriggerKey>();

        private int index = 0;

        #region プロパティ...
        public List<TriggerKey> Sequence
        {
            get { return trigger; }
        }
        public TriggerKey this[int index]
        {
            get { return trigger[index]; }
        }
        public int Count
        {
            get { return trigger.Count; }
        }
        #endregion

        public TriggerSequence()
        {
        }

        public void Reset()
        {
            index = 0;
        }

        public void Add(Keys key, Keys modifier)
        {
            if (key == Keys.Back)
            {
                Back();
            }
            else
            {
                trigger.Add(new TriggerKey(key, modifier));
            }
        }

        public void Back()
        {
            if (trigger.Count > 0)
            {
                trigger.RemoveAt(trigger.Count - 1);
            }
        }

        public override string ToString()
        {
            return GetViewFormat();
        }

        public string GetViewFormat()
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < this.Count; i++)
            {
                result.Append(trigger[i].GetViewFormat());
                if (i != this.Count - 1)
                {
                    result.Append(" ");
                }
            }
            return result.ToString();
        }

        public string GetSaveFormat()
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < this.Count; i++)
            {
                result.Append(trigger[i].GetSaveFormat());
                if (i != this.Count - 1)
                {
                    result.Append(",");
                }
            }
            return result.ToString();
        }

        public void SetSaveFormat(string format)
        {
            string[] items = format.Split(new char[] { ',' });
            for (int i = 0; i < items.Length; i++)
            {
                TriggerKey key = new TriggerKey(items[i]);
                trigger.Add(key);
            }
        }

        public int CompareTo(TriggerSequence other)
        {
            if (Sequence.Count.CompareTo(other.Sequence.Count) != 0)
            {
                Console.WriteLine("false in TriggerSequence");
                return -1;
            }

            for (int i = 0; i < Sequence.Count; i++)
            {
                if (Sequence[i].CompareTo(other.Sequence[i]) != 0)
                {
                    Console.WriteLine("false in TriggerSequence");
                    return -1;
                }
            }
            return 0;
        }

        /// <summary>
        /// ショートカットシーケンスが登録してあるものと一致するかチェックする
        /// キーワードとして「end」が登録してある場合
        /// hogeと打った後にendと打っても認識されるようにしてある
        /// 厳密には
        /// hogeeeeeeeeeeeeend
        /// とキーワードの一部を連続して打っても認識される仕様になっている
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool IsInvoke(IKeyState state)
        {
            if (this[index].Same(state))
            {
                ++index;
                if (index == this.Count)
                {
                    index = 0;
                    return true;
                }
            }
            else if(index > 0 && !this[index-1].Same(state))
            {
                index = 0;
            }
            return false;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
