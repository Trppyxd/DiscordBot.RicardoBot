using System;
using System.Collections.Generic;
using System.Data;
using SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.BlueBot;
using DiscordBot.BlueBot.Core;

namespace DiscordBot_BlueBot
{
    public class DBase
    {
        private SQLiteConnection db;
        public static string dbPath = $@"{AppDomain.CurrentDomain.BaseDirectory}Core\UserAccounts\UserDB.db";

        public DBase()
        {
            //var dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "UserDB.db");
            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Core\UserAccounts"))
                Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Core\UserAccounts");

            db = new SQLiteConnection(dbPath);
        }

        public void CreateUserTable()
        {
            db.CreateTable<UserAccount>();
            
        }

        public void AddUser(UserAccount user)
        {
            db.Insert(user);
            Utilities.LogConsole(Utilities.LogType.DATABASE, $"{DateTime.Now.ToLocalTime():dd/MM/yy hh:mm:ss} > Added user {user.DiscordId} - \"{user.Username}\" to the database.");
        }

        public void UpdateUser(UserAccount user)
        {
            db.Update(user);
        }

        public void EditUser(ulong discordId, string dbProperty, string value)
        {
            SQLiteCommand cmd = new SQLiteCommand(db);
            cmd.CommandText = $@"Update UserAccount Set {dbProperty} = {value} Where DiscordId = {discordId}";

            int result = cmd.ExecuteNonQuery();
            if (result == 1)
            {
                Utilities.LogConsole(Utilities.LogType.DATABASE, $"Edit Successful > User {discordId}, property {dbProperty}, new value {value}");
            }
            else { Utilities.LogConsole(Utilities.LogType.ERROR, $"Couldn't change property > User {discordId}, property {dbProperty}, new value {value}"); }
        }
        
        

        public List<UserAccount> GetAllUsers()
        {

            var table = db.Table<UserAccount>();

            return table.ToList();
        }

        public void RemoveUser(UserAccount user)
        {
            db.Delete<UserAccount>(user.Id);
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
                newUser.IsMember = 1;
                db.Insert(newUser);
            }
        }

    }
}
