using System;
using System.Data;
using MonsterTradingCardsGame;
using Npgsql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

///////////////////////////////////////////////////////////////////////////////
/// MAIN


HttpServer server = new HttpServer();

server.Incoming += _Svr_Incoming;

server.Run();

void _Svr_Incoming(object sender, HttpServerEventArgs e)
{

    try
    {
        switch (e.Path)
        {
            case "/users":

                switch (e.Method)
                {
                    case "POST":
                        Console.WriteLine("before create");
                        server.UserManager.CreateUser(e.Data);
                        e.Reply(200, "Created User.");
                        break;
                }

                break;

            case "/sessions":

                switch (e.Method)
                {
                    case "POST":
                        string token = server.UserManager.LoginUser(e.Data);
                        if (token != "-")
                        {
                            e.Reply(200, $"User successfully logged in. \nToken: {token}");
                            server.UserManager.PrintUsers();
                        } else
                        {
                            e.Reply(409, "Invalid username or password!");
                        }
                        
                        break;
                }

                break;

            case "/packages":

                switch (e.Method)
                {
                    case "POST":
                        List<HttpHeader> h = e.Headers.ToList();
                        Console.WriteLine("Token: " + h[4].Value);
                        bool authorized = server.UserManager.IsAuthorized(h[4].Value, true);
                        if (authorized)
                        {
                            bool success = server.CardPackageManager.CreateCardPackage(e.Data);
                            if (success)
                            {
                                e.Reply(200, "Package successfully created.");
                            }
                            else
                            {
                                e.Reply(409, "Package creation failed.");
                            }
                        } else
                        {
                            e.Reply(409, "Authentication failed! User not authorized or logged in.");
                        }
                        break;
                }

                break;

            case "/transactions/packages":

                break;

            case "/cards":

                break;

            case "/deck":

                break;

            case "/deck?format=plain":

                break;

        }
    }
    catch (NpgsqlException ex)
    {
        if (ex.SqlState == "23505")
        {
            // Unique Constraint Violation
            e.Reply(409, "Username already exists!");
            Console.WriteLine("Unique Constraint got violated");
        }
    }


}
