//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WmBridge.Service
{
    public class Autodiscovery : IDisposable
    {
        private static readonly ILog logger = LogManager.GetLogger("WmBridge.Autodiscovery");

        private readonly UdpClient udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 53581));

        private readonly byte[][] responses;

        public static Autodiscovery StartResponder(IEnumerable<string> urls)
        {
            var instance = new Autodiscovery(urls.Select(url => Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(new { url, machine = Environment.MachineName })))
                .ToArray());

            instance.BeginReceive();
            
            return instance;
        }

        private Autodiscovery(byte[][] responses)
        {
            this.responses = responses;
        }

        public void BeginReceive()
        {
            try
            {
                udpClient.BeginReceive(RequestCallback, null);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void RequestCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                udpClient.EndReceive(ar, ref ep);

                foreach (byte[] data in responses)
                    udpClient.Send(data, data.Length, ep);
            }
            catch (ObjectDisposedException) // when UDP client is closing
            {
                return;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            BeginReceive();
        }

        public void Dispose()
        {
            try
            {
                udpClient.Close();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}
