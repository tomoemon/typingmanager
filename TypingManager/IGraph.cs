using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace TypingManager
{
    /// <summary>
    /// Leftなどの値と直接設定したいのでDrawing.Rectangleとは別に実装
    /// Rectangleのメソッドを使いたい場合は内部にRectangleインスタンスを
    /// 持てばいいかも
    /// </summary>
    public class Rect
    {
        private int left;
        private int top;
        private int right;
        private int bottom;

        #region プロパティ...
        public int Left
        {
            get { return left; }
            set { left = value; }
        }
        public int Top
        {
            get { return top; }
            set { top = value; }
        }
        public int Right
        {
            get { return right; }
            set { right = value; }
        }
        public int Bottom
        {
            get { return bottom; }
            set { bottom = value; }
        }
        public int Width
        {
            get { return right - left + 1; }
        }
        public int Height
        {
            get { return bottom - top + 1; }
        }
        #endregion

        public Rect(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }
    }

    public interface IGraph
    {
        void Resize(int new_width, int new_height);
        void DrawFrame();
        void SetMargin(int left, int top, int right, int bottom);
        void Draw(Graphics g);

        Bitmap Bmp { get;}
        int Width { get;}
        int Height { get;}
        Rect DrawRect { get;}
        string GraphName { get;}
    }

    public abstract class Graph : IGraph
    {
        private Bitmap bmp;
        private Rect draw_rect;
        private string graph_name;

        #region プロパティ...
        public Bitmap Bmp
        {
            get { return bmp; }
        }
        public int Width
        {
            get { return bmp.Width; }
        }
        public int Height
        {
            get { return bmp.Height; }
        }
        public Rect DrawRect
        {
            get { return draw_rect; }
            set { draw_rect = value; }
        }
        public string GraphName
        {
            get { return graph_name; }
            set { graph_name = value; }
        }
        #endregion

        public Graph(int width, int height)
            :this(width, height, new Rect(0,0,width-1, height-1)){}

        public Graph(int width, int height, Rect rect)
        {
            bmp = new Bitmap(width, height);
            draw_rect = rect;
        }

        public void Resize(int new_width, int new_height)
        {
            bmp.Dispose();
            bmp = new Bitmap(new_width, new_height);
        }

        public virtual void Draw(Graphics g)
        {
            if (bmp != null)
            {
                g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
            }
        }

        public void SetMargin(Rect margin)
        {
            DrawRect.Left = margin.Left;
            DrawRect.Top = margin.Top;
            DrawRect.Right = Bmp.Width - DrawRect.Right;
            DrawRect.Bottom = Bmp.Height - DrawRect.Bottom;
        }

        public void SetMargin(int left, int top, int right, int bottom)
        {
            DrawRect.Left = left;
            DrawRect.Top = top;
            DrawRect.Right = Bmp.Width - right;
            DrawRect.Bottom = Bmp.Height - bottom;
        }

        public abstract void DrawFrame();
    }
}
