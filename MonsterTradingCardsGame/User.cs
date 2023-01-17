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
		public int Coins;
		public List<Card> stack; // all cards
		public List<Card> deck;	// battles deck (4 cards)


		public User(string username, string password, string token)
		{
			Username = username;
			Password = password;
			AuthenticationToken = token;
			Coins = 20;
			stack = new List<Card>();
			deck = new List<Card>();
			GetUserIdFromDB();
			LoadStackFromDB();
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

		public void UpdateUser()
		{
			Database db = new Database();
			NpgsqlCommand cmd = db.conn.CreateCommand();
			cmd.CommandText = $"UPDATE users SET name = '{Username}', password = '{Password}', token = '{AuthenticationToken}', coins = '{Coins}' WHERE id = '{Id}'";
			cmd.ExecuteNonQuery();
			db.CloseConnection();
		}

		public void LoadStackFromDB()
		{
            Database db = new Database();
            NpgsqlCommand cmd = db.conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM user_cards WHERE user_id = '{Id}'";

			NpgsqlDataReader dr = cmd.ExecuteReader();
			List<int> cardIds = new List<int>();
			while (dr.Read())
			{
				cardIds.Add((int)dr[0]);
			}
			dr.Close();

			foreach(int cardId in cardIds)
			{
                cmd.CommandText = $"SELECT * FROM cards WHERE id = '{cardId}'";
				dr = cmd.ExecuteReader();
				if (dr.Read())
				{
					Card c = new Card((int)dr[0], (string)dr[1], (string)dr[2], (string)dr[3], (float)dr[4]);
					stack.Add(c);
				}
				dr.Close();
            }

            cmd.Dispose();
            db.CloseConnection();
        }

		public string GetStack()
		{
			string output = "-----/ " + Username + "s Stack \\-----\n";
			foreach(Card c in stack)
			{
				output += c.GetProperties() + "\n";
			}
			output += "------------------------------------------------------";
			return output;
		}

    }
}

