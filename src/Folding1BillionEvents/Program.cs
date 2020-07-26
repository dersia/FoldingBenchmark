using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Folding1BillionEvents.Events;
using Folding1BillionEvents.Functions;
using Folding1BillionEvents.States;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Folding1BillionEvents
{
    [MemoryDiagnoser]
    public class Program
    {
        private EventBase[] eventStream;
        private List<EventBase> eventStreamEx;

        [GlobalSetup(Targets = new[] { nameof(MutableFold), nameof(ImmutableFold), nameof(MutableFoldWithFilter), nameof(ImmutableFoldWithFilter), nameof(FilterEventStream) })]
        public void Setup()
        {
            eventStream = new EventBase[1_000_000_000];
            var userId = Guid.NewGuid().ToString();
            eventStream.Concat(CreateNoopEvents(100_000_000));
            eventStream.Concat(CreateRoomAssignedEvents(100_000_000));
            eventStream.Concat(CreateUserChangedEvents(userId, 400_000_000));
            eventStream.Concat(CreateUserVotedEvents(userId, 400_000_000));
            eventStream.OrderBy(a => Guid.NewGuid()); //randomize
            eventStream[0] = CreateRegisterEvent(userId);
        }

        [GlobalSetup(Targets = new[] { nameof(MutableFoldOnlyUserEvents), nameof(ImmutableFoldOnlyUserEvents) })]
        public void SetupWithFilter()
        {
            eventStream = new EventBase[1_000_000_000];
            var userId = Guid.NewGuid().ToString();
            eventStream.Concat(CreateUserChangedEvents(userId, 400_000_000));
            eventStream.Concat(CreateUserVotedEvents(userId, 400_000_000));
            eventStream.OrderBy(a => Guid.NewGuid()); //randomize
            eventStream[0] = CreateRegisterEvent(userId);
        }

        [Benchmark]
        public User MutableFold() => FoldUser.MuteableFold(eventStream.AsSpan());

        [Benchmark]
        public User ImmutableFold() => FoldUser.ImmuteableFold(eventStream.AsSpan());

        [Benchmark]
        public EventBase[] FilterEventStream() => eventStream.Where(e => e is UserRegistered || e is UserChangedName || e is UserVoted).ToArray();

        [Benchmark]
        public User MutableFoldWithFilter() => FoldUser.MuteableFold(eventStream.Where(e => e is UserRegistered || e is UserChangedName || e is UserVoted).ToArray().AsSpan());

        [Benchmark]
        public User ImmutableFoldWithFilter() => FoldUser.ImmuteableFold(eventStream.Where(e => e is UserRegistered || e is UserChangedName || e is UserVoted).ToArray().AsSpan());

        [Benchmark]
        public User MutableFoldOnlyUserEvents() => FoldUser.MuteableFold(eventStream.AsSpan());

        [Benchmark]
        public User ImmutableFoldOnlyUserEvents() => FoldUser.ImmuteableFold(eventStream.AsSpan());

        public static void Main(string[] args) => BenchmarkSwitcher.FromAssemblies(new[] { typeof(Program).Assembly }).Run(args);

        public IEnumerable<EventBase> CreateUserChangedEvents(string userId, int count)
        {
            for(int i = 0;i<count;i++)
            {
                yield return CreateUserChangedNameEvent(userId, i.ToString());
            }
        }

        public IEnumerable<EventBase> CreateUserVotedEvents(string userId, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return CreateUserVotedEvent(userId);
            }
        }

        public IEnumerable<EventBase> CreateNoopEvents(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return CreateNoopEvent();
            }
        }

        public IEnumerable<EventBase> CreateRoomAssignedEvents(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return CreateRoomAssignedEvent(i.ToString());
            }
        }


        public EventBase CreateRegisterEvent(string userId)
        {
            return new UserRegistered
            {
                FirstName = "Sia",
                LastName = "Der",
                UserId = userId
            };
        }

        public EventBase CreateUserChangedNameEvent(string userId, string suffix)
        {
            return new UserChangedName
            {
                FirstName = $"Sia-{suffix}",
                LastName = $"Der-{suffix}",
                UserId = userId
            };
        }

        public EventBase CreateUserVotedEvent(string userId)
        {
            return new UserVoted
            {
                UserId = userId
            };
        }

        public EventBase CreateNoopEvent()
        {
            return new NoopEvent();
        }

        public EventBase CreateRoomAssignedEvent(string roomId)
        {
            return new RoomAssigned
            {
                RoomId = roomId,
                RoomName = $"Room-{roomId}"
            };
        }
    }
}
