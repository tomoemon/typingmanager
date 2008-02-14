using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Plugin
{
    public interface IPluginController
    {
        /// <summary>
        /// 他のプラグインに関する情報を返す
        /// プラグインから呼ばれる関数
        /// </summary>
        /// <param name="name">情報の欲しいプラグインの名前</param>
        /// <returns></returns>
        object GetInfo(string name);

        /// <summary>
        /// 各プラグインがログの保存に使うディレクトリ
        /// プラグインから呼ばれる関数
        /// </summary>
        /// <returns></returns>
        string GetSaveDir(string pluginname);

        /// <summary>
        /// 各プラグインが設定の保存に使うディレクトリ
        /// プラグインから呼ばれる関数
        /// </summary>
        /// <returns></returns>
        string GetConfigDir(string pluginname);
    }

    public interface IStrokePluginController : IPluginController
    {
    }

    public interface IFilterPluginController
    {
        void FilteredKeyUp(IFilterPlugin plugin, IKeyState keycode, uint militime,
            string app_path, string app_title);
        void FilteredKeyDown(IFilterPlugin plugin, IKeyState keycode, uint militime,
            string app_path, string app_title);
    }
}
