using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularPollAPI.Models
{
    public class PollAnswerVote
    {
        public int PollAnswerVoteID { get; set; }
        public int PollAnswerID { get; set; }
        public int UserID { get; set; }
        public PollAnswer PollAnswer { get; set; }
        public User User { get; set; }
    }
}
