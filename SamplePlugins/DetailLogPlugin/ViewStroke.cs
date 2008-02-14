using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Plugin;

namespace DetailLogPlugin
{
    public class ViewStroke
    {
        class Stroke
        {
            public Stroke(IKeyState _key, uint _militime)
            {
                key = _key;
                militime = _militime;
                data = string.Format("{0}, {1}, {2}", militime.ToString().PadLeft(5, ' '),
                    key.KeyCode.ToString().PadLeft(3, ' '), key.KeyName);
            }
            IKeyState key;
            uint militime;
            string data;

            public override string ToString()
            {
                return data;
            }
        }

        private int NewLineLength = Environment.NewLine.Length;
        private uint first_event_time = 0;
        private int max_log_num = 10;
        private TextBox up_text;
        private TextBox down_text;

        // 一番上の行を消す際に"\n"を検索して削除するのは効率が悪いので
        // テキストボックスの現在の一行目の文字数を保持しておく
        private int first_up_length = 0;
        private int first_down_length = 0;

        LinkedList<Stroke> up_list = new LinkedList<Stroke>();
        LinkedList<Stroke> down_list = new LinkedList<Stroke>();

        public ViewStroke(TextBox _up_text, TextBox _down_text, int max)
        {
            up_text = _up_text;
            down_text = _down_text;
            max_log_num = max;
        }

        public int MaxLogNum
        {
            get { return max_log_num; }
            set { max_log_num = value; }
        }
        public int UpCount
        {
            get { return up_list.Count; }
        }
        public int DownCount
        {
            get { return down_list.Count; }
        }
        public string GetFirstUp()
        {
            return up_list.First.Value.ToString();
        }
        public string GetFirstDown()
        {
            return down_list.First.Value.ToString();
        }
        public string GetLastUp()
        {
            return up_list.Last.Value.ToString();
        }
        public string GetLastDown()
        {
            return down_list.Last.Value.ToString();
        }

        public void KeyUp(IKeyState key_state, uint militime)
        {
            if (max_log_num == 0)
            {
                return;
            }
            if (first_event_time == 0)
            {
                first_event_time = militime;
            }
            militime -= first_event_time;

            up_list.AddLast(new Stroke(key_state, militime));
            string line = GetLastUp();
            up_text.Text += line + Environment.NewLine;
            if (UpCount == 1)
            {
                first_up_length = line.Length;
            }
            while (UpCount > max_log_num)
            {
                up_list.RemoveFirst();
                up_text.Text = up_text.Text.Substring(first_up_length + NewLineLength);
                first_up_length = GetFirstUp().Length;
            }
            up_text.SelectionStart = up_text.Text.Length - 1;
            up_text.ScrollToCaret();
        }

        public void KeyDown(IKeyState key_state, uint militime)
        {
            if (max_log_num == 0)
            {
                return;
            }
            if (!key_state.IsPush(key_state.KeyCode))
            {
                return;
            }
            if (first_event_time == 0)
            {
                first_event_time = militime;
            }
            militime -= first_event_time;
            //Console.WriteLine("first:{0}, time:{1}", first_event_time, militime);

            down_list.AddLast(new Stroke(key_state, militime));
            string line = GetLastDown();
            down_text.Text += line + Environment.NewLine;
            if (DownCount == 1)
            {
                first_down_length = line.Length;
            }
            while(DownCount > max_log_num)
            {
                down_list.RemoveFirst();
                down_text.Text = down_text.Text.Substring(first_down_length + NewLineLength);
                first_down_length = GetFirstDown().Length;
            }
            down_text.SelectionStart = down_text.Text.Length - 1;
            down_text.ScrollToCaret();
        }

        public void Clear()
        {
            down_list.Clear();
            up_list.Clear();
            down_text.Text = "";
            up_text.Text = "";
        }
    }
}
