using System.Collections.Generic;

namespace CSharpDemo.Service
{
    public interface IAppDataService
    {
        List<string> GetItemModels();

        /// <summary>
        /// 下发状态采集指令
        /// </summary>
        /// <param name="devId"></param>
        string GetStatusCollectCmd(byte devId);

        /// <summary>
        /// 下发唤醒指令-加速度计
        /// </summary>
        string GetCorrelatorWakeUpCmd();
    }
}