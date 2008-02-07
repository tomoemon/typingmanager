using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace AnalyzePlugin
{
    public interface IStrokePluginController
    {
        /// <summary>
        /// プラグインから呼ばれる関数，他のプラグインに関する情報を返してあげる
        /// </summary>
        /// <param name="name">情報の欲しいプラグインの名前</param>
        /// <returns></returns>
        object GetInfo(string name);
    }

    public interface IStrokePlugin
    {
        /// <summary>プラグインの名前を返すこと</summary>
        string GetPluginName();

        /// <summary>プラグインに関する簡単な説明を書くこと</summary>
        string GetComment();

        /// <summary>プラグイン作者の名前を返すこと</summary>
        string GetAuthorName();

        /// <summary>プラグインのバージョンを書くこと</summary>
        string GetVersion();

        /// <summary>キーが押されたときに呼び出される</summary>
        /// <param name="keycode">押されたキーの仮想キーコード</param>
        /// <param name="militime">キーが押された時間[ミリ秒]（OSが起動してからの経過時間）</param>
        /// <param name="app_path">キーが押されたアプリケーションのフルパス</param>
        /// <param name="app_title">キーが押されたウィンドウのタイトル</param>
        void KeyDown(int keycode, int militime, string app_path, string app_title);

        /// <summary>キーが上がったときに呼び出される</summary>
        void KeyUp(int keycode, int militime, string app_path, string app_title);
        
        /// <summary>
        /// 他のプラグインでも使える情報を渡したいときに使う
        /// IStrokePluginControllerから呼ばれる
        /// </summary>
        /// <returns></returns>
        object GetInfo();

        /// <summary>メインメニューに加えるメニューを返す</summary>
        List<Menu> GetMainMenu();

        IStrokePluginController Controller
        {
            get;set;
        }
    }
}
