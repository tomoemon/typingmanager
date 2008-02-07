using System;
using System.Collections.Generic;
using System.Text;

namespace AnalyzePlugin
{
    public interface IAnalyze {
        string GetPluginName();
        string GetComment();
        string GetAuthorName();
        string GetInputTemplate();
        string GetVersion();
        string Run();
    }

    public class AnalyzeTool : IAnalyze
    {
        // プラグインの名前を返すこと
        public virtual string GetPluginName() { return ""; }

        // プラグインに関する簡単な説明を書くこと
        public virtual string GetComment() { return ""; }

        // プラグイン作者の名前を返すこと
        public virtual string GetAuthorName() { return ""; }

        // プラグインの実行に必要なパラメータ名をiniっぽく返すこと
        public virtual string GetInputTemplate() { return ""; }

        // プラグインのバージョンを書くこと
        public virtual string GetVersion() { return ""; }

        // プラグインが実行されたときの処理を書くこと
        // 返り値は出力結果
        public virtual string Run() { return ""; }
    }
}
