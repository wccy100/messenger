﻿using Messenger.Models;
using Messenger.Modules;
using Mikodev.Logger;
using Mikodev.Network;
using System;
using System.Windows;

namespace Messenger.Handles
{
    /// <summary>
    /// 文件传输
    /// </summary>
    [Handle("share")]
    public class Thing : LinkPacket
    {
        /// <summary>
        /// 处理传入的文件信息
        /// </summary>
        [Handle("file")]
        public void Take()
        {
            var tak = default(PortTaker);
            try
            {
                tak = new PortTaker(Data);
            }
            catch (Exception ex)
            {
                Log.Err(ex);
                return;
            }
            var trs = new Cargo(Source, tak);
            Application.Current.Dispatcher.Invoke(() =>
            {
                Ports.Expect.Add(trs);
                Ports.Takers.Add(trs);
                tak.Started += Ports.Trans_Changed;
                tak.Disposed += Ports.Trans_Changed;
            });
            var pkt = new Packet() { Source = Source, Target = Linkers.ID, Groups = Source, Path = "file", Value = trs };
            Application.Current.Dispatcher.Invoke(() =>
            {
                var pks = Packets.Query(Source);
                pks.Add(pkt);
            });
        }

        /// <summary>
        /// 处理传入的文件信息
        /// </summary>
        [Handle("dir")]
        public void Directory()
        {
            var tak = default(PortTaker);
            try
            {
                tak = new PortTaker(Data);
            }
            catch (Exception ex)
            {
                Log.Err(ex);
                return;
            }
            var trs = new Cargo(Source, tak);
            Application.Current.Dispatcher.Invoke(() =>
            {
                Ports.Expect.Add(trs);
                Ports.Takers.Add(trs);
                tak.Started += Ports.Trans_Changed;
                tak.Disposed += Ports.Trans_Changed;
            });
            var pkt = new Packet() { Source = Source, Target = Linkers.ID, Groups = Source, Path = "dir", Value = trs };
            Application.Current.Dispatcher.Invoke(() =>
            {
                var pks = Packets.Query(Source);
                pks.Add(pkt);
            });
        }
    }
}
