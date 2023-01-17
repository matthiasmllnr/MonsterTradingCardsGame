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
        List<HttpHeader> h;
        User? user;
        string token;
        switch (e.Path)
        {
            case "/users":

                switch (e.Method)
                {
                    case "POST":
                        server.UserManager.CreateUser(e.Data);
                        e.Reply(200, "Created User.");
                        break;
                }

                break;

            case "/sessions":

                switch (e.Method)
                {
                    case "POST":
                        token = server.UserManager.LoginUser(e.Data);
                        if (token != "-")
                        {
                            e.Reply(200, $"User successfully logged in. \nSession-Token: {token}");
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
                        h = e.Headers.ToList();
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
                switch (e.Method)
                {
                    case "POST":
                        h = e.Headers.ToList();
                        token = h[h.Count-2].Value;
                        Console.WriteLine(token);
                        user = server.UserManager.GetUser(token);
                        if(user != null)
                        {
                            string success = server.CardPackageManager.AcquirePackage(user);
                            e.Reply(200, "Acquire package: " + success);
                        }
                        else
                        {
                            e.Reply(409, "Acquiring package failed! User not logged in.");
                        }

                        break;
                }

                break;

            case "/cards":
                switch (e.Method)
                {
                    case "GET":
                        h = e.Headers.ToList();
                        token = h[h.Count-1].Value;
                        user = server.UserManager.GetUser(token);
                        if (user != null)
                        {
                            string userStack = user.GetStack();
                            e.Reply(200, userStack);
                        }
                        else
                        {
                            e.Reply(409, "Showing all acquired cards failed! User not logged in.");
                        }

                        break;
                }
                break;

            case "/deck":

                switch (e.Method)
                {
                    case "GET":
                        h = e.Headers.ToList();
                        token = h[h.Count - 1].Value;
                        user = server.UserManager.GetUser(token);
                        if (user != null)
                        {
                            string userDeck = user.GetDeck();
                            e.Reply(200, userDeck);
                        }
                        else
                        {
                            e.Reply(409, "Showing deck failed! User not logged in.");
                        }

                        break;

                    case "PUT":
                        h = e.Headers.ToList();
                        token = h[h.Count - 2].Value;
                        user = server.UserManager.GetUser(token);
                        if (user != null)
                        {
                            bool success = user.ConfigureDeck(e.Data);
                            if (success)
                            {
                                e.Reply(200, "Successfully configured deck.");
                            }
                            else
                            {
                                e.Reply(409, "Configuring deck failed! Invalid Input.");
                            }
                        }
                        else
                        {
                            e.Reply(409, "Configuring deck failed! User not logged in.");
                        }

                        break;
                }
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
    catch(Exception exp)
    {
        e.Reply(409, exp.Message);
    }


}
