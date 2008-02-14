using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin
{
    /// <summary>
    /// Controllerから呼び出されるKeyDown，KeyUpから
    /// 必要なものを残して，次に伝達させる．
    /// 伝達できる時点でIFilterPluginControllerのFilter関数を呼び出す
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
