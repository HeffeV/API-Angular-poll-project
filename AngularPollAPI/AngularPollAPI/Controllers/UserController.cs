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
    public class UserController : ControllerBase
    {
        private readonly PollContext _context;

        public UserController(PollContext context)
        {
            _context = context;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
        [Authorize]
        [HttpGet]
        [Route("getFriends")]
        public ActionResult<IEnumerable<User>> GetUserFriends(int userid)
        {
            var userFriends = _context.Users.Include(u => u.Friends).SingleOrDefault(u => u.UserID == userid).Friends;

            List<User> friends = new List<User>();

            foreach (Friend friend in userFriends)
            {
                User temp = new User();
                temp = _context.Users.SingleOrDefault(u => u.UserID == friend.UserFriendID && friend.Status == 3);
                if (temp != null)
                {
                    temp.Password = null;
                    friends.Add(temp);
                }
            }

            return friends;
        }
        [Authorize]
        [HttpGet]
        [Route("getFriendRequests")]
        public ActionResult<IEnumerable<User>> GetFriendRequests(int userid)
        {
            var userFriends = _context.Users.Include(u => u.Friends).SingleOrDefault(u => u.UserID == userid).Friends;

            List<User> friends = new List<User>();

            foreach (Friend friend in userFriends)
            {
                User temp = new User();
                temp = _context.Users.SingleOrDefault(u => u.UserID == friend.UserFriendID && friend.Status == 2);
                if (temp != null)
                {
                    temp.Password = null;
                    friends.Add(temp);
                }
            }

            return friends;
        }
        [Authorize]
        [HttpPost]
        [Route("deleteFriend")]
        public ActionResult<Friend> DeleteFriend(int userid,int friendid)
        {
            var userFriend = _context.Users.Include(u => u.Friends).SingleOrDefault(u => u.UserID == userid).Friends.SingleOrDefault(f=>f.UserFriendID==friendid);

            if (userFriend != null)
            {
                _context.Friends.Remove(userFriend);
            }
            else
            {
                return BadRequest();
            }

            userFriend = _context.Users.Include(u => u.Friends).SingleOrDefault(u => u.UserID == friendid).Friends.SingleOrDefault(f => f.UserFriendID == userid);

            if (userFriend != null)
            {
                _context.Friends.Remove(userFriend);
                _context.SaveChanges();
                return null;
            }
            else
            {
                return BadRequest();
            }
        }
        [Authorize]
        [HttpPost]
        [Route("acceptFriend")]
        public ActionResult<Friend> AcceptFriend(int userid, int friendid)
        {
            var userFriend = _context.Users.Include(u => u.Friends).SingleOrDefault(u => u.UserID == userid).Friends.SingleOrDefault(f => f.UserFriendID == friendid);
            // _context.Entry(poll).State = EntityState.Modified;
            if (userFriend != null)
            {
                userFriend.Status = 3;
                _context.Entry(userFriend).State = EntityState.Modified;
            }
            else
            {
                return BadRequest();
            }

            var userFriend2 = _context.Users.Include(u => u.Friends).SingleOrDefault(u => u.UserID == friendid).Friends.SingleOrDefault(f => f.UserFriendID == userid);

            if (userFriend2 != null)
            {
                userFriend2.Status = 3;
                _context.Entry(userFriend2).State = EntityState.Modified;
                _context.SaveChanges();
                return null;
            }
            else
            {
                return BadRequest();
            }
        }


        // POST: api/User
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (!_context.Users.Any(e=>e.Username==user.Username)&& !_context.Users.Any(e => e.Email == user.Email))
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetUser", new { id = user.UserID }, user);
            }
            else
            {
                return BadRequest();
            }

        }
    }
}
