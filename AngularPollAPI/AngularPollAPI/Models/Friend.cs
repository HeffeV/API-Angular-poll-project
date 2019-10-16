using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularPollAPI.Models
{
    public class Friend
    {
        public int FriendID { get; set; }
        public int UserFriendID { get; set; }
        
        //1==sender//2==receiver//3==accepted
        public int Status { get; set; }
    }
}
