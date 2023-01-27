namespace Tests_MTCG;

using System.Security.Cryptography;
using System.Xml.Linq;
using MonsterTradingCardsGame;
using Npgsql;

[TestClass]
public class MTCG_Tests
{
    [TestMethod]
    public void DBConnectionOpensAndCloses()
    {
        Database db = new Database();
        if (db.conn.State.ToString() == "Closed") Assert.Fail("Connection should be open.");
       
        db.CloseConnection();
        if (db.conn.State.ToString() == "Open") Assert.Fail("Connection should be closed.");
    }

    [TestMethod]
    public void BattleEnoughPlayersTrue()
    {
        BattleHandler bh = new BattleHandler();
        bh.AddPlayer(new User("testUser1", "pw", "token", 20, "bio", "image", 200, 10, 5));
        bh.AddPlayer(new User("testUser2", "pw", "token", 20, "bio", "image", 200, 10, 5));
        Assert.IsTrue(bh.EnoughPlayers());
    }

    [TestMethod]
    public void BattleEnoughPlayersFalse()
    {
        BattleHandler bh = new BattleHandler();
        Assert.IsFalse(bh.EnoughPlayers());
    }

    [TestMethod]
    public void CreateCardPackage()
    {
        CardPackageManagement cpm = new CardPackageManagement();
        string data = "[{\"Type\":\"Monster\", \"Name\":\"WaterGoblin\", \"Element\":\"Water\", \"Damage\": 10.0}, {\"Type\":\"Monster\", \"Name\":\"Dragon\", \"Element\":\"Fire\", \"Damage\": 50.0}, {\"Type\":\"Spell\", \"Name\":\"WaterSpell\", \"Element\":\"Water\", \"Damage\": 20.0}, {\"Type\":\"Monster\", \"Name\":\"Ork\", \"Element\":\"Normal\", \"Damage\": 45.0}, {\"Type\":\"Spell\", \"Name\":\"FireSpell\", \"Element\":\"Fire\", \"Damage\": 25.0}]";
        Assert.IsTrue(cpm.CreateCardPackage(data));
        Assert.IsFalse(cpm.CreateCardPackage(""));
    }

    [TestMethod]
    public void AcquirePackageNotEnoughCoins()
    {
        CardPackageManagement cpm = new CardPackageManagement();
        User testUser = new User("testUser", "pw", "token", 3, "bio", "image", 200, 10, 5);
        if (cpm.AcquirePackage(testUser) != "Not enough coins.") Assert.Fail("Should be able to afford package.");
    }

    [TestMethod]
    public void UserM_AddUser()
    {
        UserManagement uM = new UserManagement();
        int countBefore = uM.users.Count;
        uM.AddUser("testUser", "pw", "token", 3, "bio", "image", 200, 10, 5);
        int countAfter = uM.users.Count;
        if (countAfter <= countBefore) Assert.Fail("Didnt add user.");
    }

    [TestMethod]
    public void IsAuthorized()
    {
        UserManagement uM = new UserManagement();
        User testUser = new User("testUser", "pw", "token", 3, "bio", "image", 200, 10, 5);
        // not logged in
        Assert.IsFalse(uM.IsAuthorized(testUser.AuthenticationToken, false));
        // logged in
        uM.AddUser("testUser", "pw", "token", 3, "bio", "image", 200, 10, 5);
        Assert.IsTrue(uM.IsAuthorized(testUser.AuthenticationToken, false));
        // admin required should fail
        Assert.IsFalse(uM.IsAuthorized(testUser.AuthenticationToken, true));
        // admin require should succeed
        uM.AddUser("admin", "pw", "tokenAdmin", 3, "bio", "image", 200, 10, 5);
        Assert.IsTrue(uM.IsAuthorized("tokenAdmin", true));
    }

    [TestMethod]
    public void GetUserByToken()
    {
        UserManagement uM = new UserManagement();
        uM.AddUser("testUser", "pw", "token", 3, "bio", "image", 200, 10, 5);
        Assert.IsNull(uM.GetUser("NoToken"));
        Assert.IsNotNull(uM.GetUser("token"));
    }

    [TestMethod]
    public void GetRandomCardFromDeck()
    {
        User testUser = new User("testUser", "pw", "token", 3, "bio", "image", 200, 10, 5);
        testUser.deck.Add(new Card("testCard", "monster", "water", 20));
        testUser.deck.Add(new Card(2, "testCard2", "monster", "water", 20));
        testUser.deck.Add(new Card(3, "testCard3", "monster", "water", 20));
        Assert.IsNotNull(testUser.GetRandomCardFromDeck());
    }

    [TestMethod]
    public void IsCardInDeck()
    {
        User testUser = new User("testUser", "pw", "token", 3, "bio", "image", 200, 10, 5);
        testUser.deck.Add(new Card(1, "testCard", "monster", "water", 20));
        testUser.deck.Add(new Card(2, "testCard2", "monster", "water", 20));
        testUser.deck.Add(new Card(3, "testCard3", "monster", "water", 20));
        Assert.IsTrue(testUser.IsCardInDeck(1));
        Assert.IsFalse(testUser.IsCardInDeck(200));
    }

    [TestMethod]
    public void IsCardInStack()
    {
        User testUser = new User("testUser", "pw", "token", 3, "bio", "image", 200, 10, 5);
        testUser.stack.Add(new Card(1, "testCard", "monster", "water", 20));
        testUser.stack.Add(new Card(2, "testCard2", "monster", "water", 20));
        testUser.stack.Add(new Card(3, "testCard3", "monster", "water", 20));
        Assert.IsTrue(testUser.IsCardInStack(1));
        Assert.IsFalse(testUser.IsCardInStack(200));
    }

    [TestMethod]
    public void RemoveCardFromStack()
    {
        User testUser = new User("testUser", "pw", "token", 3, "bio", "image", 200, 10, 5);
        testUser.stack.Add(new Card(1, "testCard", "monster", "water", 20));
        testUser.stack.Add(new Card(2, "testCard2", "monster", "water", 20));
        testUser.stack.Add(new Card(3, "testCard3", "monster", "water", 20));
        // with existing card id
        int before = testUser.stack.Count();
        testUser.RemoveCardFromStack(1);
        int after = testUser.stack.Count();
        if (after >= before) Assert.Fail("Remove didnt work.");
        // with not existing card id
        before = testUser.stack.Count();
        testUser.RemoveCardFromStack(200);
        after = testUser.stack.Count();
        if (after != before) Assert.Fail("Removed wrong card.");
    }

    [TestMethod]
    public void CardTest()
    {
        int cId = 1000;
        string cName = "TestCard";
        string cType = "Monster";
        string cElement = "Fire";
        float cDmg = 20;
        Card testCard = new Card(cId, cName, cType, cElement, cDmg);
        if (testCard.Id != cId) Assert.Fail("Card Id is not the same.");
        if (testCard.Name != cName) Assert.Fail("Card name is not the same.");
        if (testCard.Type != cType) Assert.Fail("Card type is not the same.");
        if (testCard.Element != cElement) Assert.Fail("Card element is not the same.");
        if (testCard.Damage != cDmg) Assert.Fail("Card damage is not the same.");
    }

    [TestMethod]
    public void CardPackageTest()
    {
        string cName = "TestCard";
        string cType = "Monster";
        string cElement = "Fire";
        float cDmg = 20;
        Card testCard1 = new Card(1000, cName, cType, cElement, cDmg);
        Card testCard2 = new Card(1001, cName, cType, cElement, cDmg);
        Card testCard3 = new Card(1002, cName, cType, cElement, cDmg);
        Card testCard4 = new Card(1003, cName, cType, cElement, cDmg);
        Card testCard5 = new Card(1004, cName, cType, cElement, cDmg);
        List<Card> l = new List<Card>();
        l.Add(testCard1);
        l.Add(testCard2);
        l.Add(testCard3);
        l.Add(testCard4);
        l.Add(testCard5);
        CardPackage cp = new CardPackage(100, l);
        if (cp == null) Assert.Fail("CardPackage is null.");
        if (cp.Cards.Count != 5) Assert.Fail("Not all cards got added to the package.");
    }

    [TestMethod]
    public void CardPackage_SavedInDB()
    {
        string cName = "TestCard";
        string cType = "Monster";
        string cElement = "Fire";
        float cDmg = 20;
        Card testCard1 = new Card(cName, cType, cElement, cDmg);
        Card testCard2 = new Card(cName, cType, cElement, cDmg);
        Card testCard3 = new Card(cName, cType, cElement, cDmg);
        Card testCard4 = new Card(cName, cType, cElement, cDmg);
        Card testCard5 = new Card(cName, cType, cElement, cDmg);
        List<Card> l = new List<Card>();
        l.Add(testCard1);
        l.Add(testCard2);
        l.Add(testCard3);
        l.Add(testCard4);
        l.Add(testCard5);
        CardPackage cp = new CardPackage(l);

        Database db = new Database();
        NpgsqlCommand cmd = db.conn.CreateCommand();
        cmd.CommandText = $"SELECT * FROM cards WHERE name = '{cName}'";
        NpgsqlDataReader dr = cmd.ExecuteReader();
        if (!dr.Read())
        {
            Assert.Fail("Cards not found in DB.");
        }
        dr.Close();

        cmd.CommandText = $"SELECT * FROM packages p " +
            $"JOIN cards c ON c.id = p.card1_id " +
            $"WHERE c.name = '{cName}'";
        dr = cmd.ExecuteReader();
        if (!dr.Read())
        {
            Assert.Fail("Package not found in DB.");
        }
        int pId = (int)dr[0];
        dr.Close();
        cmd.CommandText = $"DELETE FROM packages WHERE id = '{pId}'";
        cmd.ExecuteNonQuery();
        cmd.CommandText = $"DELETE FROM cards WHERE name = '{cName}'";
        cmd.ExecuteNonQuery();
        cmd.Dispose();
        db.CloseConnection();
    }

    [TestMethod]
    public void Card_SavedInDB()
    {
        string cName = "TestCard";
        string cType = "Monster";
        string cElement = "Fire";
        float cDmg = 20;
        Card testCard = new Card(cName, cType, cElement, cDmg); // should save in DB

        Database db = new Database();
        NpgsqlCommand cmd = db.conn.CreateCommand();
        cmd.CommandText = $"SELECT * FROM cards WHERE name = '{cName}'";
        NpgsqlDataReader dr = cmd.ExecuteReader();
        if (!dr.Read())
        {
            Assert.Fail("Card not found in DB.");
        }
        dr.Close();

        cmd.CommandText = $"DELETE FROM cards WHERE name = '{cName}'";
        cmd.ExecuteNonQuery();
        cmd.Dispose();
        db.CloseConnection();
    }

    [TestMethod]
    public void GetUserIdFromDB()
    {
        string name = "TEST_USER";
        string pw = "TEST_PW";
        string token = "TEST_TOKEN";
        User testUser = new User(name, pw, token, 20, "bio", "image", 100, 0, 0);
        Database db = new Database();
        NpgsqlCommand cmd = db.conn.CreateCommand();
        cmd.CommandText = $"INSERT INTO users (name, password, token) VALUES ('{name}', '{pw}', '{token}')";
        cmd.ExecuteNonQuery();
        Assert.IsNotNull(testUser.Id);
        cmd.CommandText = $"DELETE FROM users WHERE name = '{name}'";
        cmd.ExecuteNonQuery();
        cmd.Dispose();
        db.CloseConnection();
    }

    [TestMethod]
    public void UpdateUser()
    {
        string name = "TEST_USER";
        string pw = "TEST_PW";
        string token = "TEST_TOKEN";
        Database db = new Database();
        NpgsqlCommand cmd = db.conn.CreateCommand();
        cmd.CommandText = $"INSERT INTO users (name, password, token) VALUES ('{name}', '{pw}', '{token}')";
        cmd.ExecuteNonQuery();
        cmd.Dispose();
        db.CloseConnection();
        User testUser = new User(name, pw, token, 20, "NEW_BIO", "NEW_IMAGE", 200, 10, 10);
        testUser.UpdateUser();
        if (testUser.Bio != "NEW_BIO") Assert.Fail("Bio did not update");
        if (testUser.Image != "NEW_IMAGE") Assert.Fail("Image did not update");
        if (testUser.Elo != 200) Assert.Fail("Elo did not update");
        if (testUser.Wins != 10) Assert.Fail("Wins did not update");
        if (testUser.Losses != 10) Assert.Fail("Losses did not update");
        db = new Database();
        cmd = db.conn.CreateCommand();
        cmd.CommandText = $"DELETE FROM users WHERE name = '{name}'";
        cmd.ExecuteNonQuery();
        cmd.Dispose();
        db.CloseConnection();
    }

    [TestMethod]
    public void GetCardFromStackById()
    {
        int id = 10000;
        string name = "TEST_USER";
        string pw = "TEST_PW";
        string token = "TEST_TOKEN";
        User testUser = new User(name, pw, token, 20, "TEST_BIO", "TEST_IMAGE", 200, 10, 10);
        Card c = new Card(id, "TEST_CARD", "Monster", "Water", 20000);
        testUser.stack.Add(c);
        Card? getCard = testUser.GetCardFromStackByID(id);
        if (getCard == null) Assert.Fail("GetCardFromStackById failed.");
    }

    [TestMethod]
    public void CheckCardsOwned()
    {
        List<int> ids = new List<int>();
        ids.Add(1000);
        ids.Add(2000);
        ids.Add(3000);
        string name = "TEST_USER";
        string pw = "TEST_PW";
        string token = "TEST_TOKEN";
        User testUser = new User(name, pw, token, 20, "TEST_BIO", "TEST_IMAGE", 200, 10, 10);
        Card c1 = new Card(ids[0], "TEST_CARD", "Monster", "Water", 20000);
        Card c2 = new Card(ids[1], "TEST_CARD", "Monster", "Water", 20000);
        Card c3 = new Card(ids[2], "TEST_CARD", "Monster", "Water", 20000);
        testUser.stack.Add(c1);
        testUser.stack.Add(c2);
        testUser.stack.Add(c3);
        Assert.IsTrue(testUser.CheckCardsOwned(ids));
        ids.Add(4000); // doenst own this one
        Assert.IsFalse(testUser.CheckCardsOwned(ids));
    }
}
