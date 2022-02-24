using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Net;

namespace adv_network
{
    public class Client : NetworkBase
    {
        public Client(string host, int port, int snd_buffer_size, int rec_buffer_size, bool en_udp)
            : base(snd_buffer_size, rec_buffer_size)
        {
            TcpClient tcp = new TcpClient(host, port);
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            UdpClient udp = en_udp ? new UdpClient(host, port) : null;
            Init(ep, tcp, udp);
        }

        public override void UDP_Send(byte[] data)
        {
            try
            {
                udp.Send(data, snd_buffer_size);
            }
            catch { }
        }

        public override void Disconnect()
        {
            try
            {
                active = false;
                tcp_st.Close();
                tcp.Close();
                if (udp != null) udp.Close();
            }
            catch
            {
            }
        }
    }
}