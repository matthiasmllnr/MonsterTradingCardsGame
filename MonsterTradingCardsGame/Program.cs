using System;
using System.Data;
using MonsterTradingCardsGame;
using Npgsql;


///////////////////////////////////////////////////////////////////////////////
/// MAIN

var cs = "Host=localhost;Username=postgres;Password=admin;Database=monsterTradingCards";
Database db = new Database(cs);

HttpServer server = new HttpServer();

server.Incoming += _Svr_Incoming;

server.Run();

void _Svr_Incoming(object sender, HttpServerEventArgs e)
{
    // Prints Payload
    Console.WriteLine("-----Payload------");
    foreach (string key in e.Payload.Keys)
    {
        Console.WriteLine(key + " : " + e.Payload[key]);
    }
    Console.WriteLine("Path: " + e.Path);
    Console.WriteLine("------------------");
    Console.WriteLine();

    try
    {
        switch (e.Path)
        {
            case "/users":

                switch (e.Method)
                {
                    case "POST":
                        db.CreateUser(e.Payload);
                        e.Reply(200, "Created User.");
                        break;
                }

                break;

            case "/sessions":

                switch (e.Method)
                {
                    case "POST":
                        string token = db.LoginUser(e.Payload);
                        if (token != "-")
                        {
                            e.Reply(200, $"User successfully logged in. \nToken: {token}");
                        } else
                        {
                            e.Reply(409, "Invalid username or password!");
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
            Console.WriteLine("Unique Constraint got violated");
        }
    }


}
