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



            PollUser pollUser = new PollUser();

            User user = new User();
            user.Username = "Admin";
            user.Password = "admin";
            user.Email = "admin@thomasmore.be";
            user.PollUsers = new List<PollUser>() { pollUser};

            pollUser.User = user;
            pollUser.Poll = new Poll()
            {
                Name = "TestPoll",
                Accepted = true,
                PollAnswers = new List<PollAnswer>()
                {
                    new PollAnswer()
                    {
                        Answer="Yes", Poll=pollUser.Poll,
                        PollAnswerVotes = new List<PollAnswerVote>()
                        {
                            new PollAnswerVote()
                            {
                                User = user
                            }
                        }
                    },
                    new PollAnswer()
                    {
                        Answer="No", Poll=pollUser.Poll
                    }
                }
                
            };

            context.Users.AddRange(user);
            context.PollUsers.AddRange(pollUser);

            pollUser = new PollUser();

            pollUser.User = user;
            pollUser.Poll = new Poll()
            {
                Name = "NotMyTestPoll",
                Accepted = false,
                PollAnswers = new List<PollAnswer>()
                {
                    new PollAnswer()
                    {
                        Answer="Yes", Poll=pollUser.Poll
                    },
                    new PollAnswer()
                    {
                        Answer="No", Poll=pollUser.Poll
                    }
                }

            };
            context.PollUsers.AddRange(pollUser);

            //context.Polls.AddRange(poll);
            //context.PollAnswers.AddRange(pollAnswer, pollAnswer2);
            //context.PollAnswerVotes.Add(pollAnswerVote);

            context.SaveChanges();
        }
    }
}
