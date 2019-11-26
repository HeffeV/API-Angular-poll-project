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
                //find the user data with the userid from the userFriends list.
                User temp = new User();
                //status 3 is accepted friends
                temp = _context.Users.SingleOrDefault(u => u.UserID == friend.UserFriendID && friend.Status == 3);
                if (temp != null)
                {
                    //remove the password and add the object to the friends list.
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
                //find the user data with the userid from the userFriends list.
                User temp = new User();
                //status 2 is requests.
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
            //get the friends object from the users friendlist.
            var userFriend = _context.Users.Include(u => u.Friends).SingleOrDefault(u => u.UserID == userid).Friends.SingleOrDefault(f=>f.UserFriendID==friendid);

            if (userFriend != null)
            {
                //remove the userfriend object.
                _context.Friends.Remove(userFriend);
            }
            else
            {
                return BadRequest();
            }

            //get the friends object from the other user his friendlist
            userFriend = _context.Users.Include(u => u.Friends).SingleOrDefault(u => u.UserID == friendid).Friends.SingleOrDefault(f => f.UserFriendID == userid);

            if (userFriend != null)
            {
                //remove this userfriend object aswell.
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
            //get the Userfriend object from the users friendlist
            var userFriend = _context.Users.Include(u => u.Friends).SingleOrDefault(u => u.UserID == userid).Friends.SingleOrDefault(f => f.UserFriendID == friendid);

            if (userFriend != null)
            {
                //set the status to 3 (accepted) and set the object as modified.
                userFriend.Status = 3;
                _context.Entry(userFriend).State = EntityState.Modified;
            }
            else
            {
                return BadRequest();
            }

            //get the senders userfriend object from the friendlist.
            var userFriend2 = _context.Users.Include(u => u.Friends).SingleOrDefault(u => u.UserID == friendid).Friends.SingleOrDefault(f => f.UserFriendID == userid);

            if (userFriend2 != null)
            {
                //set the status to 3 (accepted) and set the object as modified. and save all changes
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
            //check if there is an user with this email. if there is not add email to waitlist to await registration with this email
            if (!_context.Users.Any(e => e.Email == friendEmail))
            {
                //add new friendwaitlist object with the userid and the invited user email.
                FriendWaitList friendWaitList = new FriendWaitList()
                {
                    SenderUserID = userid,
                    UserEmail = friendEmail
                };

                //add invite and save
                _context.FriendWaitLists.Add(friendWaitList);

                _context.SaveChanges();

                return BadRequest();
            }
            else
            {
                //email exists
                //get the user object from the friend
                var UserFriend = _context.Users.Include(e=>e.Friends).SingleOrDefault(e => e.Email == friendEmail);
                //get sender his user object
                var user = _context.Users.Include(e => e.Friends).SingleOrDefault(e => e.UserID==userid) ;

                //add to both users a new friend with the status of sender/reciever and the other persons userid
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

                //add friends and save them to the databank
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
            //check if username and email are both not taken already
            if (!_context.Users.Any(e=>e.Username==user.Username)&& !_context.Users.Any(e => e.Email == user.Email))
            {
                //check if already invited before to another user his friendlist
                var friendwaitlist = _context.FriendWaitLists.SingleOrDefault(e => e.UserEmail == user.Email);
                if (friendwaitlist != null)
                {
                    //add friend request to this user
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
