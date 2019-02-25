using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot_BlueBot
{
    public class DBase
    {
        private SQLiteConnection _sqliteCon;
        private SQLiteCommand _sqliteCmd;

        public DBase()
        {
            _sqliteCon = new SQLiteConnection("Data Source=/Core/UserAccounts/UserDB.db;New=False;");
        }

        public void LoadData()
        {

        }

        public void ExecuteQuery(string txtQuery)
        {
            _sqliteCon.Open();
            _sqliteCmd = _sqliteCon.CreateCommand();
            _sqliteCmd.CommandText = txtQuery;
            _sqliteCmd.ExecuteNonQuery();
            _sqliteCon.Close();
        }

        //public DataTable SelectQuery(string query)
        //{
        //    SQLiteDataAdapter ad;
        //    DataTable dt = new DataTable();

        //    try
        //    {
        //        SQLiteCommand cmd;
        //        sqlite.Open(); //Initiate connection to the db

        //        cmd = sqlite.CreateCommand();
        //        cmd.CommandText = query; //set the passed query
        //        ad = new SQLiteDataAdapter(cmd);
        //        ad.Fill(dt); //fill the datasource
        //    }
        //    catch (SQLiteException ex)
        //    {
        //        //Add your exception code here.
        //    }

        //    sqlite.Close();
        //    return dt;
        //}
    }
}
