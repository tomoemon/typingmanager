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
        // �v���O�C���̖��O��Ԃ�����
        public virtual string GetPluginName() { return ""; }

        // �v���O�C���Ɋւ���ȒP�Ȑ�������������
        public virtual string GetComment() { return ""; }

        // �v���O�C����҂̖��O��Ԃ�����
        public virtual string GetAuthorName() { return ""; }

        // �v���O�C���̎��s�ɕK�v�ȃp�����[�^����ini���ۂ��Ԃ�����
        public virtual string GetInputTemplate() { return ""; }

        // �v���O�C���̃o�[�W��������������
        public virtual string GetVersion() { return ""; }

        // �v���O�C�������s���ꂽ�Ƃ��̏�������������
        // �Ԃ�l�͏o�͌���
        public virtual string Run() { return ""; }
    }
}
