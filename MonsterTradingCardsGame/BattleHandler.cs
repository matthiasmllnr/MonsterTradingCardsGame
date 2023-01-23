using System;
namespace MonsterTradingCardsGame
{
	public class BattleHandler
	{
		private User? User1;
		private User? User2;
        private const int MAX_ROUNDS = 100;

		public BattleHandler()
		{

		}

		public string StartBattle()
		{
			string battleResult = "";

			if(User1 != null && User2 != null)
			{
                for (int i = 0; i < MAX_ROUNDS; i++)
                {
					// each player chooses a card
					Card card_1 = User1.GetRandomCardFromDeck();
					Card card_2 = User2.GetRandomCardFromDeck();

					// play round
					battleResult += $"----------[ ROUND {i + 1} ]----------\n\n";
					battleResult += PlayRound(card_1, card_2);
					battleResult += "\n";
					
					if(User2.deck.Count == 0)
					{
                        // User1 is the winner
                        User1.Elo += 3;
                        User2.Elo -= 5;
                        User1.Wins++;
                        User2.Losses++;
                        User1.Coins++;
                        battleResult += $"----------[ Battle Result ]----------\n\n";
                        battleResult += $"Winner: {User1.Username}\n";
                        battleResult += $"Loser: {User2.Username}\n";
                        battleResult += "\n--------------------------------------\n\n";
                        break;
                    }
                    else if (User1.deck.Count == 0)
                    {
                        // User2 is the winner
                        User2.Elo += 3;
                        User1.Elo -= 5;
                        User2.Wins++;
                        User1.Losses++;
                        User2.Coins++;
                        battleResult += $"----------[ Battle Result ]----------\n\n";
                        battleResult += $"Winner: {User2.Username}\n";
                        battleResult += $"Loser: {User1.Username}\n";
                        battleResult += "\n--------------------------------------\n\n";
                        break;
                    }
                }

                // reset user decks
                User1.LoadDeckFromDB();
                User2.LoadDeckFromDB();

                // update database
                User1.UpdateUser();
                User2.UpdateUser();

                // add battle log
                User1.LastBattleLog = battleResult;
                User2.LastBattleLog = battleResult;
            }

			return battleResult;
		}

		public void AddPlayer(User u)
		{
			if (User1 == null) User1 = u;
			else if (User2 == null) User2 = u;
		}

		public bool EnoughPlayers()
		{
			if(User1 != null && User2 != null)
			{
				return true;
			}
			return false;
		}

		private Card GetRandomCard(List<Card> deck)
		{
			Random random = new Random();
			int randIndex = random.Next(deck.Count);
			Card c = deck.ElementAt(randIndex);
			deck.RemoveAt(randIndex);
			return c;
		}

		private string PlayRound(Card c1, Card c2)
		{
			if(IsMonsterFight(c1, c2))
			{
				// Only MonsterCards
				return PlayMonsterFight(c1, c2);
			}
			else if(IsSpellFight(c1, c2))
			{
                // Only SpellCards
                return PlaySpellFight(c1, c2);
            }
			else if(IsMixedFight(c1, c2))
			{
                // 1 Monster Card and 1 SpellCard
                return PlayMixedFight(c1, c2);
            }
            return "Something went wrong";
		}

		private string PlayMonsterFight(Card c1, Card c2)
		{
            string battle = "";
			if(User1 != null && User2 != null)
			{
				// card vs card string creation
                battle = $"[{User1.Username}]{c1.Name}<{c1.Damage}> VS [{User2.Username}]{c2.Name}<{c2.Damage}>\n";

                // Goblin/Dragon interaction: Goblins are too afraid of Dragons to attack
                if (c1.Name.Equals(CardTypes.MonsterNames.WaterGoblin) && c2.Name.Equals(CardTypes.MonsterNames.Dragon))
                {
					// c2 wins
					battle += $"=> Goblins are too afraid of Dragons to attack => Dragon defeats Goblin\n";
					User2.deck.Add(c1); // get enemies card 
                    User2.deck.Add(c2); // get own card back
                    return battle;
                }

                if (c2.Name.Equals(CardTypes.MonsterNames.WaterGoblin) && c1.Name.Equals(CardTypes.MonsterNames.Dragon))
                {
                    // c1 wins
                    battle += $"=> Goblins are too afraid of Dragons to attack => Dragon defeats Goblin\n";
                    User1.deck.Add(c1); 
                    User1.deck.Add(c2); 
                    return battle;
                }

                // Wizzard/Ork interaction: Wizzards can control Orks so they are not able to damage them
                if (c1.Name.Equals(CardTypes.MonsterNames.Wizzard) && c2.Name.Equals(CardTypes.MonsterNames.Ork))
                {
                    // c1 wins
                    battle += $"=> Wizzards can control Orks so they are not able to damage them => Wizzard defeats Ork\n";
                    User1.deck.Add(c1);
                    User1.deck.Add(c2);
                    return battle;
                }

                if (c2.Name.Equals(CardTypes.MonsterNames.Wizzard) && c1.Name.Equals(CardTypes.MonsterNames.Ork))
                {
                    // c2 wins
                    battle += $"=> Wizzards can control Orks so they are not able to damage them => Wizzard defeats Ork\n";
                    User2.deck.Add(c1); 
                    User2.deck.Add(c2); 
                    return battle;
                }

                // FireElf/Dragon interaction: The FireElves know Dragons since they were little and can evade their attacks
                if (c1.Name.Equals(CardTypes.MonsterNames.FireElf) && c2.Name.Equals(CardTypes.MonsterNames.Dragon))
                {
                    // c1 wins
                    battle += $"=> The FireElves know Dragons since they were little and can evade their attacks => FireElf defeats Dragon\n";
                    User1.deck.Add(c1);
                    User1.deck.Add(c2);
                    return battle;
                }

                if (c2.Name.Equals(CardTypes.MonsterNames.FireElf) && c1.Name.Equals(CardTypes.MonsterNames.Dragon))
                {
                    // c2 wins
                    battle += $"=> The FireElves know Dragons since they were little and can evade their attacks => FireElf defeats Dragon\n";
                    User2.deck.Add(c1); 
                    User2.deck.Add(c2); 
                    return battle;
                }
            }
            

			battle += SimpleCardFight(c1, c2, false);
            return battle;
        }

        private string PlaySpellFight(Card c1, Card c2)
        {
            string battle = "";
            if(User1 != null && User2 != null)
            {
                battle = $"[{User1.Username}]{c1.Name}<{c1.Damage}> VS [{User2.Username}]{c2.Name}<{c2.Damage}>\n";
                battle += SimpleCardFight(c1, c2, true);
            }
            return battle;
        }

        private string PlayMixedFight(Card c1, Card c2)
        {
            string battle = "";
            if(User1 != null && User2 != null)
            {
                battle = $"[{User1.Username}]{c1.Name}<{c1.Damage}> VS [{User2.Username}]{c2.Name}<{c2.Damage}>\n";

                // Kraken/SpellCard interaction: The Kraken is immune against spells
                if(c1.Name.Equals(CardTypes.MonsterNames.Kraken) && c2.Type.Equals(CardTypes.SpellCard))
                {
                    // c1 wins
                    battle += $"=> The Kraken is immune against spells => Kraken defeats Spell\n";
                    User1.deck.Add(c1);
                    User1.deck.Add(c2);
                    return battle;
                }

                if (c2.Name.Equals(CardTypes.MonsterNames.Kraken) && c1.Type.Equals(CardTypes.SpellCard))
                {
                    // c2 wins
                    battle += $"=> The Kraken is immune against spells => Kraken defeats Spell\n";
                    User2.deck.Add(c1);
                    User2.deck.Add(c2);
                    return battle;
                }

                // Knight/SpellCard interaction: The armor of Knights is so heavy that WaterSpells make them drown them instantly
                if (c1.Name.Equals(CardTypes.MonsterNames.Knight) && c2.Type.Equals(CardTypes.SpellCard) && c2.Element.Equals(CardTypes.ElementTypes.Water))
                {
                    // c2 wins
                    battle += $"=> The armor of Knights is so heavy that WaterSpells make them drown instantly => WaterSpell defeats Knight\n";
                    User2.deck.Add(c1);
                    User2.deck.Add(c2);
                    return battle;
                }

                if (c2.Name.Equals(CardTypes.MonsterNames.Knight) && c1.Type.Equals(CardTypes.SpellCard) && c1.Element.Equals(CardTypes.ElementTypes.Water))
                {
                    // c1 wins
                    battle += $"=> The armor of Knights is so heavy that WaterSpells make them drown instantly => WaterSpell defeats Knight\n";
                    User1.deck.Add(c1);
                    User1.deck.Add(c2);
                    return battle;
                }

                battle += SimpleCardFight(c1, c2, true);
            }
            return battle;
        }

		private string SimpleCardFight(Card c1, Card c2, bool considerElement)
		{
            string battle = "";
            if (User1 != null && User2 != null)
            {
                float dmg1 = c1.Damage;
                float dmg2 = c2.Damage;

                if (considerElement)
                {
                    if(c1.Element.Equals(CardTypes.ElementTypes.Fire) && c2.Element.Equals(CardTypes.ElementTypes.Water))
                    {
                        dmg1 *= 2;
                        dmg2 /= 2;
                    }
                    else if(c1.Element.Equals(CardTypes.ElementTypes.Water) && c2.Element.Equals(CardTypes.ElementTypes.Fire))
                    {
                        dmg1 /= 2;
                        dmg2 *= 2;
                    }
                }

                if (dmg1 > dmg2)
                {
                    // c1 wins
                    battle += $"=> {dmg1} VS {dmg2} => {c1.Name} defeats {c2.Name}\n";
                    User1.deck.Add(c1);
                    User1.deck.Add(c2);
                }
                else if (dmg2 > dmg1)
                {
                    // c2 wins
                    battle += $"=> {dmg1} VS {dmg2} => {c2.Name} defeats {c1.Name}\n";
                    User2.deck.Add(c1);
                    User2.deck.Add(c2);
                }
                else if (dmg1 == dmg2)
                {
                    // draw
                    battle += $"=> {dmg1} VS {dmg2} => Draw\n";
                    User1.deck.Add(c1); // get their own card back
                    User2.deck.Add(c2); // get their own card back
                }
            }

            return battle;
		}

        private bool IsMonsterFight(Card c1, Card c2)
		{
			if(c1.Type.Equals(CardTypes.MonsterCard) && c2.Type.Equals(CardTypes.MonsterCard))
			{
				return true;
			}
			return false;
		}

        private bool IsSpellFight(Card c1, Card c2)
        {
            if (c1.Type.Equals(CardTypes.SpellCard) && c2.Type.Equals(CardTypes.SpellCard))
            {
                return true;
            }
            return false;
        }

        private bool IsMixedFight(Card c1, Card c2)
        {
            if ((c1.Type.Equals(CardTypes.MonsterCard) && c2.Type.Equals(CardTypes.SpellCard)) || (c1.Type.Equals(CardTypes.SpellCard) && c2.Type.Equals(CardTypes.MonsterCard)))
            {
                return true;
            }
            return false;
        }

        private void ResetBattleHandler()
		{
			User1 = null;
			User2 = null;
		}

	}
}

