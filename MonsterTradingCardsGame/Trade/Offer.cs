using System;
using Newtonsoft.Json;
using Npgsql;

namespace MonsterTradingCardsGame
{
	public class Offer
	{
		public int Id;
		public int UserId;
		public int CardId;
		public string CardName;
		public string CardType;
		public float MinDamage;

		// Constructor to use when creating new offer
		public Offer(int uId, int cId, string name, string type, float md)
		{
			UserId = uId;
			CardId = cId;
			CardName = name;
			CardType = type;
			MinDamage = md;
			SaveInDB();
		}

		// Constructor to use when loading from DB
		[JsonConstructor]
        public Offer(int id, int uId, int cId, string name, string type, float md)
        {
			Id = id;
            UserId = uId;
            CardId = cId;
			CardName = name;
            CardType = type;
            MinDamage = md;
        }

		// saves object in database and sets Id
		private void SaveInDB()
		{
			Database db = new Database();
			NpgsqlCommand cmd = db.conn.CreateCommand();
			cmd.CommandText = "INSERT INTO trades " +
							  "(user_id, card_id, card_name, card_type, min_damage) " +
							  "VALUES " +
							  $"(@userId, @cardId, @cardName, @cardType, @minDamage) " +
							  "RETURNING id";
			cmd.Parameters.AddWithValue("@userId", UserId);
			cmd.Parameters.AddWithValue("@cardId", CardId);
			cmd.Parameters.AddWithValue("@cardName", CardName);
			cmd.Parameters.AddWithValue("@cardType", CardType);
			cmd.Parameters.AddWithValue("@minDamage", MinDamage);
			Object? res = cmd.ExecuteScalar();
			if (res != null) Id = (int)res;
			cmd.Dispose();
			db.CloseConnection();
		}

		public void DeleteFromDB()
		{
			Database db = new Database();
			NpgsqlCommand cmd = db.conn.CreateCommand();
			cmd.CommandText = $"DELETE FROM trades WHERE id = @id";
			cmd.Parameters.AddWithValue("@id", Id);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
			db.CloseConnection();
		}

		public List<string> GetCardProperties(int cId, int uId)
		{
			Database db = new Database();
			NpgsqlCommand cmd = db.conn.CreateCommand();
			cmd.CommandText = $"SELECT * FROM cards WHERE id = @cId";
			cmd.Parameters.AddWithValue("@cId", cId);
			NpgsqlDataReader dr = cmd.ExecuteReader();
			List<string> l = new List<string>();
			if (dr.Read())
			{
				l.Add((string)dr[1]); // name
                l.Add((string)dr[3]); // element
                l.Add(((float)dr[4]).ToString()); // damage
            }
			dr.Close();
			cmd.Dispose();
			db.CloseConnection();
			return l;
		}
    }
}

