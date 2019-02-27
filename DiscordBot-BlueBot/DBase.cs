using System;
using System.Collections.Generic;
using System.Data;
using SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.BlueBot.Core;

namespace DiscordBot_BlueBot
{
    public class DBase
    {
        private SQLiteConnection db;


        public DBase()
        {
            //var dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "UserDB.db");
            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Core\UserAccounts"))
                Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Core\UserAccounts");

            var dbPath = $@"{AppDomain.CurrentDomain.BaseDirectory}Core\UserAccounts\UserDB.db";
            db = new SQLiteConnection(dbPath);
        }

        public void CreateUserTable()
        {
            db.CreateTable<UserAccount>();
        }

        public void AddUser(UserAccount user)
        {
            db.Insert(user);
            Console.WriteLine($"[DB] {DateTime.Now.ToLocalTime()} | Added \"{user.DiscordId} - {user.Username}\" to the database.");
        }

        public void UpdateUser(UserAccount user)
        {
            db.Update(user);
        }

        public List<UserAccount> GetAllUsers()
        {
            var table = db.Table<UserAccount>();

            return table.ToList();
        }

        public void RemoveNote(UserAccount note)
        {
            db.Delete<UserAccount>(note.Id);
        }

        public void CreateTableWithData()
        {
            db.CreateTable<UserAccount>();
            if (!db.Table<UserAccount>().Any())
            {
                var newUser = new UserAccount();
                newUser.DiscordId = 189139492488085504;
                newUser.Username = "TestName";
                newUser.JoinDate = DateTime.Now.ToLocalTime();
                db.Insert(newUser);
            }
        }

    }
}
