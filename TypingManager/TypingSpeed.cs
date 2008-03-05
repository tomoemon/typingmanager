using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace TypingManager
{
    /// <summary>
    /// ���̑Ō����x���v�Z����
    /// </summary>
    public class TypingSpeed
    {
        /// <summary>�Ō����x���v�Z���邽�߂̃T���v����ێ����鐔</summary>
        const int SAMPLE_MAX = 256;

        /// <summary>�f�t�H���g�̕��ϑŌ��Ԋu[msec]</summary>
        const uint DEFAULT_AVERAGE_TIME = 1000;

        /// <summary>�Ō����x���Z�o���邽�߂ɗp����T���v����</summary>
        private int sample_num;

        /// <summary>�Ō���������[msec]��1�Ō����ƂɋL�^�����f�[�^</summary>
        private List<uint> sample_data;

        /// <summary>�Ō����x[stroke/min]��1�b�����ɋL�^�����f�[�^</summary>
        private List<float> speed_per_sec;

        /// <summary>1�Ō�������̕��ϑŌ��Ԋu[sec]</summary>
        private float average_stroke_time = DEFAULT_AVERAGE_TIME;

        public TypingSpeed(int sample_num)
        {
            this.sample_num = sample_num;
            sample_data = new List<uint>(SAMPLE_MAX);
            speed_per_sec = new List<float>(SAMPLE_MAX);
            speed_per_sec.Add(0);
        }

        public void Stroke(uint mili_sec)
        {
            sample_data.Add(mili_sec);

            // �T���v���Ƃ��ĕێ����Ă����T�C�Y�̍ő�ɒB������擪�������폜����
            if (sample_data.Count >= SAMPLE_MAX)
            {
                sample_data.RemoveRange(0, SAMPLE_MAX / 2);
            }
        }

        private void RecordAverageSpeed(float speed)
        {
            speed_per_sec.Add(speed);

            if (speed_per_sec.Count >= SAMPLE_MAX)
            {
                speed_per_sec.RemoveRange(0, SAMPLE_MAX / 2);
            }
        }

        /// <summary>
        /// �P�ʎ��Ԃ��ƂɎ擾�������ϑŌ����x��z��Ŏ擾����
        /// </summary>
        /// <param name="start">0���ŐV�C�����傫���Ȃ�ƌÂ��f�[�^</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public float[] GetSpeedRange(int start, int count)
        {
            if (speed_per_sec.Count < count + start)
            {
                count = speed_per_sec.Count - start;
            }
            float[] result = new float[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = speed_per_sec[speed_per_sec.Count - i - 1];
            }
            return result;
        }

        /// <summary>
        /// �ߋ���sample_num���̑Ō����畽�ϑŌ����x�i�Ł^���j���v�Z����
        /// </summary>
        /// <returns></returns>
        public float GetSpeed()
        {
            return speed_per_sec[speed_per_sec.Count - 1];
        }

        public void SetLastSpeed(uint now)
        {
            int length = sample_data.Count;

            if (length == 0)
            {
                // �Ō��f�[�^���Ȃ���Ε��ϑŌ����x��0
                RecordAverageSpeed(0);
                return;
            }
            else if (now - sample_data[length - 1] > AppConfig.NoStrokeLimitTime * 1000)
            {
                // �Ō�̑Ō�����ݒ肵�����Ԉȏ�o�߂�����Ō��������Ȃ������̂Ƃ���
                RecordAverageSpeed(0);
                sample_data.Clear();
                return;
            }
            // ���ϑŌ����x�Z�o�ɗp����T���v���������߂�
            int sample = length < sample_num ? length : sample_num;
            
            uint diff_sum = now - sample_data[length - sample];
            if (diff_sum <= DEFAULT_AVERAGE_TIME && length == 1)
            {
                // 1�Ō������Ȃ��ꍇ
                diff_sum = DEFAULT_AVERAGE_TIME;
            }
            average_stroke_time = diff_sum / 1000.0f / sample;
            Console.WriteLine("���ϑŌ��Ԋu�F{0:f3}sec", average_stroke_time);
            float average_speed = (float)sample / diff_sum * 1000 * 60;
            RecordAverageSpeed(average_speed);
            return;
        }
    }
}
