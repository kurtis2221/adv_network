using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using adv_network;

namespace demo
{
    class Program
    {
        static Client cli;
        static Server srv;
        static byte[] data;
        static byte tcp_by, udp_by;

        static void Main(string[] args)
        {
            Console.Write("Server or Client?(S)");
            NetworkBase.tcp_rec_act = TCP_Rec;
            NetworkBase.udp_rec_act = UDP_Rec;
            NetworkBase.net_ex_act = Net_Ex;
            ConsoleKey ck = Console.ReadKey().Key;
            Console.WriteLine();
            if (ck == ConsoleKey.C)
            {
                Console.Write("TCP byte:");
                tcp_by = byte.Parse(Console.ReadLine());
                Console.Write("UDP byte:");
                udp_by = byte.Parse(Console.ReadLine());
                Console.WriteLine("Connecting...");
                cli = new Client("127.0.0.1", 7777, 1, 2, true);
                Console.WriteLine("Connected");
                SendLoop();
            }
            else
            {
                Console.WriteLine("Starting server");
                srv = new Server(7777, 4, 2, 1, true);
                Console.WriteLine("Listening...");
            }
        }

        static void SendLoop()
        {
            try
            {
                data = new byte[1];
                while (true)
                {
                    Thread.Sleep(1000);
                    data[0] = tcp_by;
                    cli.TCP_Send(data);
                    data[0] = udp_by;
                    cli.UDP_Send(data);
                }
            }
            catch
            {
                cli.Disconnect();
            }
        }

        static void TCP_Rec(NetworkBase sender, byte[] data)
        {
            if (cli == null)
            {
                Console.WriteLine("TCP: ID: " + sender.id + ", Data: " + data[0]);
                byte[] tmp = new byte[data.Length + 1];
                Array.Copy(data, 0, tmp, 1, data.Length);
                tmp[0] = (byte)sender.id;
                srv.Send_TCP(tmp, null);
            }
            else
            {
                Console.WriteLine("TCP: ID: " + data[0] + ", Data: " + data[1]);
            }
        }

        static void UDP_Rec(NetworkBase sender, byte[] data)
        {
            if (cli == null)
            {
                Console.WriteLine("UDP: ID: " + sender.id + ", Data: " + data[0]);
                byte[] tmp = new byte[data.Length + 1];
                Array.Copy(data, 0, tmp, 1, data.Length);
                tmp[0] = (byte)sender.id;
                srv.Send_UDP(tmp, null);
            }
            else
            {
                Console.WriteLine("UDP: ID: " + data[0] + ", Data: " + data[1]);
            }
        }

        static void Net_Ex(NetworkBase sender, Exception ex)
        {
            Console.WriteLine(ex.Message);
            if (cli == null)
            {
                srv.RemoveConnection((ServerConn)sender);
            }
            else
            {
                cli.Disconnect();
            }
        }
    }
}