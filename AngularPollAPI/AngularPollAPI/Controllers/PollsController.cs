using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AngularPollAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace AngularPollAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollsController : ControllerBase
    {
        private readonly PollContext _context;

        public PollsController(PollContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<Poll>> GetPollsFromUser(int userid)
        {
            var pollUser = _context.PollUsers.Where(p => p.UserID == userid).ToList();
            List<Poll> polls = new List<Poll>();

            foreach (PollUser polluser in pollUser)
            {
                polls.Add(_context.Polls.SingleOrDefault(p => p.PollID == polluser.PollID));
            }

            foreach (Poll poll in polls)
            {
                poll.PollAnswers = (_context.PollAnswers.Where(p => p.PollID == poll.PollID).ToList());

                foreach (PollAnswer pollAnswer in poll.PollAnswers)
                {
                    pollAnswer.PollAnswerVotes = _context.PollAnswerVotes.Where(p => p.PollAnswerID == pollAnswer.PollAnswerID).ToList();
                }

            }
            return polls;
        }

        [Authorize]
        [HttpGet]
        [Route("pollInvites")]
        public ActionResult<IEnumerable<Poll>> GetPollInvites(int userid)
        {
            User user = _context.Users.Include(inv => inv.PollUserInvites).SingleOrDefault(u => u.UserID == userid);
            List<Poll> polls = new List<Poll>();

            foreach (PollUserInvite pollInvite in user.PollUserInvites)
            {
                polls.Add(_context.Polls.Find(pollInvite.PollID));
            }
            return polls;
        }

        [Authorize]
        [HttpPost]
        [Route("acceptPoll")]
        public ActionResult<Poll> AcceptPoll(int userID, int pollID)
        {
            var user = _context.Users.Include(inv => inv.PollUserInvites).SingleOrDefault(p => p.UserID == userID);

            if (user != null)
            {
                var invite = user.PollUserInvites.FirstOrDefault(p => p.PollID == pollID);

                _context.PollUserInvites.Remove(invite);

                PollUser pollUser = new PollUser()
                {
                    PollID = invite.PollID,
                    UserID = user.UserID,
                    PollOwner = false
                };

                _context.PollUsers.Add(pollUser);

                _context.SaveChanges();

                var poll = _context.Polls.FirstOrDefault(p => p.PollID == pollID);

                return poll;
            }
            else return BadRequest();
        }

        [Authorize]
        [HttpPost]
        [Route("declinePoll")]
        public ActionResult<Poll> DeclinePoll(int userID, int pollID)
        {
            var user = _context.Users.Include(inv => inv.PollUserInvites).SingleOrDefault(p => p.UserID == userID);

            if (user != null)
            {
                var invite = user.PollUserInvites.FirstOrDefault(p => p.PollID == pollID);

                _context.PollUserInvites.Remove(invite);

                _context.SaveChanges();

                return Ok();
            }
            else return BadRequest();
        }
        [Authorize]
        [HttpPost]
        [Route("pollVote")]
        public ActionResult<IEnumerable<PollAnswer>> PollVote(int userID, int pollAnswerID, int pollID)
        {
            //check if poll can have multiple votes
            if (_context.Polls.Find(pollID).SingleVote)
            {
                //check for existing vote for that poll if poll only has single vote option
                var pollAnswers = _context.PollAnswers.Where(e => e.PollID == pollID);
                var PollVotes = new List<PollAnswerVote>();
                foreach (PollAnswer pa in pollAnswers)
                {
                    PollVotes.AddRange(_context.PollAnswerVotes.Where(e => e.PollAnswerID == pa.PollAnswerID && e.UserID == userID));
                }

                if (PollVotes != null)
                {
                    _context.PollAnswerVotes.RemoveRange(PollVotes);
                }

            }

            //create new vote
            PollAnswerVote pollAnswerVote = new PollAnswerVote();
            pollAnswerVote.PollAnswerID = pollAnswerID;
            pollAnswerVote.UserID = userID;
            pollAnswerVote.User = _context.Users.SingleOrDefault(e => e.UserID == userID);
            pollAnswerVote.PollAnswer = _context.PollAnswers.SingleOrDefault(e => e.PollAnswerID == pollAnswerID);

            _context.PollAnswerVotes.Add(pollAnswerVote);
            _context.SaveChanges();

            return Ok();
        }

        [Authorize]
        [HttpPost]
        [Route("inviteUserToPoll")]
        public ActionResult<IEnumerable<PollUserInvite>> InviteUserToPoll(int pollID, int userID)
        {
            var poll = _context.Polls.Find(pollID);
            var user = _context.Users.Include(e => e.PollUserInvites).Include(e => e.PollUsers).SingleOrDefault(e => e.UserID == userID);

            if (poll != null && user != null)
            {

                List<PollUserInvite> userinvites = new List<PollUserInvite>();
                userinvites.AddRange(user.PollUserInvites.Where(e => e.PollID == pollID));
                List<PollUser> pollusers = new List<PollUser>();
                pollusers.AddRange(user.PollUsers.Where(e => e.PollID == pollID));

                if (userinvites.Count > 0 || pollusers.Count > 0)
                {
                    return BadRequest();
                }

                PollUserInvite pollUserInvite = new PollUserInvite()
                {
                    Poll = poll,
                    PollID = pollID
                };

                user.PollUserInvites.Add(pollUserInvite);

                _context.Entry(user).State = EntityState.Modified;
                _context.SaveChanges();

                return Ok();
            }
            else return NotFound();
        }

        [Authorize]
        [HttpPost]
        [Route("addPoll")]
        public ActionResult<int> AddPoll (Poll poll)
        {

            var user = _context.Users.Find(poll.Owner);

            Poll newPoll = new Poll()
            {
                Name = poll.Name,
                Owner = poll.Owner,
                SingleVote = poll.SingleVote,
                PollUsers = new List<PollUser>(),
                PollAnswers = new List<PollAnswer>(),
            };

            newPoll.PollUsers.Add(new PollUser()
            {
                PollOwner = true,
                Poll = newPoll,
                User = user,
                UserID=user.UserID
            });

            _context.Polls.Add(newPoll);
            _context.SaveChanges();

            

            return newPoll.PollID;
        }

        [Authorize]
        [HttpPost]
        [Route("addAnswer")]
        public ActionResult<PollAnswer> AddAnswer(int pollID, string answer)
        {
            var poll = _context.Polls.Find(pollID);

            if (poll != null)
            {
                PollAnswer pollAnswer = new PollAnswer()
                {
                    PollID = poll.PollID,
                    Answer = answer,
                    Poll = poll,
                    PollAnswerVotes = new List<PollAnswerVote>()
                };

                _context.PollAnswers.Add(pollAnswer);
                _context.SaveChanges();

                return Ok();
            }
            else return NotFound();
        }


        [HttpGet]
        [Route("homePageStats")]
        public ActionResult<HomePageStats> HomePageStats()
        {
            HomePageStats homePageStats = new HomePageStats();

            homePageStats.AmountPolls = _context.Polls.Count();
            homePageStats.AmountUsers = _context.Users.Count();

            return homePageStats;
        }

        // GET: api/Polls/5
        [Authorize]
        [HttpGet("{id}")]
        public ActionResult<Poll> GetPoll(int id)
        {
            var poll = _context.Polls.Include(e => e.PollAnswers).ThenInclude(e => e.PollAnswerVotes).SingleOrDefault(e => e.PollID == id);

            if (poll == null)
            {
                return NotFound();
            }

            return poll;
        }

        [Authorize]
        [HttpPut]
        [Route("updatePoll")]
        public ActionResult<Poll> UpdatePoll(int pollid, string name, bool vote)
        {
            var poll = _context.Polls.Find(pollid);

            if (poll != null)
            {
                poll.Name = name;
                poll.SingleVote = vote;

                _context.Entry(poll).State = EntityState.Modified;

                _context.SaveChanges();

                return Ok();
            }
            else return NotFound();

        }

        // DELETE: api/Polls/5
        [Authorize]
        [HttpDelete("{id}")]
        public ActionResult<Poll> DeletePoll(int id)
        {
            var poll = _context.Polls.Include(e => e.PollUsers).Include(e => e.PollAnswers).ThenInclude(e => e.PollAnswerVotes).SingleOrDefault(e => e.PollID == id);
            if (poll == null)
            {
                return NotFound();
            }

            _context.Polls.Remove(poll);
            _context.PollUserInvites.RemoveRange(_context.PollUserInvites.Where(e => e.PollID == id));
            _context.SaveChanges();

            return poll;
        }

        [Authorize]
        [HttpDelete]
        [Route("deleteAnswer")]
        public ActionResult<PollAnswer> DeleteAnswer(int answerid)
        {
            var answer = _context.PollAnswers.Include(e => e.PollAnswerVotes).SingleOrDefault(e => e.PollAnswerID == answerid);

            if (answer != null)
            {
                _context.PollAnswers.Remove(answer);
                _context.SaveChanges();

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
        private bool PollExists(int id)
        {
            return _context.Polls.Any(e => e.PollID == id);
        }
    }
}
