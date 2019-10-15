using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AngularPollAPI.Models;

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
        [HttpGet]
        [Route("getFriends")]
        public ActionResult<IEnumerable<User>> GetUserFriends(int userid)
        {
            var userFriends = _context.Users.Include(u => u.Friends).SingleOrDefault(u => u.UserID == userid).Friends;

            List<User> friends = new List<User>();

            foreach (Friend friend in userFriends)
            {
                friends.Add(_context.Users.SingleOrDefault(u => u.UserID == friend.UserFriendID));
            }

            return friends;
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
