using System;
using Newtonsoft.Json;

namespace MonsterTradingCardsGame
{
	public class CardPackageManagement
	{
		public List<CardPackage> AllCardPackages;
		public int Count
		{
			private set; get;
		}

		public CardPackageManagement()
		{
			AllCardPackages = new List<CardPackage>();
			Count = 0;
		}

		public bool CreateCardPackage(string data)
		{
			List<Card>? package = JsonConvert.DeserializeObject<List<Card>>(data);
			if(package != null)
			{
				Console.WriteLine("------------Package created------------");
				Console.WriteLine(package[0].GetProperties());
				Console.WriteLine(package[1].GetProperties());
				Console.WriteLine(package[2].GetProperties());
				Console.WriteLine(package[3].GetProperties());
				Console.WriteLine(package[4].GetProperties());
                Console.WriteLine("---------------------------------------");
				
				CardPackage cPackage = new CardPackage();
				for(int i = 0; i < 5; i++)
				{
					cPackage.AddCard(package[i]);
				}

				return true;
            }
			return false;
		}
	}
}

