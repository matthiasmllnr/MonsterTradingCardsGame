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
		public string Bio;
		public string Image;
		public string AuthenticationToken;
		public int Coins;
		public int Elo;
		public int Wins;
		public int Losses;
		public string LastBattleLog;
		public List<Card> stack; // all cards
		public List<Card> deck;	// battle deck (4 cards)

		// Default values are set in db when creating a new user
		public User(string username, string password, string token, int coins, string bio, string image, int elo, int wins, int losses)
		{
			Username = username;
			Password = password;
			Bio = bio;  
			Image = image;	
			AuthenticationToken = token;
			Coins = coins;
			Elo = elo;
			Wins = wins;
			Losses = losses;
			LastBattleLog = "No recents battles.";
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

		public void UpdateUser()
		{
			Database db = new Database();
			NpgsqlCommand cmd = db.conn.CreateCommand();
			cmd.CommandText = $"UPDATE users SET " +
							  $"name = '{Username}', " +
							  $"password = '{Password}', " +
							  $"token = '{AuthenticationToken}', " +
							  $"coins = '{Coins}', " +
							  $"elo = '{Elo}', " +
							  $"wins = '{Wins}', " +
							  $"losses = '{Losses}' " +
							  $"WHERE id = '{Id}'";
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
			// get 4 random cards from stack and add to deck
			Random random = new Random();
			int randIndex;
			Card c;
            List<int> usedIndexes = new List<int>();
            for (int i = 0; i < 4; i++)
			{
				randIndex = random.Next(stack.Count);
				while (usedIndexes.Contains(randIndex))
				{
                    randIndex = random.Next(stack.Count);
                }
				usedIndexes.Add(randIndex);
                c = stack[randIndex];
                deck.Add(c);
			}

			// save in db
			Database db = new Database();
            NpgsqlCommand cmd = db.conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO user_decks (user_id, card1_id, card2_id, card3_id, card4_id) VALUES ('{Id}', '{deck[0].Id}', '{deck[1].Id}', '{deck[2].Id}', '{deck[3].Id}')";
			cmd.ExecuteNonQuery();
			cmd.Dispose();
			db.CloseConnection();
        }

		public void LoadDeckFromDB()
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

		public string GetProfile()
		{
			string userProfile = "";

			userProfile += "------------------------------\n";
			userProfile += "Username: " + Username + "\n";
			userProfile += "Bio:      " + Bio + "\n";
			userProfile += "Image:    " + Image + "\n";
			userProfile += "Coins:    " + Coins + "\n";
			userProfile += "Elo:    " + Elo + "\n";
			userProfile += "Wins:    " + Wins + "\n";
			userProfile += "Losses:    " + Losses + "\n";
            userProfile += "------------------------------\n\n";

            return userProfile;
		}

		public void EditProfile(string data)
		{
			ProfileInput? profile = JsonConvert.DeserializeObject<ProfileInput>(data);
			if(profile != null)
			{
				if(profile.Name != null) Username = profile.Name;
				if(profile.Bio != null) Bio = profile.Bio;
				if(profile.Image != null) Image = profile.Image;
			}
		}

		public string GetStats()
		{
			string stats = "";
            stats += "------------------------------\n";
            stats += "Elo: " + Elo + "\n";
            stats += "Wins: " + Wins + "\n";
            stats += "Losses: " + Losses + "\n";
            stats += "------------------------------\n\n";

            return stats;
        }

		// only temporary used when using JsonConvert
		private class ProfileInput
		{
			public string Name;
			public string Bio;
			public string Image;

			[JsonConstructor]
			ProfileInput(string name, string bio, string image)
			{
				Name = name;
				Bio = bio;
				Image = image;
			}
		}

		// selects a random card from the users deck and removes it
		public Card GetRandomCardFromDeck()
		{
			Random random = new Random();
			int randomIndex = random.Next(deck.Count);
			Card c = deck[randomIndex];
			deck.RemoveAt(randomIndex);
			return c;
		}

		public bool IsCardInDeck(int cId)
		{
			foreach(Card c in deck)
			{
				if (c.Id == cId) return true;
			}
			return false;
		}

        public bool IsCardInStack(int cId)
        {
            foreach (Card c in stack)
            {
                if (c.Id == cId) return true;
            }
            return false;
        }

		public void RemoveCardFromStack(int cId)
		{
			int i = 0;
			foreach(Card c in stack)
			{
				if (c.Id == cId) break;
				i++;
			}
            stack.RemoveAt(i);
        }

    }
}

