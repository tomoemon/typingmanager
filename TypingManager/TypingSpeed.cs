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
        // �Ō����x���v�Z���邽�߂̃T���v����ێ����鐔
        const int SAMPLE_MAX = 1024;

        // ���̎��Ԃň�����͂�������ΑŌ����x0�Ƃ���
        const int ZERO_TIME = 20000; // 20�b

        private int sample_num;
        private List<int> sample_data;
        private List<float> speed_per_sec;

        public TypingSpeed(int sample_num)
        {
            this.sample_num = sample_num;
            sample_data = new List<int>(SAMPLE_MAX);
            speed_per_sec = new List<float>(SAMPLE_MAX);
        }

        public void Stroke(int mili_sec)
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
            return GetSpeed(sample_data[sample_data.Count-1]);
        }

        public float GetSpeed(int now)
        {
            int length = sample_data.Count; 
            if (sample_data.Count < sample_num ||
                now - sample_data[length - 1] > ZERO_TIME)
            {
                RecordAverageSpeed(0);
                return 0;
            }
            int diff_sum = now - sample_data[length - 20];
            float average = (float)sample_num / diff_sum * 1000 * 60;
            RecordAverageSpeed(average);
            return average;
        }
    }
}
