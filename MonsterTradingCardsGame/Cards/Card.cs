using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace MonsterTradingCardsGame
{
	public class Card
	{
		public int Id;
        public string Type;
        public string Name;
        public string Element;
        public float Damage;

		[JsonConstructor]
		public Card(string name, string type, string element, float damage)
		{
			Type = type;
			Name = name;
			Element = element;
			Damage = damage;
			SaveCardInDB();
		}

        public Card(int id, string name, string type, string element, float damage)
        {
			Id = id;
            Type = type;
            Name = name;
            Element = element;
            Damage = damage;
        }

        public string GetProperties()
		{
			string s = "";

			s += "Id: " + Id + " | ";
			s += "Type: " + Type + " | ";
			s += "Name: " + Name + " | ";
			s += "Element: " + Element + " | ";
			s += "Damage: " + Damage;

			return s;
		}

		private void SaveCardInDB()
		{
			Database db = new Database();

            NpgsqlCommand cmd = db.conn.CreateCommand();
            cmd = db.conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO cards (name, type, element, damage) VALUES (@name, @type, @element, @damage) RETURNING id";
			cmd.Parameters.AddWithValue("@name", Name);
			cmd.Parameters.AddWithValue("@type", Type);
			cmd.Parameters.AddWithValue("@element", Element);
			cmd.Parameters.AddWithValue("@damage", Damage);
			object? obj = cmd.ExecuteScalar();
			if (obj != null) Id = (int)obj;
			cmd.Dispose();
            db.CloseConnection();
		}
	}
}

