using System;
using System.Data;
using Npgsql;
using System.Collections;

namespace MonsterTradingCardsGame
{
	public class Database
	{
        private NpgsqlConnection conn;

        public Database(string cs)
		{
            conn = new NpgsqlConnection(cs);
            conn.Open();
        }

        public void CreateUser(Hashtable data)
        {
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO users (name, password, token) VALUES (:n, :pw, :t)";

            IDataParameter pName = cmd.CreateParameter();
            pName.ParameterName = ":n";
            pName.Value = data["Username"];

            IDataParameter pPassword = cmd.CreateParameter();
            pPassword.ParameterName = ":pw";
            pPassword.Value = data["Password"];

            IDataParameter pToken = cmd.CreateParameter();
            pToken.ParameterName = ":t";
            pToken.Value = "-";

            cmd.Parameters.Add(pName);
            cmd.Parameters.Add(pPassword);
            cmd.Parameters.Add(pToken);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        public string LoginUser(Hashtable data)
        {
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM users WHERE name = '{data["Username"]}' AND password = '{data["Password"]}'";

            IDataReader dr = cmd.ExecuteReader();
            bool valid = dr.Read();
            dr.Close();
            cmd.Dispose();

            if (valid)
            {
                // user and password exist and are correct
                string token = generateSecurityToken();

                cmd = conn.CreateCommand();
                cmd.CommandText = $"UPDATE users SET token = '{token}' WHERE name = '{data["Username"]}' AND password = '{data["Password"]}'";
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                return token;

            } else
            {
                // username or password are incorrect
                return "-";
            }
        }

        private string generateSecurityToken()
        {
            string token = "";
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int alphabetLength = alphabet.Length;
            int tokenLength = 50;

            Random random = new Random();
            int idx;
            for(int i = 0; i < tokenLength; i++)
            {
                idx = random.Next(0, alphabetLength - 1);
                token += alphabet[idx];
            }

            return token;
        }
    }
}

