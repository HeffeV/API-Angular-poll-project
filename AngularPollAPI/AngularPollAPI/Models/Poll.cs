using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularPollAPI.Models
{
    public class Poll
    {
        public int PollID { get; set; }
        public string Name { get; set; }
        public bool SingleVote { get; set; }
        public int Owner { get; set; }
        public ICollection<PollAnswer> PollAnswers { get; set; }
        public ICollection<PollUser> PollUsers { get; set; }
    }
}
