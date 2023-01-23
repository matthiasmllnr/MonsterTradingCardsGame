using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace MonsterTradingCardsGame
{

    public delegate void IncomingEventHandler(object sender, HttpServerEventArgs e);

    public class HttpServer
	{
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members                                                                                          //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private TcpListener? _Listener;
        public UserManagement UserManager;
        public CardPackageManagement CardPackageManager;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public events                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///
        public event IncomingEventHandler? Incoming;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public HttpServer()
        {
            UserManager = new UserManagement();
            CardPackageManager = new CardPackageManagement();
        }

        /// <summary>Runs the server.</summary>
        /// <remarks>Probably won't stay this way forever.</remarks>
        public void Run()
        {
            _Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 10001); // p:12000
            _Listener.Start();

            byte[] buf = new byte[256];
            int n;
            string data;

            while (true)
            {
                Console.WriteLine("Waiting for connections...");
                TcpClient client = _Listener.AcceptTcpClient();                 // wait for a client to connect
                Console.WriteLine("Client connected!");

                NetworkStream stream = client.GetStream();                      // get the client stream

                data = "";
                while (stream.DataAvailable || data == "")
                {                                                               // read and decode stream
                    n = stream.Read(buf, 0, buf.Length);
                    data += Encoding.ASCII.GetString(buf, 0, n);
                }

                Console.WriteLine("--------Data--------");
                Console.WriteLine(data);                                        // write decoded data to console
                Console.WriteLine("--------------------");
                Console.WriteLine();

                Incoming?.Invoke(this, new HttpServerEventArgs(data, client));

            }
        }

        public string GetAuthToken(List<HttpHeader> headers)
        {
            string token = "-";

            foreach(HttpHeader h in headers)
            {
                if (h.Name.Equals("Authorization"))
                {
                    token = h.Value;
                }
            }

            return token;
        }
    }
}

