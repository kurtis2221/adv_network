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
    public delegate void DataRecieveDelegate(NetworkBase sender, byte[] data);
    public delegate void NetworkExceptionDelegate(NetworkBase sender, Exception ex);

    public abstract class NetworkBase
    {
        public static DataRecieveDelegate tcp_rec_act;
        public static DataRecieveDelegate udp_rec_act;
        public static NetworkExceptionDelegate net_ex_act;

        public int id;

        protected int snd_buffer_size;
        protected int rec_buffer_size;

        protected TcpClient tcp;
        protected UdpClient udp;
        protected NetworkStream tcp_st;
        protected IPEndPoint ep;

        Thread tcp_rec_thd;
        Thread udp_rec_thd;
        protected bool active;

        public NetworkBase(int snd_buffer_size, int rec_buffer_size)
        {
            this.snd_buffer_size = snd_buffer_size;
            this.rec_buffer_size = rec_buffer_size;
        }

        protected void Init(IPEndPoint ep, TcpClient tcp, UdpClient udp)
        {
            active = true;
            this.ep = ep;
            this.tcp = tcp;
            this.udp = udp;
            tcp_st = tcp.GetStream();
            tcp_rec_thd = new Thread(TCP_Rec);
            tcp_rec_thd.Start();
            if (udp != null)
            {
                udp_rec_thd = new Thread(UDP_Rec);
                udp_rec_thd.Start();
            }
        }

        void TCP_Rec()
        {
            try
            {
                byte[] data = new byte[rec_buffer_size];
                int i;
                int len;
                while (active)
                {
                    i = 0;
                    do
                    {
                        len = tcp_st.Read(data, i, rec_buffer_size);
                        i += len;
                    }
                    while (i < rec_buffer_size);
                    tcp_rec_act(this, data);
                }
            }
            catch (Exception ex)
            {
                net_ex_act(this, ex);
            }
        }

        void UDP_Rec()
        {
            while (active)
            {
                try
                {
                    byte[] data = udp.Receive(ref ep);
                    udp_rec_act(this, data);
                }
                catch { }
            }
        }

        public void TCP_Send(byte[] data)
        {
            tcp_st.Write(data, 0, snd_buffer_size);
        }

        public abstract void UDP_Send(byte[] data);
        public abstract void Disconnect();
    }
}