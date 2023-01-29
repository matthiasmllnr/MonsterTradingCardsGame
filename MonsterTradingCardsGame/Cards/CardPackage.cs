using System;
using Npgsql;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace MonsterTradingCardsGame
{
	public class CardPackage
	{
		public int Id;
		public List<Card> Cards;

        [JsonConstructor]
		public CardPackage(List<Card> listCards)
		{
            Cards = new List<Card>();
			foreach(Card c in listCards)
			{
				Cards.Add(c);
			}
			SaveInDB();
		}

        // Constructor without Save in DB
        public CardPackage(int id, List<Card> listCards)
        {
            Id = id;
            Cards = new List<Card>();
            foreach (Card c in listCards)
            {
                Cards.Add(c);
            }
        }

        public void SaveInDB()
		{
			Database db = new Database();

            NpgsqlCommand cmd = db.conn.CreateCommand();
            cmd = db.conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO packages (card1_id, card2_id, card3_id, card4_id, card5_id) VALUES (@card0, @card1, @card2, @card3, @card4) RETURNING id";
            cmd.Parameters.AddWithValue("@card0", Cards[0].Id);
            cmd.Parameters.AddWithValue("@card1", Cards[1].Id);
            cmd.Parameters.AddWithValue("@card2", Cards[2].Id);
            cmd.Parameters.AddWithValue("@card3", Cards[3].Id);
            cmd.Parameters.AddWithValue("@card4", Cards[4].Id);
            object? obj = cmd.ExecuteScalar();
            if (obj != null) Id = (int)obj;
            cmd.Dispose();

            db.CloseConnection();
        }
    }
}

