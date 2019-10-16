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
                    UserFriendID = 2,
                    Status=3
                },
                new Friend()
                {
                    UserFriendID = 3,
                    Status=2
                }
            };
            pollUser.PollOwner = true;
            pollUser.User = user;
            pollUser.Poll = new Poll()
            {
                Name = "TestPoll",
                Owner = 1,
                SingleVote = true,
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

            //context.PollUsers.AddRange(pollUser);

            PollUser pollUser2 = new PollUser();

            User user2 = new User();
            user2.Username = "Admin2";
            user2.Password = "admin2";
            user2.Email = "admin2@thomasmore.be";
            user2.Friends = new List<Friend>()
            {
                new Friend()
                {
                    UserFriendID = 1,
                    Status=3
                }
            };

            user2.PollUsers = new List<PollUser>() { pollUser2 };
            pollUser2.PollOwner = true;
            pollUser2.User = user2;


            Poll Poll2 = new Poll()
            {
                Name = "Poll Invited",
                Owner = 2,
                SingleVote = false,
                PollAnswers = new List<PollAnswer>()
                {
                    new PollAnswer()
                    {
                        Answer="Yes", Poll=pollUser2.Poll
                    },
                    new PollAnswer()
                    {
                        Answer="No", Poll=pollUser2.Poll
                    },
                    new PollAnswer()
                    {
                        Answer="Maybe", Poll=pollUser2.Poll
                    }
                }

            };

            pollUser2.Poll = Poll2;

            user.PollUserInvites = new List<PollUserInvite>()
            {
                new PollUserInvite()
                {
                    Poll = Poll2
                }
            };

            context.Users.AddRange(user);
            context.Users.AddRange(user2);

            User user3 = new User();
            user3.Username = "Admin3";
            user3.Password = "admin3";
            user3.Email = "admin3@thomasmore.be";
            user3.Friends = new List<Friend>()
            {
                new Friend()
                {
                    UserFriendID = 1,
                    Status=1
                }
            };

            context.Users.AddRange(user3);
            //context.Polls.AddRange(poll);
            //context.PollAnswers.AddRange(pollAnswer, pollAnswer2);
            //context.PollAnswerVotes.Add(pollAnswerVote);

            context.SaveChanges();
        }
    }
}
