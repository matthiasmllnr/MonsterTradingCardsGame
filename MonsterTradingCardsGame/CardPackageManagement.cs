using System;
using Newtonsoft.Json;

namespace MonsterTradingCardsGame
{
	public class CardPackageManagement
	{
		public Dictionary<int, CardPackage> AllCardPackages;
		public int Count
		{
			private set; get;
		}

		public CardPackageManagement()
		{
			AllCardPackages = new Dictionary<int, CardPackage>();
			Count = 0;
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
				Count++;

				return true;
            }
			return false;
		}
	}
}

