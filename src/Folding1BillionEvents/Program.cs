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
        private EventBase[] _eventStream;

        [GlobalSetup(Targets = new[] { nameof(MutableFold), nameof(ImmutableFold), nameof(FilterEventStream) })]
        public void Setup()
        {
            var eventStream = new List<EventBase>();
            var userId = Guid.NewGuid().ToString();
            eventStream.AddRange(CreateNoopEvents(100_000_000));
            eventStream.AddRange(CreateRoomAssignedEvents(100_000_000));
            eventStream.AddRange(CreateUserChangedEvents(userId, 400_000_000));
            eventStream.AddRange(CreateUserVotedEvents(userId, 400_000_000));
            _eventStream = eventStream.OrderBy(a => Guid.NewGuid()).ToArray(); //randomize
            _eventStream[0] = CreateRegisterEvent(userId);
            Console.WriteLine($"// Numer of Events to Process: {_eventStream.Length}");
        }

        [GlobalSetup(Targets = new[] { nameof(MutableFoldWithFilter), nameof(ImmutableFoldWithFilter) })]
        public void SetupWithFilters()
        {
            var eventStream = new List<EventBase>();
            var userId = Guid.NewGuid().ToString();
            eventStream.AddRange(CreateNoopEvents(100_000_000));
            eventStream.AddRange(CreateRoomAssignedEvents(100_000_000));
            eventStream.AddRange(CreateUserChangedEvents(userId, 400_000_000));
            eventStream.AddRange(CreateUserVotedEvents(userId, 400_000_000));
            _eventStream = eventStream.OrderBy(a => Guid.NewGuid()).ToArray(); //randomize
            _eventStream[0] = CreateRegisterEvent(userId);
            _eventStream = _eventStream.Where(e => e is UserRegistered || e is UserChangedName || e is UserVoted).ToArray();
            Console.WriteLine($"// Numer of Events to Process: {_eventStream.Length}");
        }

        [GlobalSetup(Targets = new[] { nameof(MutableFoldOnlyUserEvents), nameof(ImmutableFoldOnlyUserEvents) })]
        public void SetupReduced()
        {
            var eventStream = new List<EventBase>();
            var userId = Guid.NewGuid().ToString();
            eventStream.AddRange(CreateUserChangedEvents(userId, 400_000_000));
            eventStream.AddRange(CreateUserVotedEvents(userId, 400_000_000));
            _eventStream = eventStream.OrderBy(a => Guid.NewGuid()).ToArray(); //randomize
            _eventStream[0] = CreateRegisterEvent(userId);
            Console.WriteLine($"// Numer of Events to Process: {_eventStream.Length}");
        }

        [Benchmark]
        public User MutableFold() => FoldUser.MuteableFold(_eventStream.AsSpan());

        [Benchmark]
        public User ImmutableFold() => FoldUser.ImmuteableFold(_eventStream.AsSpan());

        [Benchmark]
        public EventBase[] FilterEventStream() => _eventStream.Where(e => e is UserRegistered || e is UserChangedName || e is UserVoted).ToArray();

        [Benchmark]
        public User MutableFoldWithFilter() => FoldUser.MuteableFold(_eventStream.AsSpan());

        [Benchmark]
        public User ImmutableFoldWithFilter() => FoldUser.ImmuteableFold(_eventStream.AsSpan());

        [Benchmark]
        public User MutableFoldOnlyUserEvents() => FoldUser.MuteableFold(_eventStream.AsSpan());

        [Benchmark]
        public User ImmutableFoldOnlyUserEvents() => FoldUser.ImmuteableFold(_eventStream.AsSpan());

        public static void Main(string[] args) => BenchmarkSwitcher.FromAssemblies(new[] { typeof(Program).Assembly }).Run(args);

        public static IEnumerable<EventBase> CreateUserChangedEvents(string userId, int count)
        {
            for(int i = 0;i<count;i++)
            {
                yield return CreateUserChangedNameEvent(userId, i.ToString());
            }
        }

        public static IEnumerable<EventBase> CreateUserVotedEvents(string userId, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return CreateUserVotedEvent(userId);
            }
        }

        public static IEnumerable<EventBase> CreateNoopEvents(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return CreateNoopEvent();
            }
        }

        public static IEnumerable<EventBase> CreateRoomAssignedEvents(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return CreateRoomAssignedEvent(i.ToString());
            }
        }


        public static EventBase CreateRegisterEvent(string userId)
        {
            return new UserRegistered
            {
                FirstName = "Sia",
                LastName = "Der",
                UserId = userId
            };
        }

        public static EventBase CreateUserChangedNameEvent(string userId, string suffix)
        {
            return new UserChangedName
            {
                FirstName = $"Sia-{suffix}",
                LastName = $"Der-{suffix}",
                UserId = userId
            };
        }

        public static EventBase CreateUserVotedEvent(string userId)
        {
            return new UserVoted
            {
                UserId = userId
            };
        }

        public static EventBase CreateNoopEvent()
        {
            return new NoopEvent();
        }

        public static EventBase CreateRoomAssignedEvent(string roomId)
        {
            return new RoomAssigned
            {
                RoomId = roomId,
                RoomName = $"Room-{roomId}"
            };
        }
    }
}
