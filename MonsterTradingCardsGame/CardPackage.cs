using System;
using Npgsql;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

namespace MonsterTradingCardsGame
{
	public class CardPackage
	{
		public int Id;
		public List<Card> Cards;

		public CardPackage(List<Card> listCards)
		{
            Cards = new List<Card>();
			foreach(Card c in listCards)
			{
				Cards.Add(c);
			}
			SaveInDB();
		}

		public void SaveInDB()
		{
			Database db = new Database();

            NpgsqlCommand cmd = db.conn.CreateCommand();
            cmd = db.conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO packages (card1_id, card2_id, card3_id, card4_id, card5_id) VALUES ('{Cards[0].Id}', '{Cards[1].Id}', '{Cards[2].Id}', '{Cards[3].Id}', '{Cards[4].Id}') RETURNING id";
            object? obj = cmd.ExecuteScalar();
            if (obj != null) Id = (int)obj;
            cmd.Dispose();

            db.CloseConnection();
        }
    }
}

