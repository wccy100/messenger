﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static System.BitConverter;

namespace Mikodev.Network
{
    public static class LinkExtension
    {
        public static byte[] Concat(params byte[][] arrays)
        {
            var sum = 0;
            for (int i = 0; i < arrays.Length; i++)
                sum += arrays[i].Length;
            var arr = new byte[sum];
            var idx = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                var cur = arrays[i];
                var len = cur.Length;
                Buffer.BlockCopy(cur, 0, arr, idx, len);
                idx += len;
            }
            return arr;
        }

        public static Task ConnectAsyncEx(this Socket socket, IPEndPoint endpoint) => Task.Factory.FromAsync((a, o) => socket.BeginConnect(endpoint, a, o), socket.EndConnect, null);

        public static Task<Socket> AcceptAsyncEx(this Socket socket) => Task.Factory.FromAsync(socket.BeginAccept, socket.EndAccept, null);

        public static int SetKeepAlive(this Socket socket, bool enable = true, uint before = Links.KeepAliveBefore, uint interval = Links.KeepAliveInterval)
        {
            if (enable == true && (before < 1 || interval < 1))
                throw new ArgumentOutOfRangeException("Keep alive argument out of range.");
            var val = new byte[sizeof(uint)];
            var res = Concat(GetBytes(1U), GetBytes(before), GetBytes(interval));
            socket.IOControl(IOControlCode.KeepAliveValues, res, val);
            return ToInt32(val, 0);
        }

        public static async Task<byte[]> ReceiveAsyncExt(this Socket socket)
        {
            var buf = await ReceiveAsyncEx(socket, sizeof(int));
            var len = ToInt32(buf, 0);
            var res = await ReceiveAsyncEx(socket, len);
            return res;
        }

        public static async Task<byte[]> ReceiveAsyncEx(this Socket socket, int length)
        {
            if (length < 1 || length > Links.BufferLengthLimit)
                throw new LinkException(LinkError.Overflow, "Buffer length out of range!");
            var buf = new byte[length];
            var idx = 0;
            while (idx < length)
            {
                var sub = length - idx;
                var len = await Task.Factory.FromAsync((a, s) => socket.BeginReceive(buf, idx, sub, SocketFlags.None, a, s), socket.EndReceive, null);
                if (len < 1)
                    throw new SocketException((int)SocketError.ConnectionReset);
                idx += len;
            }
            return buf;
        }

        public static async Task SendAsyncExt(this Socket socket, byte[] buffer)
        {
            var len = GetBytes(buffer.Length);
            await SendAsyncEx(socket, len);
            await SendAsyncEx(socket, buffer);
        }

        public static Task SendAsyncEx(this Socket socket, byte[] buffer) => SendAsyncEx(socket, buffer, 0, buffer.Length);

        public static async Task SendAsyncEx(this Socket socket, byte[] buffer, int offset, int length)
        {
            while (length > 0)
            {
                var len = await Task.Factory.FromAsync((a, o) => socket.BeginSend(buffer, offset, length, SocketFlags.None, a, o), socket.EndSend, null);
                length -= len;
            }
        }

        public static LinkPacket LoadValue(this LinkPacket src, byte[] buf)
        {
            var ori = new PacketReader(buf);
            src._buf = buf;
            src._ori = ori;
            src._src = ori["source"].Pull<int>();
            src._tar = ori["target"].Pull<int>();
            src._pth = ori["path"].Pull<string>();
            src._dat = ori["data", true];
            return src;
        }

        public static T WaitTimeout<T>(this Task<T> task, string message = null, int milliseconds = Links.Timeout)
        {
            var res = task.Wait(milliseconds);
            if (res == false)
                throw string.IsNullOrEmpty(message)
                    ? new TimeoutException()
                    : new TimeoutException(message);
            return task.Result;
        }

        public static void WaitTimeout(this Task task, string message = null, int milliseconds = Links.Timeout)
        {
            var res = task.Wait(milliseconds);
            if (res == false)
                throw string.IsNullOrEmpty(message)
                    ? new TimeoutException()
                    : new TimeoutException(message);
            return;
        }
    }
}
