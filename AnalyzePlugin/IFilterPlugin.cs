using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin
{
    /// <summary>
    /// Controller����Ăяo�����KeyDown�CKeyUp����
    /// �K�v�Ȃ��̂��c���āC���ɓ`�B������D
    /// �`�B�ł��鎞�_��IFilterPluginController��Filter�֐����Ăяo��
    /// </summary>
    public interface IFilterPlugin : IPluginBase
    {
        IFilterPluginController FilterController
        {
            get;
            set;
        }
    }
}
