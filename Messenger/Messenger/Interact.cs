﻿using Messenger.Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Interop;

namespace Messenger
{
    class Interact
    {
        private object _locker = new object();
        private Client _client = null;

        private static Interact _instance = new Interact();

        public static int ID => _instance._client?.ID ?? ModuleProfile.Current.ID;

        public static bool IsRunning
        {
            get
            {
                var clt = _instance._client;
                if (clt == null)
                    return false;
                if (clt.IsStarted == true && clt.IsDisposed == false)
                    return true;
                return false;
            }
        }

        public static event EventHandler<GenericEventArgs<(Guid, Socket)>> Requests
        {
            add
            {
                var clt = _instance._client;
                if (clt == null)
                    return;
                clt.Requests += value;
            }
            remove
            {
                var clt = _instance._client;
                if (clt == null)
                    return;
                clt.Requests -= value;
            }
        }

        public static void Start(int id, IPEndPoint endpoint)
        {
            var clt = default(Client);
            try
            {
                clt = new Client(id);
                clt.Start(endpoint);
            }
            catch (Exception ex)
            {
                Log.E(nameof(Interact), ex, "连接服务器失败.");
                clt?.Dispose();
                throw;
            }

            clt.Received += Client_Received;
            clt.Shutdown += Client_Shutdown;

            lock (_instance._locker)
            {
                if (_instance._client != null)
                {
                    clt.Dispose();
                    throw new InvalidOperationException();
                }
                _instance._client = clt;
            }

            ModulePacket.OnHandled += ModuleMessage_Onhandled;
            ModuleTrans.Expect.ListChanged += ModuleTrans_ListChanged;
            ModuleProfile.Current.ID = id;
            Enqueue(Server.ID, PacketGenre.UserProfile, ModuleProfile.Current);
            if (ModuleProfile.ImageBuffer != null)
                Enqueue(Server.ID, PacketGenre.UserImage, ModuleProfile.ImageBuffer);
            Enqueue(Server.ID, PacketGenre.UserRequest);
            var lst = ModuleProfile.GroupIDs;
            if (lst != null)
                Enqueue(Server.ID, PacketGenre.UserGroups, lst.ToList());
        }

        public static void Close()
        {
            var clt = default(Client);
            lock (_instance._locker)
            {
                clt = _instance._client;
                _instance._client = null;
            }

            if (clt == null)
                return;
            clt.Received -= Client_Received;
            clt.Shutdown -= Client_Shutdown;
            clt.Dispose();

            ModulePacket.OnHandled -= ModuleMessage_Onhandled;
            ModuleTrans.Expect.ListChanged -= ModuleTrans_ListChanged;
        }

        public static void Enqueue(int target, PacketGenre genre, object value = null)
        {
            var clt = _instance._client;
            if (clt == null)
                return;
            var pkt = Extension.GetPacket(target, clt.ID, genre, value);
            clt.Enqueue(pkt);
        }

        /// <summary>
        /// 获取与连接关联的 NAT 内部端点和外部端点 (若二者相同 则只返回一个 且不会返回 null)
        /// </summary>
        public static List<IPEndPoint> GetEndPoints()
        {
            var lst = new List<IPEndPoint>();
            var clt = _instance._client;
            if (clt?.InnerEndPoint is IPEndPoint iep)
                lst.Add(iep);
            if (clt?.OuterEndPoint is IPEndPoint rep)
                lst.Add(rep);
            var res = lst.Distinct((a, b) => a.Equals(b)).ToList();
            return res;
        }

        private static void ModuleMessage_Onhandled(object sender, GenericEventArgs<ItemPacket> e)
        {
            Application.Current.Dispatcher.Invoke(() =>
                {
                    var pro = ModuleProfile.Query(e.Value.Groups);
                    if (pro == null)
                        return;
                    var hdl = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                    if (Application.Current.MainWindow.IsActive == false || e.Handled == false)
                        NativeMethods.FlashWindow(hdl, true);
                    if (e.Handled == false)
                        pro.Hint += 1;
                });
        }

        private static void ModuleTrans_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (sender == ModuleTrans.Expect && e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (Application.Current.MainWindow.IsActive == true)
                    return;
                var hdl = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                NativeMethods.FlashWindow(hdl, true);
            }
        }

        private static void Client_Shutdown(object sender, EventArgs e)
        {
            MainWindow.ShowMessage("服务器连接已断开", _instance?._client?.Exception);
        }

        private static void Client_Received(object sender, PacketEventArgs e)
        {
            try
            {
                switch (e.Genre)
                {
                    case PacketGenre.MessageText:
                        var msg = Xml.Deserialize<string>(e.Stream);
                        ModulePacket.Insert(e, msg);
                        break;

                    case PacketGenre.MessageImage:
                        var buf = e.Stream?.ToArray();
                        ModulePacket.Insert(e, buf);
                        break;

                    case PacketGenre.UserIDs:
                        var lst = Xml.Deserialize<List<int>>(e.Stream);
                        ModuleProfile.Remove(lst);
                        break;

                    case PacketGenre.UserImage:
                        var str = Cache.SetBuffer(e.Stream.ToArray(), true);
                        var pfl = ModuleProfile.Query(e.Source);
                        if (str != null && pfl != null)
                            pfl.Image = str;
                        break;

                    case PacketGenre.UserProfile:
                        var pro = Xml.Deserialize<Profile>(e.Stream);
                        ModuleProfile.Insert(pro);
                        break;

                    case PacketGenre.UserRequest:
                        Enqueue(e.Source, PacketGenre.UserProfile, ModuleProfile.Current);
                        if (ModuleProfile.ImageBuffer != null)
                            Enqueue(e.Source, PacketGenre.UserImage, ModuleProfile.ImageBuffer);
                        break;

                    case PacketGenre.FileInfo:
                        var trs = ModuleTrans.Take(e);
                        if (trs == null)
                            break;
                        var pkt = new ItemPacket() { Source = e.Source, Target = ID, Groups = e.Source, Genre = PacketGenre.FileInfo, Value = trs };
                        Application.Current.Dispatcher.Invoke(() =>
                            {
                                var pks = ModulePacket.Query(e.Source);
                                pks.Add(pkt);
                            });
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.E(nameof(Interact), ex, "处理消息出错.");
            }
        }
    }
}
