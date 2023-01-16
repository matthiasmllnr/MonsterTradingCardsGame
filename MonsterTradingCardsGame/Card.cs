using System;
namespace MonsterTradingCardsGame
{
	public class Card
	{
		private int Id;
		private string Type;
		private string Name;
		private string Element;
		private float Damage;

		public Card(string type, string name, string element, float damage)
		{
			Type = type;
			Name = name;
			Element = element;
			Damage = damage;

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
		private string SaveCardInDB()
		{
			return "";
		}
	}
}

