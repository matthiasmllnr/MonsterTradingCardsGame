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
			LoadDeckFromDB();
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
			output += "------------------------------------------------------\n";
			return output;
		}

        public void InitUserDeck()
        {
			Console.WriteLine("in initUserDeck");
			// get 4 random cards from stack and add to deck
			Random random = new Random();
			int randIndex;
			Card c;
			for(int i = 0; i < 4; i++)
			{
				randIndex = random.Next(stack.Count);
				c = stack[randIndex];
				while (deck.Contains(c))
				{
                    c = stack[randIndex];
                }
				deck.Add(c);
			}
			Console.WriteLine("nach deck adden");

			// save in db
			Database db = new Database();
            NpgsqlCommand cmd = db.conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO user_decks (user_id, card1_id, card2_id, card3_id, card4_id) VALUES ('{Id}', '{deck[0].Id}', '{deck[1].Id}', '{deck[2].Id}', '{deck[3].Id}')";
			cmd.ExecuteNonQuery();
			cmd.Dispose();
			db.CloseConnection();
			Console.WriteLine("nach db save");
        }

		private void LoadDeckFromDB()
		{
            Database db = new Database();
            NpgsqlCommand cmd = db.conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM user_decks WHERE user_id = '{Id}'";
			NpgsqlDataReader dr = cmd.ExecuteReader();
			if (dr.Read())
			{
				List<int> cardIds = new List<int>();
                deck.Clear();
				cardIds.Add((int)dr[1]);
				cardIds.Add((int)dr[2]);
				cardIds.Add((int)dr[3]);
				cardIds.Add((int)dr[4]);
				Card? c;
				foreach(int id in cardIds)
				{
					c = GetCardFromStackByID(id);
					if (c != null) deck.Add(c);
				}
            }

			dr.Close();
			cmd.Dispose();
			db.CloseConnection();
        }

		public Card? GetCardFromStackByID(int id)
		{
			foreach(Card card in stack)
			{
				if (card.Id.Equals(id))
				{
					return card;
				}
			}

			return null;
		}

		public string GetDeck()
		{
            string output = "-----/ " + Username + "s Deck \\-----\n";
            foreach (Card c in deck)
            {
                output += c.GetProperties() + "\n";
            }
            output += "------------------------------------------------------\n";
            return output;
        }

        public bool ConfigureDeck(string data)
		{
            List<int>? cardIds = JsonConvert.DeserializeObject<List<int>>(data);
			bool cardsOwnedByUser = CheckCardsOwned(cardIds);
			if(cardIds != null && cardIds.Count == 4 && cardsOwnedByUser)
			{
				Database db = new Database();
				NpgsqlCommand cmd = db.conn.CreateCommand();
				cmd.CommandText = $"UPDATE user_decks SET card1_id = '{cardIds[0]}', card2_id = '{cardIds[1]}', card3_id = '{cardIds[2]}', card4_id = '{cardIds[3]}' WHERE user_id = '{Id}'";
				cmd.ExecuteNonQuery();
				cmd.Dispose();
				db.CloseConnection();
				LoadDeckFromDB();
				return true;
            }
			return false;
		}

		private bool CheckCardsOwned(List<int>? cardIds)
		{
			if(cardIds != null)
			{
                foreach (int id in cardIds)
                {
                    Card? c = GetCardFromStackByID(id);
                    if (c == null) return false;
                }
            }
			return true;
		}

    }
}

