using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace TypingManager
{
    public class GraphChanger : Graph, ITimerTask
    {
        public const int TIMER_ID_UPDATE = 0;
        private const int VIEW_VALUE_TEXT_MARGIN = 3;

        private GaugeGraph gaugeGraph;
        private Dictionary<LineGraphType, LineGraph> graphDic;
        private Dictionary<LineGraphType, string> graphViewFormat;
        private TypingSpeed typing_speed;
        private StrokeNumLog stroke_num;
        private TextBox typeSpeedText;
        private PictureBox typeSpeedPicture;
        private PictureBox historyPicture;
        private TextBox historyMaxValue;
        private TextBox historyMinValue;
        private TextBox viewValue;

        #region プロパティ...
        public LineGraph this[LineGraphType index]
        {
            get { return graphDic[index]; }
        }
        #endregion

        public GraphChanger(PictureBox pictureBox,
            TextBox _historyMaxValue, TextBox _historyMinValue,
            PictureBox _typeSpeedPicture, TextBox _typeSpeedText, TextBox _viewValue)
            : base(pictureBox.Width, pictureBox.Height)
        {
            historyPicture = pictureBox;
            historyMaxValue = _historyMaxValue;
            historyMinValue = _historyMinValue;
            typeSpeedPicture = _typeSpeedPicture;
            typeSpeedText = _typeSpeedText;
            viewValue = _viewValue;
            graphDic = new Dictionary<LineGraphType, LineGraph>();
            graphViewFormat = new Dictionary<LineGraphType, string>();

            // 打鍵速度の瞬間値を表示するグラフの設定
            gaugeGraph = new GaugeGraph(typeSpeedPicture.Width, typeSpeedPicture.Height);
            gaugeGraph.SetMargin(21, 5, 25, typeSpeedText.Height + 10);

            viewValue.Parent = pictureBox;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="box_name">グラフを囲んでいるグループボックス名</param>
        /// <param name="plot_num">プロットする数</param>
        /// <param name="max_adder"></param>
        public void SetGraph(LineGraphType type, 
            string box_name, int plot_num, int max_adder, string viewFormat)
        {
            LineGraph linegraph = new LineGraph(Width, Height, DrawRect);
            linegraph.PlotNum = plot_num;
            linegraph.MaxValueAdder = max_adder;
            linegraph.GraphName = box_name;
            linegraph.DrawRect = DrawRect;
            graphDic.Add(type, linegraph);
            graphViewFormat[type] = viewFormat;
        }

        public void SetDataSource(StrokeNumLog stroke_num, TypingSpeed typing_speed)
        {
            this.stroke_num = stroke_num;
            this.typing_speed = typing_speed;
        }

        public void SetGraphMark(LineGraphMarkType mark)
        {
            foreach (LineGraph graph in graphDic.Values)
            {
                graph.MarkType = mark;
            }
        }

        public void SetMarkSize(LineGraphMarkType mark, float size)
        {
            foreach (LineGraph graph in graphDic.Values)
            {
                graph.SetMarkSize(mark, size);
            }
        }

        public void SetValue(LineGraphType type)
        {
            float[] speed = typing_speed.GetSpeedRange(0,this[LineGraphType.TypeSpeedPerStroke].PlotNum);
            this[LineGraphType.TypeSpeedPerStroke].SetValue(speed);
            this[LineGraphType.TypeSpeedPerStroke].SlideGrid();

            if(type != LineGraphType.TypeSpeedPerStroke)
            {
                int[] source;
                if (type == LineGraphType.StrokeNumPerMinute)
                {
                    source = stroke_num.GetMinuteStroke(this[type].PlotNum);
                }
                else if (type == LineGraphType.StrokeNumPerHour)
                {
                    source = stroke_num.GetHourStroke(this[type].PlotNum);
                }
                else
                {
                    source = stroke_num.GetDayStroke(this[type].PlotNum);
                }
                float[] data = new float[source.Length];
                //Debug.Write("data: ");
                for (int i = 0; i < source.Length; i++)
                {
                    data[i] = source[i];
                    //Debug.Write("{0},", source[i]);
                }
                //Debug.Write("\n");
                this[type].SetValue(data);
            }
        }

        public override void DrawFrame()
        {
            throw new Exception("this method is not implemented");
        }

        public override void Draw(System.Drawing.Graphics g)
        {
            throw new Exception("this method is not implemented");
        }

        public void TimerTask(DateTime date, int id)
        {
            typing_speed.SetLastSpeed(QueryTime.NowMiliSec);
            SetGraphValue();
        }

        public void SetGraphValue()
        {
            float speed = typing_speed.GetSpeed();

            SetValue(AppConfig.LineGraphType);
            this[AppConfig.LineGraphType].DrawFrame();

            gaugeGraph.Max = this[LineGraphType.TypeSpeedPerStroke].ValueMax;
            gaugeGraph.SetValue(speed);
            gaugeGraph.DrawFrame();

            typeSpeedText.Text = speed.ToString("f0") + "打/分";
            typeSpeedPicture.Invalidate();
            historyMaxValue.Text = this[AppConfig.LineGraphType].ValueMax.ToString("f0");
            historyMinValue.Text = this[AppConfig.LineGraphType].ValueMin.ToString("f0");
            historyPicture.Invalidate();
        }

        public void DrawGauge(Graphics g)
        {
            gaugeGraph.Draw(g);
        }

        public void ShowText(int x, int y)
        {
            viewValue.Left = this.DrawRect.Left;
            viewValue.Top = this.DrawRect.Top;
            viewValue.Show();
            MoveText(x, y);
        }

        public void HideText()
        {
            viewValue.Hide();
        }

        public void MoveText(int x, int y)
        {
            Point p = viewValue.Parent.PointToClient(new Point(x, y));
            //Console.WriteLine("GraphChanger.MoveText, x={0}, y={1}", p.X, p.Y);
            int index = this[AppConfig.LineGraphType].CursorPosToIndex(p.X);
            string format = graphViewFormat[AppConfig.LineGraphType];
            string text = string.Format(format, index, this[AppConfig.LineGraphType].GetValue(index));
            viewValue.Text = text;
            viewValue.Width = viewValue.PreferredSize.Width;
            if (p.X + viewValue.Width + VIEW_VALUE_TEXT_MARGIN > historyPicture.Width)
            {
                viewValue.Left = historyPicture.Width - viewValue.Width;
            }
            else if (p.X + VIEW_VALUE_TEXT_MARGIN < this.DrawRect.Left)
            {
                viewValue.Left = this.DrawRect.Left;
            }
            else
            {
                viewValue.Left = p.X + VIEW_VALUE_TEXT_MARGIN;
            }
            if (p.Y - viewValue.Height - VIEW_VALUE_TEXT_MARGIN < this.DrawRect.Top)
            {
                viewValue.Top = this.DrawRect.Top;
            }
            else
            {
                viewValue.Top = p.Y - viewValue.Height - VIEW_VALUE_TEXT_MARGIN;
            }
        }
    }
}
