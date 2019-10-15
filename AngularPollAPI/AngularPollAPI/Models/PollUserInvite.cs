using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularPollAPI.Models
{
    public class PollUserInvite
    {
        public int PollUserInviteID { get; set; }
        public int PollID { get; set; }
        public Poll Poll { get; set; }
    }
}
