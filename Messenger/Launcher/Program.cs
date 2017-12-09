﻿using Mikodev.Logger;
using Mikodev.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.SetPath(nameof(Launcher));

            try
            {
                Entrance(args);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        static void Entrance(string[] args)
        {
            var nam = default(string);
            var max = Links.ServerSocketLimit;
            var pot = Links.ListenPort;
            var bro = Links.BroadcastPort;
            var dic = new Dictionary<string, string>();
            foreach (var i in args)
            {
                var idx = i.Split(new char[] { ':' }, 2);
                if (idx.Length < 2)
                    continue;
                dic.Add(idx[0].ToLower(), idx[1]);
            }
            if (dic.TryGetValue("name", out var val))
                nam = val;
            if (dic.TryGetValue("max", out var lin))
                max = int.Parse(lin);
            if (dic.TryGetValue("port", out var por))
                pot = int.Parse(por);
            if (dic.TryGetValue("broadcast", out var bad))
                bro = int.Parse(bad);

            var lis = new LinkListener(pot, max);
            lis.Listen();
            lis.Broadcast(bro, nam).Wait();
        }
    }
}
