using System;
namespace MonsterTradingCardsGame
{
	public class CardPackage
	{
		private int Id;
		private List<Card> Cards;

		public CardPackage()
		{
            Cards = new List<Card>();
		}

		public void AddCard(Card c)
		{
            Cards.Add(c);
		}
	}
}

