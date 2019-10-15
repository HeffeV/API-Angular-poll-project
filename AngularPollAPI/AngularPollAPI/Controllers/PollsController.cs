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

            foreach(Poll poll in polls)
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
        public ActionResult<Poll> AcceptPoll(int userID,int pollID)
        {
            var user = _context.Users.Include(inv=>inv.PollUserInvites).SingleOrDefault(p=>p.UserID==userID);

            if (user != null)
            {
                var invite = user.PollUserInvites.FirstOrDefault(p => p.PollID == pollID);

                _context.PollUserInvites.Remove(invite);

                PollUser pollUser = new PollUser()
                {
                    PollID = invite.PollID,
                    UserID = user.UserID
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

                return null;
            }
            else return BadRequest();
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
        [HttpGet("{id}")]
        public async Task<ActionResult<Poll>> GetPoll(int id)
        {
            var poll = await _context.Polls.FindAsync(id);

            if (poll == null)
            {
                return NotFound();
            }

            return poll;
        }

        // PUT: api/Polls/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPoll(int id, Poll poll)
        {
            if (id != poll.PollID)
            {
                return BadRequest();
            }

            _context.Entry(poll).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PollExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Polls
        [HttpPost]
        public async Task<ActionResult<Poll>> PostPoll(Poll poll)
        {
            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPoll", new { id = poll.PollID }, poll);
        }

        // DELETE: api/Polls/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Poll>> DeletePoll(int id)
        {
            var poll = await _context.Polls.FindAsync(id);
            if (poll == null)
            {
                return NotFound();
            }

            _context.Polls.Remove(poll);
            await _context.SaveChangesAsync();

            return poll;
        }

        private bool PollExists(int id)
        {
            return _context.Polls.Any(e => e.PollID == id);
        }
    }
}
