﻿using System;
using System.Diagnostics;
using System.IO;

namespace Messenger.Foundation
{
    /// <summary>
    /// 日志类 (保存位置为系统临时文件夹)
    /// </summary>
    public class Log : TraceListener
    {
        private string _path = null;

        private void _BufferWriter(params string[] message)
        {
            var fil = default(FileStream);
            var fsw = default(StreamWriter);
            try
            {
                fil = new FileStream(_path, FileMode.Append, FileAccess.Write);
                fsw = new StreamWriter(fil);
                fsw.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]");
                foreach (var m in message)
                    fsw.Write(m);
                return;
            }
            catch { }
            finally
            {
                fsw?.Dispose();
                fil?.Dispose();
            }
        }

        public Log(string filename) => _path = Path.Combine(Path.GetTempPath(), filename);

        public override void Write(string message) => _BufferWriter(message);

        public override void WriteLine(string message) => _BufferWriter(message, Environment.NewLine);
    }
}
