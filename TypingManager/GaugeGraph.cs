using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace TypingManager
{
    public class GaugeGraph : Graph
    {
        // キューの中に入れておける過去の値の数
        const int MAX_QUEUE = 1024;

        // ゲージの個数
        const int GAUGE_NUM = 10;

        // ゲージごとの余白
        const int GAUGE_MARGIN = 1;

        private float max;
        private float min;
        private List<float> values;

        #region プロパティ
        public float Max
        {
            get { return max; }
            set { max = value; }
        }
        public float Min
        {
            get { return min; }
            set { min = value; }
        }
        #endregion

        public GaugeGraph(int width, int height, Rect rect)
            : base(width, height, rect)
        {
            Init();
        }

        public GaugeGraph(int width, int height)
            : base(width, height)
        {
            Init();
        }

        private void Init()
        {
            min = 0f;
            max = 1000f;
            values = new List<float>(MAX_QUEUE);
        }

        public void SetValue(float value)
        {
            values.Add(value);
            
            // サンプルとして保持しておくサイズの最大に達したら先頭半分を削除する
            if (values.Count >= MAX_QUEUE)
            {
                values.RemoveRange(0, MAX_QUEUE / 2);
            }
        }

        public int GaugeNum(float value)
        {
            // maxとminの中で今の値は何％になっているかを調べる
            float percent = 0;
            if (value > min)
            {
                percent = (float)(value - min) / (max - min);
            }

            // 離散化したゲージの中で何個目まで達しているか
            int gauge_value = (int)(GAUGE_NUM * percent);
            if (gauge_value > GAUGE_NUM)
            {
                gauge_value = GAUGE_NUM;
            }
            return gauge_value;
        }

        private int GetGaugeHeight()
        {
            // 描画できる範囲はdraw_rect
            int drawable_height = DrawRect.Height - GAUGE_MARGIN * (GAUGE_NUM - 1);
            if (drawable_height < GAUGE_NUM)
            {
                drawable_height = DrawRect.Height;
            }
            // 一つのゲージ当たりの高さ
            int gauge_height = drawable_height / GAUGE_NUM;
            //Debug.WriteLine("DrawRectHeight:{0}, DrawableHeight:{1}", DrawRect.Height, drawable_height);
            return gauge_height;
        }

        private void DrawGauges(Graphics g, Brush brush, int gauge_value)
        {
            int gauge_height = GetGaugeHeight();
            
            for (int i = 0; i < gauge_value; i++)
            {
                g.FillRectangle(brush, DrawRect.Left,
                    DrawRect.Top + gauge_height * (GAUGE_NUM - i) + GAUGE_MARGIN * (GAUGE_NUM - i),
                    DrawRect.Width, gauge_height);
            }
        }

        public override void DrawFrame()
        {
            // bmpに設定されているイメージのGraphicsオブジェクトを取得
            Graphics g = Graphics.FromImage(Bmp);
            g.FillRectangle(Brushes.Black, 0, 0, Bmp.Width, Bmp.Height);

            float now = values[values.Count-1];
            int gauge_value = GaugeNum(now);
            DrawGauges(g, Brushes.Green, GAUGE_NUM);
            DrawGauges(g, Brushes.Lime, gauge_value);
            
            //Graphicsを破棄する
            g.Dispose();
        }
    }
}
