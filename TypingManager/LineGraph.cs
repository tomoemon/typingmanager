using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace TypingManager
{
    public enum LineGraphMarkType
    {
        None,
        Plus,
        VerticalBar,
        HorizonBar,
        Square,
    }

    public class LineGraph : Graph
    {
        // 縦を何分割する格子を描画するか
        const int VERT_GRID_NUM = 4;

        const float DEFAULT_MARK_SIZE = 3;

        const int CONTROL_BORDER_SIZE = 2;

        // グラフで表現する最大値と最小値
        private float value_min;
        private float value_max;

        // 格子を描画するときの開始位置
        private float start_grid;

        // プロットする数
        private int plot_num;

        // プロットする点の間隔
        private float plot_interval;

        // プロットする値が現在のvalue_maxを超えたときにvalue_maxに加える値
        private int max_value_adder;

        private LineGraphMarkType mark_type;
        private Dictionary<LineGraphMarkType, float> mark_size;

        private Color grid_color;
        private Color line_color;
        private Color mark_color;
        private Color mark_strong_color;

        private float[] data_list;

        #region プロパティ...
        public float ValueMin
        {
            get { return value_min; }
            set { value_min = value; }
        }
        public float ValueMax
        {
            get { return value_max; }
            set { value_max = value; }
        }
        public float GridWidth
        {
            get
            {
                float width = DrawRect.Height / VERT_GRID_NUM;
                return width;
            }
        }
        public int PlotNum
        {
            get { return plot_num; }
            set
            {
                plot_num = value;
                plot_interval = (float)DrawRect.Width / (plot_num - 1);
            }
        }
        public int MaxValueAdder
        {
            get { return max_value_adder; }
            set { max_value_adder = value; }
        }
        public LineGraphMarkType MarkType
        {
            get { return mark_type; }
            set { mark_type = value; }
        }
        public Color GridColor
        {
            get { return grid_color; }
            set { grid_color = value; }
        }
        public Color LineColor
        {
            get { return line_color; }
            set { line_color = value; }
        }
        public Color MarkColor
        {
            get { return mark_color; }
            set { mark_color = value; }
        }
        public Color MarkStrongColor
        {
            get { return mark_strong_color; }
            set { mark_strong_color = value; }
        }
        #endregion

        public LineGraph(int width, int height) : base(width, height)
        {
            Init();
        }

        public LineGraph(int width, int height, Rect rect)
            : base(width, height, rect)
        {
            Init();
        }

        private void Init()
        {
            value_min = 0f;
            value_max = 1000f;
            start_grid = 0;
            mark_type = LineGraphMarkType.None;
            mark_size = new Dictionary<LineGraphMarkType, float>();
            grid_color = Color.Green;
            line_color = Color.Lime;
            mark_color = Color.Aqua;
            mark_strong_color = Color.Red;
            foreach (LineGraphMarkType type in Enum.GetValues(typeof(LineGraphMarkType)))
            {
                mark_size[type] = DEFAULT_MARK_SIZE;
            }
        }

        public void SetMarkSize(LineGraphMarkType type, float size)
        {
            mark_size[type] = size;
        }

        public void DrawGrid(Graphics g, Color color, float start_x)
        {
            Pen pen = new Pen(color);
            // 横に線を引く
            for (int i = 1; i < VERT_GRID_NUM; i++)
            {
                g.DrawLine(pen, DrawRect.Left, GridWidth * i,
                    DrawRect.Right, GridWidth * i);
            }

            // 縦に線を引く
            g.DrawLine(pen, DrawRect.Left, DrawRect.Top, DrawRect.Left, DrawRect.Bottom);
            g.DrawLine(pen, DrawRect.Right, DrawRect.Top, DrawRect.Right, DrawRect.Bottom);

            float x = start_grid;
            while (x < DrawRect.Width)
            {
                x += GridWidth;
                if (x + DrawRect.Left < 0 || x + DrawRect.Left > DrawRect.Right)
                {
                    continue;
                }
                g.DrawLine(pen, x + DrawRect.Left, DrawRect.Top, x+DrawRect.Left, DrawRect.Bottom);
            }
            pen.Dispose();
        }

        /// <summary>
        /// 実際の値から縦軸の位置を決める
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public float ValueToPos(float value)
        {
            // maxとminの中で今の値は何％になっているかを調べる
            float percent = 0;
            if (value > value_min)
            {
                percent = (float)(value - value_min) / (value_max - value_min);
                percent = percent > 1.0f ? 1.0f : percent;
            }

            float pos = DrawRect.Height * percent;
            return pos;
        }

        /// <summary>
        /// グラフ上のX座標から最も近いプロットしているindexの値を返す
        /// indexは右端が0で左に向かう方向が正
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int CursorPosToIndex(float pos_x)
        {
            for (int i = 0; i < PlotNum; i++)
            {
                float x = DrawRect.Left + DrawRect.Width - plot_interval * i;
                if (pos_x > x)
                {
                    return i - 1;
                }
            }
            return PlotNum - 1;
        }

        public float GetValue(int index)
        {
            if (0 <= index && index < data_list.Length)
            {
                return data_list[index];
            }
            return 0;
        }

        /// <summary>
        /// 与えられたdataの最大値を上回るもっとも小さいMaxValueAdderの整数倍を返す
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public float AdjustMaxValue(float[] data)
        {
            float max = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (max < data[i])
                {
                    max = data[i];
                }
            }
            float border_max = 0;
            while (border_max <= max)
            {
                border_max += MaxValueAdder;
            }
            return border_max;
        }

        public void PlotMark(Graphics g, int index, Color color)
        {
            float value = ValueMin;
            if (index < data_list.Length)
            {
                value = data_list[index];
            }
            float x = DrawRect.Left + DrawRect.Width - plot_interval * index;
            float y = DrawRect.Bottom - ValueToPos(value) - CONTROL_BORDER_SIZE;
            PlotMark(g, x, y, color);
        }

        public void PlotMark(Graphics g, float x, float y, Color color)
        {
            Pen pen = new Pen(color);
            Brush brush = new SolidBrush(color);
            float size = mark_size[MarkType];
            if (MarkType == LineGraphMarkType.HorizonBar || MarkType == LineGraphMarkType.Plus)
            {
                g.DrawLine(pen, x - size / 2, y, x + size / 2, y);
            }
            if (MarkType == LineGraphMarkType.VerticalBar || MarkType == LineGraphMarkType.Plus)
            {
                g.DrawLine(pen, x,  y - size / 2, x,  y + size / 2);
            }
            if (MarkType == LineGraphMarkType.Square)
            {
                g.FillRectangle(brush, x - size / 2, y - size / 2, size, size);
            }
            pen.Dispose();
            brush.Dispose();
        }

        public void SetValue(float[] data)
        {
            data_list = data;
            ValueMax = AdjustMaxValue(data_list);
        }

        private void DrawData(Graphics g)
        {
            Pen pen = new Pen(line_color);

            float last_x = -1;
            float last_y = -1;
            float[] x_list = new float[PlotNum];
            float[] y_list = new float[PlotNum];

            for (int i = 0; i < PlotNum; i++)
            {
                float value = ValueMin;
                if (i < data_list.Length)
                {
                    value = data_list[i];
                }
                float x = DrawRect.Left + DrawRect.Width - plot_interval * i;
                float y = DrawRect.Bottom - ValueToPos(value) - 2;
                x_list[i] = x;
                y_list[i] = y;

                /*
                if (i == 0 || i==PlotNum-1)
                    Debug.WriteLine("left:{0}, right:{1}, x:{2}, y:{3}",
                        DrawRect.Left, DrawRect.Right, x, y);
                */
                if (last_x != -1)
                {
                    g.DrawLine(pen, x, y, last_x, last_y);
                }
                last_x = x;
                last_y = y;
            }
            for (int i = 0; i < PlotNum; i++)
            {
                PlotMark(g, x_list[i], y_list[i], mark_color);
            }
            pen.Dispose();
        }

        public void SlideGrid()
        {
            start_grid -= plot_interval;
            if (start_grid <= -GridWidth)
            {
                start_grid = start_grid + GridWidth;
            }
        }

        public override void DrawFrame()
        {
            // bmpに設定されているイメージのGraphicsオブジェクトを取得
            Graphics g = Graphics.FromImage(Bmp);
            g.FillRectangle(Brushes.Black, 0, 0, Bmp.Width, Bmp.Height);

            DrawGrid(g, grid_color, start_grid);
            DrawData(g);

            //Graphicsを破棄する
            g.Dispose();
        }
    }
}
