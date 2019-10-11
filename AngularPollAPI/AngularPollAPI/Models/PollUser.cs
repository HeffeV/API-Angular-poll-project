using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularPollAPI.Models
{
    public class PollUser
    {
        public int PollUserID { get; set; }
        public int UserID { get; set; }
        public int PollID { get; set; }
        public User User { get; set; }
        public Poll Poll { get; set; }
    }
}
