using System;
using Newtonsoft.Json;
using System.Data;
using Npgsql;

namespace MonsterTradingCardsGame
{
	public class User
	{
		public int Id;
		public string Username;
		public string Password;
		public string AuthenticationToken;
		private int Coins;
		private List<Card> stack;   // all cards
		private List<Card> deck;	// battles deck (4 cards)


		public User(string username, string password, string token)
		{
			Username = username;
			Password = password;
			AuthenticationToken = token;
			Coins = 20;
			stack = new List<Card>();
			deck = new List<Card>();
			GetUserIdFromDB();
		}

		private void GetUserIdFromDB()
		{
			Database db = new Database();

			NpgsqlCommand cmd = db.conn.CreateCommand();
			cmd.CommandText = $"SELECT id FROM users WHERE name = '{Username}'";
            NpgsqlDataReader dr = cmd.ExecuteReader();
			if (dr.Read())
			{
				Id = (int)dr[0];
			}
			dr.Close();
            db.CloseConnection();
		}

		public void PrintUser()
		{
			Console.WriteLine("Id: " + Id);
			Console.WriteLine("Username: " + Username);
			Console.WriteLine("Password: " + Password);
			Console.WriteLine("Token: " + AuthenticationToken);
			Console.WriteLine("Coins: " + Coins);
		}

    }
}

