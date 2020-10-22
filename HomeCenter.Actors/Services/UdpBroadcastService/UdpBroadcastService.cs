﻿using HomeCenter.CodeGeneration;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HomeCenter.Services.Networking
{
    [ProxyCodeGenerator]
    public class UdpBroadcastService : Service
    {
        [Subscribe(true)]
        protected async Task Handle(UdpCommand udpCommand)
        {
            using (var socket = new UdpClient())
            {
                var uri = new Uri($"udp://{udpCommand.Address}");

                socket.Connect(uri.Host, uri.Port);
                socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
                await socket.SendAsync(udpCommand.Body, udpCommand.Body.Length);
            }
        }
    }
}