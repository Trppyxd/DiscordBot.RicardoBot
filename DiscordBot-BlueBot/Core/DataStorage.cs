using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DiscordBot.BlueBot.Core
{
    class DataStorage
    {
        // TODO Connect app to a database and add all messages and users to it.
        SQLiteConnection db =  new SQLiteConnection("Data Source=.\\UserAccounts/database.sqlite;Version=3;");
        

        static DataStorage()
        {

        }
    }
}
