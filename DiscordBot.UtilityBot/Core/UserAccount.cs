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
        [Unique]
        public long DiscordId { get; set; }
        [MaxLength(255)]
        public string Username { get; set; }
        [MaxLength(255)]
        public DateTimeOffset JoinDate { get; set; }
        [MaxLength(1)]
        public int IsMember { get; set; }
        [MaxLength(255)]
        public DateTimeOffset LeaveDate { get; set; }
    }

    public class MessageInfo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique]
        public long DiscordId { get; set; }
        [MaxLength(2000)]
        public string Message { get; set; }
        [MaxLength(255)]
        public DateTimeOffset MessageDate { get; set; }
    }
}
