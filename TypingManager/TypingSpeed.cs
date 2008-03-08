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
        /// <summary>打鍵速度を計算するためのサンプルを保持する数</summary>
        const int SAMPLE_MAX = 256;

        /// <summary>デフォルトの平均打鍵間隔[msec]</summary>
        const uint DEFAULT_AVERAGE_TIME = 1000;

        /// <summary>打鍵速度を算出するために用いるサンプル数</summary>
        private int sample_num;

        /// <summary>打鍵した時間[msec]を1打鍵ごとに記録したデータ</summary>
        private List<uint> sample_data;

        /// <summary>打鍵速度[stroke/min]を1秒おきに記録したデータ</summary>
        private List<float> speed_per_sec;

        /// <summary>1打鍵あたりの平均打鍵間隔[sec]</summary>
        private float average_stroke_time = DEFAULT_AVERAGE_TIME;

        /// <summary>1秒おきの前回の最後の打鍵時間[msec]</summary>
        private uint last_sample_data = 0;

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
            return speed_per_sec[speed_per_sec.Count - 1];
        }

        /// <summary>
        /// 現在タイピング中か
        /// </summary>
        /// <returns></returns>
        public bool IsTyping()
        {
            if (GetSpeed() == 0.0f)
            {
                return false;
            }
            return true;
        }

        public void SetLastSpeed(uint now)
        {
            int length = sample_data.Count;
            uint no_stroke_time = (uint)(average_stroke_time * 1000 * AppConfig.NoStrokeLimitTime);

            // no_stroke_timeが例えば0.01秒とかになると新しい打鍵をしても
            // 最後の打鍵から今の時間までの差の方が大きくなってしまうため
            // 常に下の条件が成り立ってsample_dataが消えてしまう
            // よってno_stroke_timeがDEFAULTよりも小さいときはDEFAULTに設定しなおす
            no_stroke_time = no_stroke_time < DEFAULT_AVERAGE_TIME ? DEFAULT_AVERAGE_TIME : no_stroke_time;
            
            if (length == 0 || now - sample_data[length - 1] > no_stroke_time)
            {
                // 最後の打鍵から設定した時間以上経過したら打鍵が無くなったものとする
                RecordAverageSpeed(0);
                sample_data.Clear();
                return;
            }

            // 平均打鍵速度算出に用いるサンプル数を決める
            int sample = length < sample_num ? length : sample_num;
            
            uint diff_sum = now - sample_data[length - sample];
            if (length == 1 && diff_sum <= DEFAULT_AVERAGE_TIME)
            {
                // 1打鍵しかない場合で打ってからDEFAULT_AVERAGE_TIMEしか経過していない場合
                diff_sum = DEFAULT_AVERAGE_TIME;
            }
            // 前回計算したときから新たな打鍵が発生していない場合は計算しない
            if (last_sample_data != sample_data[length - 1])
            {
                average_stroke_time = diff_sum / 1000.0f / sample;
                last_sample_data = sample_data[length - 1];
            }
            //Debug.WriteLine(string.Format("平均打鍵間隔：{0:f3}sec", average_stroke_time));
            float average_speed = (float)sample / diff_sum * 1000 * 60;
            RecordAverageSpeed(average_speed);
            return;
        }
    }
}
