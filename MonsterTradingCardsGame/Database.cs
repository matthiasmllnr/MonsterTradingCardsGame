using System;
using System.Data;
using Npgsql;
using System.Collections;
using Newtonsoft.Json;

namespace MonsterTradingCardsGame
{
	public class Database
	{
        private readonly string connectionString = "Host=localhost;Username=postgres;Password=admin;Database=monsterTradingCards";
        public NpgsqlConnection conn;

        public Database()
		{
            conn = new NpgsqlConnection(connectionString);
            conn.Open();
        }

        public void CloseConnection()
        {
            conn.Close();
        }
    }
}

