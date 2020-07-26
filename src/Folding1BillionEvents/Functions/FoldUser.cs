using Folding1BillionEvents.Events;
using Folding1BillionEvents.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folding1BillionEvents.Functions
{
    public static class FoldUser
    {
        public static User MuteableFold(ReadOnlySpan<EventBase> events)
        {
            var user = new User();
            foreach(var @event in events)
            {
                MutableApply(user, @event);
            }
            return user;
        }

        public static User ImmuteableFold(ReadOnlySpan<EventBase> events)
        {
            var user = new User();
            foreach (var @event in events)
            {
                user = ImmutableApply(user, @event);
            }
            return user;
        }

        private static void MutableApply(User state, EventBase @event)
        {
            switch(@event)
            {
                case NoopEvent _: break;
                case RoomAssigned _: break;
                case UserRegistered registered:
                    if(string.IsNullOrWhiteSpace(state.UserId))
                    {
                        state.UserId = registered.UserId;
                        state.LastName = registered.LastName;
                        state.FirstName = registered.FirstName;
                    }
                    break;
                case UserChangedName changedName:
                    if(!string.IsNullOrWhiteSpace(state.UserId))
                    {
                        state.LastName = changedName.LastName;
                        state.FirstName = changedName.FirstName;
                    }
                    break;
                case UserVoted voted:
                    if (!string.IsNullOrWhiteSpace(state.UserId))
                    {
                        state.TimesVoted++;
                    }
                    break;
            }
        }

        private static User ImmutableApply(User state, EventBase @event)
        {
            var alreadyRegistered = string.IsNullOrWhiteSpace(state.UserId);
            return @event switch {
                NoopEvent _ => state,
                RoomAssigned _ => state,
                UserRegistered registered => alreadyRegistered ? state : new User { UserId = registered.UserId, FirstName = registered.FirstName, LastName = registered.LastName },
                UserChangedName changedName => alreadyRegistered ? new User { UserId = changedName.UserId, FirstName = changedName.FirstName, LastName = changedName.LastName, TimesVoted = state.TimesVoted } : state,
                UserVoted voted => alreadyRegistered ? new User { UserId = state.UserId, FirstName = state.FirstName, LastName = state.LastName, TimesVoted = ++state.TimesVoted } : state,
                _ => state
            };
        }
    }
}
