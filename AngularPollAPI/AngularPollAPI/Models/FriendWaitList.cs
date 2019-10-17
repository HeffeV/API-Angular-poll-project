using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularPollAPI.Models
{
    public class FriendWaitList
    {
        public int FriendWaitListID { get; set; }
        public int SenderUserID { get; set; }
        public string UserEmail { get; set; }
    }
}
