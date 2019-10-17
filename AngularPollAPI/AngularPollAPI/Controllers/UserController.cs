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
                return Ok();
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
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpPost]
        [Route("addFriend")]
        public ActionResult<Friend> AddFriend(int userid, string friendEmail)
        {
            if (!_context.Users.Any(e => e.Email == friendEmail))
            {
                FriendWaitList friendWaitList = new FriendWaitList()
                {
                    SenderUserID = userid,
                    UserEmail = friendEmail
                };

                _context.FriendWaitLists.Add(friendWaitList);

                _context.SaveChanges();

                return BadRequest();
            }
            else
            {
                var UserFriend = _context.Users.Include(e=>e.Friends).SingleOrDefault(e => e.Email == friendEmail);
                var user = _context.Users.Include(e => e.Friends).SingleOrDefault(e => e.UserID==userid) ;

                Friend friend1 = new Friend()
                {
                    Status = 1,
                    UserFriendID = UserFriend.UserID
                };
                Friend friend2 = new Friend()
                {
                    Status = 2,
                    UserFriendID = user.UserID
                };

                UserFriend.Friends.Add(friend2);
                user.Friends.Add(friend1);

                _context.Entry(UserFriend).State = EntityState.Modified;
                _context.Entry(user).State = EntityState.Modified;

                _context.SaveChanges();

                return Ok();

            }
        }


        // POST: api/User
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (!_context.Users.Any(e=>e.Username==user.Username)&& !_context.Users.Any(e => e.Email == user.Email))
            {
                //check if already invited before
                var friendwaitlist = _context.FriendWaitLists.SingleOrDefault(e => e.UserEmail == user.Email);
                if (friendwaitlist != null)
                {
                    user.Friends = new List<Friend>()
                    {
                        new Friend()
                        {
                            Status=2,
                            UserFriendID = friendwaitlist.SenderUserID
                        }
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    //add new user to existingUser friends
                    var existingUser = _context.Users.Include(e => e.Friends).SingleOrDefault(e => e.UserID == friendwaitlist.SenderUserID);
                    existingUser.Friends.Add(new Friend()
                    {
                        Status = 1,
                        UserFriendID = _context.Users.SingleOrDefault(e=>e.Email==user.Email).UserID
                    });

                    _context.FriendWaitLists.Remove(friendwaitlist);
                    _context.Entry(existingUser).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                else
                {
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }


                return CreatedAtAction("GetUser", new { id = user.UserID }, user);
            }
            else
            {
                return BadRequest();
            }

        }
    }
}
