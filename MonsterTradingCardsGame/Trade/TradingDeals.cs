using System;
using Newtonsoft.Json;
using Npgsql;

namespace MonsterTradingCardsGame
{
	public class TradingDeals
	{
		List<Offer> Offers;

		public TradingDeals()
		{
			Offers = new List<Offer>();
			LoadOffersFromDB();
		}

		public string AddOffer(string data, User u)
		{
			Offer? tempOffer = JsonConvert.DeserializeObject<Offer>(data);
			if(tempOffer != null)
			{
				if (u.GetCardFromStackByID(tempOffer.CardId) == null) return "You dont own this card.";
				if (u.IsCardInDeck(tempOffer.CardId)) return "Can not trade card. Remove it from the deck first.";
				Offer o = new Offer(u.Id, tempOffer.CardId, tempOffer.CardName, tempOffer.CardType, tempOffer.MinDamage);
				Offers.Add(o);
				return "Offer created.";
			}
			return "Something went wrong! No offer created.";
		}

		public string RemoveOffer(int offerId, int uId)
		{
            foreach (Offer o in Offers)
			{
				if (o.Id.Equals(offerId) && o.UserId == uId)
				{
					o.DeleteFromDB();
					Offers.Remove(o);
					return "Removed Offer.";
				}
			}
			return "Removing offer failed.";
        }

		private void LoadOffersFromDB()
		{
			Database db = new Database();
			NpgsqlCommand cmd = db.conn.CreateCommand();
			cmd.CommandText = "SELECT * FROM trades";
			NpgsqlDataReader dr = cmd.ExecuteReader();

			while (dr.Read())
			{
				Offer o = new Offer((int)dr[0], (int)dr[1], (int)dr[2], (string)dr[5], (string)dr[3], (float)dr[4]);
				Offers.Add(o);
			}

			dr.Close();
			cmd.Dispose();
			db.CloseConnection();
		}

		public string GetTradingDeals()
		{
			string tradingDeals = "[OfferID] CardName<Element>(Damage) <==> CardName<Element>(MinDamage)\n\n";

			foreach(Offer o in Offers)
			{
				List<string> cProp = o.GetCardProperties(o.CardId, o.UserId);
				tradingDeals += $"[{o.Id}] {cProp[0]}<{cProp[1]}>({cProp[2]}) <==> ";
				tradingDeals += $"{o.CardName}<{o.CardType}>({o.MinDamage})";
				tradingDeals += "\n";
			}

			return tradingDeals;
		}

		public string Trade(int offerId, User u, int data, UserManagement uM)
		{
			if (!OfferExists(offerId)) return "Offer does not exist.";
			if (IsOwner(u.Id)) return "You can't trade with yourself.";
			if (!u.IsCardInStack(data)) return "You are not the owner of the card.";
			if (u.IsCardInDeck(data)) return "Remove your card from the deck before trading.";

			Card? c = u.GetCardFromStackByID(data);
			Offer? o = GetOfferById(offerId);
			if(c != null && o != null)
			{
				if(o.CardName != c.Name || o.CardType != c.Element || o.MinDamage > c.Damage)
				{
					return "Card does not fullfill requirements.";
				}

				
				User? offerUser = uM.GetUser(o.UserId);
				if(offerUser != null)
				{
                    // add to stack
                    offerUser.stack.Add(c);
                    Card? c2 = offerUser.GetCardFromStackByID(o.CardId);
                    if (c2 != null) u.stack.Add(c2);

                    // remove from both stacks
                    offerUser.RemoveCardFromStack(o.CardId);
					u.RemoveCardFromStack(c.Id);

					Offers.Remove(o);

                    // swap cards and delete offer from db
                    Database db = new Database();
					NpgsqlCommand cmd = db.conn.CreateCommand();
					cmd.CommandText = $"UPDATE user_cards SET user_id = '{offerUser.Id}' WHERE card_id = '{c.Id}'";
					cmd.ExecuteNonQuery();
					cmd.CommandText = $"UPDATE user_cards SET user_id = '{u.Id}' WHERE card_id = '{o.CardId}'";
					cmd.ExecuteNonQuery();
					cmd.CommandText = $"DELETE FROM trades WHERE id = '{offerId}'";
					cmd.ExecuteNonQuery();
					cmd.Dispose();
                    db.CloseConnection();
                }

            }

			return "Trade successfull.";
		}

		private bool OfferExists(int offerId)
		{
			foreach(Offer o in Offers)
			{
				if (o.Id == offerId) return true;
			}
			return false;
		}

		private bool IsOwner(int userId)
		{
			foreach (Offer o in Offers)
			{
				if (o.UserId == userId) return true;
			}
			return false;
		}

		private Offer? GetOfferById(int offerId)
		{
			foreach(Offer o in Offers)
			{
				if (o.Id == offerId) return o;
			}
			return null;
		}

	}
}

