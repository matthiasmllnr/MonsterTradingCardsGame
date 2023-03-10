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
    Thread t = new Thread(x => RunClient(e));
    t.Start();
}

void RunClient(HttpServerEventArgs e)
{
    try
    {
        List<HttpHeader> h;
        User? user;
        string token;
        string path = e.Path;
        string offerId;

        if (e.Path.StartsWith("/users/")) path = "/users";
        if (e.Path.StartsWith("/tradings/")) path = "/tradings";

        h = e.Headers.ToList();
        token = server.GetAuthToken(h);
        user = server.UserManager.GetUser(token);

        switch (path)
        {
            case "/users":

                switch (e.Method)
                {
                    case "POST":
                        server.UserManager.CreateUser(e.Data);
                        e.Reply(200, "Created User.");
                        break;

                    case "GET":
                        if (user != null)
                        {
                            string userProfile = user.GetProfile();
                            e.Reply(200, userProfile);
                        }
                        else
                        {
                            e.Reply(409, "Showing profile failed! User not logged in.");
                        }
                        break;

                    case "PUT":
                        if (user != null)
                        {
                            user.EditProfile(e.Data);
                            e.Reply(200, "Profile edited.");
                        }
                        else
                        {
                            e.Reply(409, "Editing profile failed! User not logged in.");
                        }
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
                        }
                        else
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
                        bool authorized = server.UserManager.IsAuthorized(token, true);
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
                        }
                        else
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
                        if (user != null)
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
                        if (user != null)
                        {
                            string userStack = user.GetStack();
                            e.Reply(200, userStack);
                        }
                        else
                        {
                            e.Reply(409, "Showing all acquired cards failed! User not logged in.\\n");
                        }

                        break;
                }
                break;

            case "/deck":

                switch (e.Method)
                {
                    case "GET":
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

            case "/stats":

                switch (e.Method)
                {
                    case "GET":

                        if (user != null)
                        {
                            string stats = user.GetStats();
                            e.Reply(200, stats);
                        }
                        else
                        {
                            e.Reply(409, "Showing stats failed! User not logged in.");
                        }

                        break;
                }

                break;

            case "/score":

                switch (e.Method)
                {
                    case "GET":

                        if (user != null)
                        {
                            string scoreboard = server.UserManager.GetScoreboard();
                            e.Reply(200, scoreboard);
                        }
                        else
                        {
                            e.Reply(409, "Showing scoreboard failed! User not logged in.");
                        }

                        break;
                }

                break;

            case "/battles":

                switch (e.Method)
                {
                    case "POST":

                        if (user != null)
                        {
                            server.Battle.AddPlayer(user);
                            if (server.Battle.EnoughPlayers())
                            {
                                string battleResult = server.Battle.StartBattle();
                                e.Reply(200, battleResult);
                            }
                            else
                            {
                                e.Reply(200, " In Queue: Waiting for opponent.");
                            }
                        }
                        else
                        {
                            e.Reply(409, "Can't queue for battle! User not logged in.");
                        }

                        break;

                    case "GET":

                        if (user != null)
                        {
                            e.Reply(200, user.LastBattleLog);
                        }
                        else
                        {
                            e.Reply(409, "Can't show last battle log! User not logged in.");
                        }

                        break;
                }

                break;

            case "/tradings":

                switch (e.Method)
                {
                    case "GET":

                        if (user != null)
                        {
                            string tradingDeals = server.TradeHandler.GetTradingDeals();
                            e.Reply(200, tradingDeals);
                        }
                        else
                        {
                            e.Reply(409, "Showing tradings failed! User not logged in.");
                        }

                        break;

                    case "POST":

                        // create/trade
                        offerId = "-";
                        if (e.Path.Length > path.Length)
                        {
                            offerId = e.Path.Substring(path.Length + 1);
                        }

                        if (user != null)
                        {

                            if (offerId != "-")
                            {
                                // trade
                                Console.WriteLine(e.Data);
                                string response = server.TradeHandler.Trade(Int32.Parse(offerId), user, Int32.Parse(e.Data), server.UserManager);
                                e.Reply(200, response);
                            }
                            else
                            {
                                // create offer
                                string response = server.TradeHandler.AddOffer(e.Data, user);
                                e.Reply(200, response);
                            }
                        }
                        else
                        {
                            e.Reply(409, "Creating/Trading failed! User not logged in.");
                        }

                        break;

                    case "DELETE":

                        if (user != null)
                        {
                            offerId = "-";
                            if (e.Path.Length > path.Length)
                            {
                                offerId = e.Path.Substring(path.Length + 1);
                            }
                            Console.WriteLine(offerId);
                            string deleteOffer = server.TradeHandler.RemoveOffer(Int32.Parse(offerId), user.Id);
                            e.Reply(200, deleteOffer);
                        }
                        else
                        {
                            e.Reply(409, "Deleting trade failed! User not logged in.");
                        }

                        break;
                }

                break;
        }
    }
    catch (NpgsqlException ex)
    {
        if (ex.SqlState == "23505")
        {
            // Unique Constraint Violation
            e.Reply(409, "Username already exists!");
        }
    }
    catch (Exception exp)
    {
        e.Reply(409, exp.Message);
    }
}
