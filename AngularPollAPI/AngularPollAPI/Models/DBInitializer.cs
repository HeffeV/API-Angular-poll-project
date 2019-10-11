using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularPollAPI.Models
{
    public class DBInitializer
    {
        public static void Initialize(PollContext context)
        {
            context.Database.EnsureCreated();
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }
            context.Users.AddRange(new User 
            { Username = "Admin", Password = "admin", 
                Email = "admin@thomasmore.be" 
            });
            context.SaveChanges();
        }
    }
}
