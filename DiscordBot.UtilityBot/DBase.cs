using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.BlueBot;
using DiscordBot.BlueBot.Core;

namespace DiscordBot_BlueBot
{
    /// <summary>
    /// MUST be initialized to work with any data.
    /// </summary>
    public class DBase
    {
        private SQLiteConnection db;
        public static string dbPath;

        public DBase(SocketGuild guild)
        {
            dbPath = $@"{AppDomain.CurrentDomain.BaseDirectory}Core\UserAccounts\{guild.Name}\UserDB-{guild.Id}.db";
            //var dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "UserDB.db");
            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Core\UserAccounts"))
                Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Core\UserAccounts");
            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Core\UserAccounts\{guild.Name}"))
                Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Core\UserAccounts\{guild.Name}");

            db = new SQLiteConnection(dbPath);
        }

        public void CreateUserTable()
        {
            db.CreateTable<UserAccount>();
            
        }

        public void AddUser(UserAccount user)
        {
            db.Insert(user);
            var guildIdLength = db.DatabasePath.Split('\\').Length;
            var guildIdLength2 = db.DatabasePath.Split('\\').Length - 1;
            Utilities.LogConsole(Utilities.LogType.DATABASE, 
                $"{DateTime.Now.ToLocalTime():dd/MM/yy hh:mm:ss} > Added user {user.DiscordId} - \"{user.Username}\" to {db.DatabasePath.Split('\\')[guildIdLength]} in {db.DatabasePath.Split('\\')[guildIdLength2]}.");
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
            // If succeeded
            if (result == 1)
            {
                Utilities.LogConsole(Utilities.LogType.DATABASE, 
                    $"Edit Successful > User {GetUserByDiscordId(discordId)} - {discordId}, property {dbProperty}, new value {value}");
            }
            else { Utilities.LogConsole(Utilities.LogType.DATABASE_ERROR, 
                $"Couldn't change property > User {GetUserByDiscordId(discordId)} - {discordId}, property {dbProperty}, new value {value}"); }
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

        public void RemoveUserByDiscordId(ulong discordId)
        {
            db.Delete<UserAccount>(GetUserByDiscordId(discordId).DiscordId);
        }

        public UserAccount GetUserByDiscordId(ulong discordId)
        {
            var dId = Convert.ToInt64(discordId);
            var table = db.Table<UserAccount>().ToList(); // IMPORTANT to convert the enumerable to list first filter(lambda) data, was a real pain
            return table.First(x => x.DiscordId == dId);
        }

        public List<ulong> GetUserIds()
        {
            List<ulong> ids = null;
            foreach (var user in db.Table<UserAccount>())
            {
                ids.Add(Convert.ToUInt64(user.DiscordId));
            }

            return ids;
        }

        public void CreateNewUser(ulong discId, string username, DateTimeOffset joinDate, int isMember)
        {
            if (!db.Table<UserAccount>().Any())
            {
                var newUser = new UserAccount();
                newUser.DiscordId = Convert.ToInt64(discId);
                newUser.Username = username;
                newUser.JoinDate = joinDate;
                newUser.IsMember = isMember;
                db.Insert(newUser);
            }
        }

    }
}
