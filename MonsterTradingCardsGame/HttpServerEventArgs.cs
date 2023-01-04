using System;
using System.Collections;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;

namespace MonsterTradingCardsGame
{
    public class HttpServerEventArgs
    {

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members                                                                                          //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>TCP Client</summary>
        private TcpClient _Client { set; get; }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                             //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="tcp">HTTP message received from TCP listener.</param>
        public HttpServerEventArgs(string tcp, TcpClient tcpClient)
        {
            string[] lines = tcp.Replace("\r\n", "\n").Replace("\r", "\n").Split("\n");
            bool inheaders = true;
            _Client = tcpClient;
            List<HttpHeader> headers = new List<HttpHeader>();
            Payload = new Hashtable();

            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    string[] inq = lines[0].Split(" ");
                    Method = inq[0];
                    Path = inq[1];
                }
                else if (inheaders)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                    {
                        inheaders = false;
                    }
                    else { headers.Add(new HttpHeader(lines[i])); }
                }
                else
                {
                    // generate hash table
                    // lines[i]: {"Username":"kienboec", "Password":"daniel"}
                    AddToPayload(lines[i]);
                    //Payload += (lines[i] + "\r\n");
                }

                Headers = headers.ToArray();
            }
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                        //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Get the HTTP method.</summary>
        public string Method
        {
            get; private set;
        }


        /// <summary>Gets the URL path.</summary>
        public string Path
        {
            get; private set;
        }


        /// <summary>Gets the HTTP headers.</summary>
        public HttpHeader[] Headers
        {
            get; private set;
        }


        /// <summary>Gets the HTTP payload.</summary>
        public Hashtable Payload
        {
            get; private set;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Reply(int status, string payload)
        {
            string data;

            switch (status)
            {
                case 200:
                    data = "HTTP/1.1 200 OK\n";
                    break;
                case 409:
                    data = "HTTP/1.1 409 Conflict\n";
                    break;
                default:
                    data = "HTTP/1.1 418 I'm a Teapot\n";
                    break;
            }

            if (string.IsNullOrEmpty(payload))
            {
                data += "Content-Length: 0\n";
            }
            data += "Content-Type: text/plain\n\n";

            if (payload != null)
            {
                data += payload;
            }

            byte[] dbuf = Encoding.ASCII.GetBytes(data);
            _Client.GetStream().Write(dbuf, 0, dbuf.Length);                    // send a response

            _Client.GetStream().Close();                                        // shut down the connection
            //_Client.Dispose();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private methods                                                                                          //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void AddToPayload(string line)
        {
            // line: {"Username":"kienboec", "Password":"daniel"}
            line = line.Replace("\"", "");
            line = line.Replace("{", "");
            line = line.Replace("}", "");
            line = line.Replace(" ", "");

            string[] keyValuePairs = line.Split(",");
            foreach (string pair in keyValuePairs)
            {
                string[] splitted = pair.Split(":");
                Payload.Add(splitted[0], splitted[1]);
            }
        }

    }
}

