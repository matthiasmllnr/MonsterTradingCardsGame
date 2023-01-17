using System;
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

		public Card(string type, string name, string element, float damage)
		{
			Type = type;
			Name = name;
			Element = element;
			Damage = damage;
			SaveCardInDB();
		}

		public string GetProperties()
		{
			string s = "";

			s += "Id: " + Id + " ";
			s += "Type: " + Type + " ";
			s += "Name: " + Name + " ";
			s += "Element: " + Element + " ";
			s += "Damage: " + Damage + " ";

			return s;
		}

		// TODO
		private void SaveCardInDB()
		{
			Database db = new Database();

            NpgsqlCommand cmd = db.conn.CreateCommand();
            cmd = db.conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO cards (name, type, element, damage) VALUES ('{Name}', '{Type}', '{Element}', '{Damage}') RETURNING id";
			object? obj = cmd.ExecuteScalar();
			if (obj != null) Id = (int)obj;
			cmd.Dispose();
            db.CloseConnection();
		}
	}
}

