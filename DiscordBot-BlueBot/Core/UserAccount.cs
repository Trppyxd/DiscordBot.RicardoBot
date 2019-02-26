using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace DiscordBot.BlueBot.Core
{
    public class UserAccount
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [MaxLength(255), Unique]
        public long DiscordId { get; set; }
        [MaxLength(255)]
        public string Username { get; set; }
        [MaxLength(255)]
        public DateTime JoinDate { get; set; }
    }
}
