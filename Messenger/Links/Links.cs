﻿namespace Mikodev.Network
{
    public static class Links
    {
        public const string Protocol = "mikodev.messenger.v1.18";

        public const int ListenPort = 7550;

        public const int BroadcastPort = ListenPort;

        public const int Timeout = 5 * 1000;

        public const int KeepAliveBefore = 300 * 1000;

        public const int KeepAliveInterval = Timeout;

        public const int Id = 0;

        public const int ServerSocketLimit = 256;

        public const int ClientSocketLimit = 64;

        public const int BufferLength = 4 * 1024;

        public const int BufferLengthLimit = 1024 * 1024;

        public const int BufferQueueLimit = 16 * 1024 * 1024;

        public const int Delay = 4;

        public const int GroupLabelLimit = 32;
    }
}
