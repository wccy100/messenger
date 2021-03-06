﻿using Messenger.Models;
using Messenger.Modules;
using Mikodev.Logger;
using Mikodev.Network;

namespace Messenger.Controllers
{
    /// <summary>
    /// 消息处理
    /// </summary>
    public class MessageController : LinkPacket
    {
        /// <summary>
        /// 文本消息
        /// </summary>
        [Route("msg.text")]
        public void Text()
        {
            var txt = Data.GetValue<string>();
            HistoryModule.Insert(Source, Target, "text", txt);
        }

        /// <summary>
        /// 图片消息
        /// </summary>
        [Route("msg.image")]
        public void Image()
        {
            var buf = Data.GetArray<byte>();
            HistoryModule.Insert(Source, Target, "image", buf);
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        [Route("msg.notice")]
        public void Notice()
        {
            var dat = Data;
            var typ = Data["type"].GetValue<string>();
            var par = Data["parameter"].GetValue<string>();
            var str = typ == "share.file"
                ? $"已成功接收文件 {par}"
                : typ == "share.dir"
                    ? $"已成功接收文件夹 {par}"
                    : null;
            if (str == null)
                Log.Info($"Unknown notice type: {typ}, parameter: {par}");
            else
                HistoryModule.Insert(Source, Target, "notice", str);
            return;
        }
    }
}
