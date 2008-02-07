using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace TypingManager
{
    /// <summary>
    /// 今の打鍵速度を計算する
    /// </summary>
    public class TypingSpeed
    {
        // 打鍵速度を計算するためのサンプルを保持する数
        const int SAMPLE_MAX = 1024;

        // この時間で一つも入力が無ければ打鍵速度0とする
        const int ZERO_TIME = 20000; // 20秒

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

            // サンプルとして保持しておくサイズの最大に達したら先頭半分を削除する
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
        /// 単位時間ごとに取得した平均打鍵速度を配列で取得する
        /// </summary>
        /// <param name="start">0が最新，数が大きくなると古いデータ</param>
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
        /// 過去のsample_num数の打鍵から平均打鍵速度（打／分）を計算する
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
