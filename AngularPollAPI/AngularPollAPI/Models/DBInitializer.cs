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
            user.PollUsers = new List<PollUser>() { pollUser };

            user.Friends = new List<Friend>()
            {
                new Friend()
                {
                    UserFriendID = 2
                }
            };

            pollUser.User = user;
            pollUser.Poll = new Poll()
            {
                Name = "TestPoll",
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

            context.PollUsers.AddRange(pollUser);

            Poll Poll = new Poll()
            {
                Name = "Poll Invited",
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

            user.PollUserInvites = new List<PollUserInvite>()
            {
                new PollUserInvite()
                {
                    Poll = Poll
                }
            };

        context.Users.AddRange(user);
            context.Polls.AddRange(Poll);

            User user2 = new User();
            user2.Username = "Admin2";
            user2.Password = "admin2";
            user2.Email = "admin2@thomasmore.be";
            user2.Friends = new List<Friend>()
            {
                new Friend()
                {
                    UserFriendID = 1
                }
            };
            context.Users.AddRange(user2);

            //context.Polls.AddRange(poll);
            //context.PollAnswers.AddRange(pollAnswer, pollAnswer2);
            //context.PollAnswerVotes.Add(pollAnswerVote);

            context.SaveChanges();
        }
    }
}
