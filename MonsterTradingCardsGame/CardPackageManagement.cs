using System;
using Newtonsoft.Json;
using Npgsql;

namespace MonsterTradingCardsGame
{
	public class CardPackageManagement
	{
		public Dictionary<int, CardPackage> AllCardPackages;
		public const int PACKAGE_COST = 5;

		public CardPackageManagement()
		{
			AllCardPackages = new Dictionary<int, CardPackage>();
			LoadAllCardPackagesDB();
		}

		public bool CreateCardPackage(string data)
		{
			List<Card>? tmpPackage = JsonConvert.DeserializeObject<List<Card>>(data);
			if(tmpPackage != null)
			{
				Console.WriteLine("------------Cards created for Pckg------------");
				Console.WriteLine(tmpPackage[0].GetProperties());
				Console.WriteLine(tmpPackage[1].GetProperties());
				Console.WriteLine(tmpPackage[2].GetProperties());
				Console.WriteLine(tmpPackage[3].GetProperties());
				Console.WriteLine(tmpPackage[4].GetProperties());
                Console.WriteLine("----------------------------------------------");

				CardPackage cardPackage = new CardPackage(tmpPackage);
				AllCardPackages.Add(cardPackage.Id, cardPackage);

				return true;
            }
			return false;
		}

		public string AcquirePackage(User user)
		{
			if(AllCardPackages.Count > 0)
			{
				if (user.Coins < PACKAGE_COST) return "Not enough coins.";

				Random random = new Random();
				int index = random.Next(AllCardPackages.Count);

				KeyValuePair<int, CardPackage> pair = AllCardPackages.ElementAt(index);

                // add Card to users stack and save in DB
                foreach (Card c in pair.Value.Cards)
				{
					user.stack.Add(c);
					SaveInUserCards(user.Id, c.Id);
				}

                // remove package from db and packageManager
                DeletePackageFromDB(pair.Value.Id);
				AllCardPackages.Remove(pair.Value.Id);

                // coins - price
                user.Coins -= PACKAGE_COST;
				user.UpdateUser();
				if (user.deck.Count < 4) user.InitUserDeck();

				return "Package successfully acquired.";
            }
            else
			{
				return "No packages to acquire.";
			}
		}

		private void SaveInUserCards(int userId, int cardId)
		{
			Database db = new Database();
            NpgsqlCommand cmd = db.conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO user_cards (card_id, user_id) VALUES ('{cardId}', '{userId}')";
			cmd.ExecuteNonQuery();
			cmd.Dispose();
            db.CloseConnection();
        }

		private void DeletePackageFromDB(int packageId)
		{
            Database db = new Database();
            NpgsqlCommand cmd = db.conn.CreateCommand();
            cmd.CommandText = $"DELETE FROM packages WHERE id = '{packageId}'";
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            db.CloseConnection();
        }

        private void LoadAllCardPackagesDB()
		{
            Database db = new Database();
            NpgsqlCommand cmdPackage = db.conn.CreateCommand();
            cmdPackage.CommandText = $"SELECT * FROM packages";

			NpgsqlDataReader drPackage = cmdPackage.ExecuteReader();
			List<List<int>> packageIds = new List<List<int>>();

			while (drPackage.Read())
			{
				List<int> cardIds = new List<int>();
				cardIds.Add((int)drPackage[0]);
				cardIds.Add((int)drPackage[1]);
				cardIds.Add((int)drPackage[2]);
				cardIds.Add((int)drPackage[3]);
				cardIds.Add((int)drPackage[4]);
				cardIds.Add((int)drPackage[5]);
				packageIds.Add(cardIds);
            }

			drPackage.Close();
            cmdPackage.Dispose();

            NpgsqlCommand cmdCard = db.conn.CreateCommand();

			foreach(List<int> l in packageIds)
			{
                cmdCard.CommandText = $"SELECT * FROM cards WHERE id IN ('{l[1]}', '{l[2]}', '{l[3]}', '{l[4]}', '{l[5]}')";
                NpgsqlDataReader drCard = cmdCard.ExecuteReader();
                List<Card> cardList = new List<Card>();
                while (drCard.Read())
                {
                    Card c = new Card((int)drCard[0], (string)drCard[1], (string)drCard[2], (string)drCard[3], (float)drCard[4]);
                    cardList.Add(c);
                }
                CardPackage cardPackage = new CardPackage(l[0], cardList);
                AllCardPackages.Add((int)l[0], cardPackage);
                drCard.Close();
            }
            cmdCard.Dispose();
            db.CloseConnection();
        }

    }
}

