﻿using System;
using System.Collections;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace MonsterTradingCardsGame
{
	public class UserManagement
	{
		private List<User> users;

		public UserManagement()
		{
			users = new List<User>();
		}

		public void AddUser(string username, string password, string token, int coins, string bio, string image, int elo, int wins, int losses)
		{
			User user = new User(username, password, token, coins, bio, image, elo, wins, losses);
			users.Add(user);
		}

		public void CreateUser(string data)
		{
			User? tmpUser = JsonConvert.DeserializeObject<User>(data);
			if(tmpUser != null)
			{
				Database db = new Database();

                NpgsqlCommand cmd = db.conn.CreateCommand();
                cmd.CommandText = $"INSERT INTO users (name, password, token) VALUES ('{tmpUser.Username}', '{tmpUser.Password}', '-')";
				cmd.ExecuteNonQuery();
                cmd.Dispose();

                db.CloseConnection();
            }
        }

        public string LoginUser(string data)
        {
            string token = "-";
            User? tmpUser = JsonConvert.DeserializeObject<User>(data);
            if (tmpUser != null)
            {
                Database db = new Database();
                NpgsqlCommand cmd = db.conn.CreateCommand();
                cmd.CommandText = $"SELECT * FROM users WHERE name = '{tmpUser.Username}' AND password = '{tmpUser.Password}'";

                IDataReader dr = cmd.ExecuteReader();
                bool valid = dr.Read();
                if (valid)
                {
                    // user and password exist and are correct
                    token = GenerateSecurityToken();
                    AddUser(tmpUser.Username, tmpUser.Password, token,
                        (int)dr[4],     // coins
                        (string)dr[5],  // bio
                        (string)dr[6],  // image
                        (int)dr[7],     // elo
                        (int)dr[8],     // wins
                        (int)dr[9]      // losses
                    );
                    dr.Close();
                    cmd.CommandText = $"UPDATE users SET token = '{token}' WHERE name = '{tmpUser.Username}' AND password = '{tmpUser.Password}'";
                    cmd.ExecuteNonQuery();
                }
                cmd.Dispose();
                db.CloseConnection();
            }
            return token;
        }

        private string GenerateSecurityToken()
        {
            string token = "";
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int alphabetLength = alphabet.Length;
            int tokenLength = 50;

            Random random = new Random();
            int idx;
            for (int i = 0; i < tokenLength; i++)
            {
                idx = random.Next(0, alphabetLength - 1);
                token += alphabet[idx];
            }

            return token;
        }

        public void PrintUsers()
        {
            foreach (User u in users)
            {
                u.PrintUser();
            }
        }

        public bool IsAuthorized(string token, bool adminRequired)
        {
            bool authorized = false;
            foreach (User user in users)
            {
                if (user.AuthenticationToken.Equals(token))
                {
                    authorized = true;
                    if (adminRequired && !user.Username.Equals("admin"))
                    {
                        authorized = false;
                    }
                    break;
                }
            }
            return authorized;
        }

        public User? GetUser(string token)
        {
            foreach(User u in users)
            {
                if (u.AuthenticationToken.Equals(token))
                {
                    return u;
                }
            }
            return null;
        }

    }
}

