using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularPollAPI.Models
{
    public class PollAnswer
    {
        public int PollAnswerID { get;set;}
        public int PollID { set; get; }
        public string Answer { get; set; }

        public Poll Poll { get; set; }
        public ICollection<PollAnswerVote> PollAnswerVotes { get; set; }
    }
}
