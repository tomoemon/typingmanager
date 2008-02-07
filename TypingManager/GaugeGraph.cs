using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace TypingManager
{
    public class GaugeGraph : Graph
    {
        // �L���[�̒��ɓ���Ă�����ߋ��̒l�̐�
        const int MAX_QUEUE = 1024;

        // �Q�[�W�̌�
        const int GAUGE_NUM = 10;

        // �Q�[�W���Ƃ̗]��
        const int GAUGE_MARGIN = 1;

        private float max;
        private float min;
        private List<float> values;

        #region �v���p�e�B
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
            
            // �T���v���Ƃ��ĕێ����Ă����T�C�Y�̍ő�ɒB������擪�������폜����
            if (values.Count >= MAX_QUEUE)
            {
                values.RemoveRange(0, MAX_QUEUE / 2);
            }
        }

        public int GaugeNum(float value)
        {
            // max��min�̒��ō��̒l�͉����ɂȂ��Ă��邩�𒲂ׂ�
            float percent = 0;
            if (value > min)
            {
                percent = (float)(value - min) / (max - min);
            }

            // ���U�������Q�[�W�̒��ŉ��ڂ܂ŒB���Ă��邩
            int gauge_value = (int)(GAUGE_NUM * percent);
            if (gauge_value > GAUGE_NUM)
            {
                gauge_value = GAUGE_NUM;
            }
            return gauge_value;
        }

        private int GetGaugeHeight()
        {
            // �`��ł���͈͂�draw_rect
            int drawable_height = DrawRect.Height - GAUGE_MARGIN * (GAUGE_NUM - 1);
            if (drawable_height < GAUGE_NUM)
            {
                drawable_height = DrawRect.Height;
            }
            // ��̃Q�[�W������̍���
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
            // bmp�ɐݒ肳��Ă���C���[�W��Graphics�I�u�W�F�N�g���擾
            Graphics g = Graphics.FromImage(Bmp);
            g.FillRectangle(Brushes.Black, 0, 0, Bmp.Width, Bmp.Height);

            float now = values[values.Count-1];
            int gauge_value = GaugeNum(now);
            DrawGauges(g, Brushes.Green, GAUGE_NUM);
            DrawGauges(g, Brushes.Lime, gauge_value);
            
            //Graphics��j������
            g.Dispose();
        }
    }
}
