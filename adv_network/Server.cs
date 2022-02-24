using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;

namespace adv_network
{
    public class ServerConn : NetworkBase
    {
        private ServerConn(int snd_buffer_size, int rec_buffer_size)
            : base(snd_buffer_size, rec_buffer_size)
        {
        }

        public static bool GetNewConnection(TcpListener srv, UdpClient udp, int port,
            int snd_buffer_size, int rec_buffer_size,
            bool full, out ServerConn con)
        {
            con = null;
            try
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
                TcpClient tcp = srv.AcceptTcpClient();
                if (full)
                {
                    tcp.Close();
                    return false;
                }
                con = new ServerConn(snd_buffer_size, rec_buffer_size);
                con.Init(ep, tcp, udp);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void UDP_Send(byte[] data)
        {
            try
            {
                udp.Send(data, snd_buffer_size, ep);
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
            }
            catch
            {
            }
        }
    }

    public class Server
    {
        int id;
        int port;
        int snd_buffer_size;
        int rec_buffer_size;
        bool en_udp;

        TcpListener srv;
        UdpClient udp;
        List<ServerConn> con_list;
        Stack<int> free_ids;
        Thread thd;

        public Server(int port, int slots, int snd_buffer_size, int rec_buffer_size, bool en_udp)
        {
            this.port = port;
            this.snd_buffer_size = snd_buffer_size;
            this.rec_buffer_size = rec_buffer_size;
            this.en_udp = en_udp;
            con_list = new List<ServerConn>(slots);
            free_ids = new Stack<int>(slots);
            srv = new TcpListener(System.Net.IPAddress.Any, port);
            srv.Start();
            udp = en_udp ? new UdpClient(port) : null;
            thd = new Thread(HandleConnections);
            thd.Start();
        }

        public void RemoveConnection(ServerConn con)
        {
            free_ids.Push(con.id);
            con.Disconnect();
            con_list.Remove(con);
        }

        void HandleConnections()
        {
            ServerConn con;
            while (true)
            {
                if (ServerConn.GetNewConnection(srv, udp, port, snd_buffer_size, rec_buffer_size,
                    con_list.Count >= con_list.Capacity, out con))
                {
                    con.id = free_ids.Count > 0 ? free_ids.Pop() : id++;
                    con_list.Add(con);
                }
            }
        }

        public void Send_TCP(byte[] data, NetworkBase ignore)
        {
            foreach (ServerConn con in con_list)
            {
                if (con != ignore) con.TCP_Send(data);
            }
        }

        public void Send_UDP(byte[] data, NetworkBase ignore)
        {
            foreach (ServerConn con in con_list)
            {
                if (con != ignore) con.UDP_Send(data);
            }
        }
    }
}