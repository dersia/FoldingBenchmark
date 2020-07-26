## Folding 1 Billion Events in a State Benchmark

# Benchmarking
I am no Benchmark expert, I just use BenchmarkDotNet to create a basic Benchmark about how long it'll take to fold(aggregate) a billion events, when doing EventSourcing.
If you can provide imporvements to this, please submit a pull-request

# The Idea and Setup
There are 5 differnet EventTypes, 1 State(aggregate) and 1 folding function.
2 of those EventTypes (`NoopEvent` and `RoomAssigned`) are of no interest to the User-State and therefor they are doing notthing to the state and are skipped over.
The other 3 (`UserRegistered`, `UserChangedName` and `UserVoted`) are proccessed.
The Setup creates 100_000_000 `NoopEvent`, 100_000_000 `RoomAssigned`, 400_000_000 `UserChangedName` and 400_000_000 `UserVoted` Events on the in memory-eventstream (simple array) and then shuffles those by `eventStream.OrderBy(a => Guid.NewGui());`
Then the first event is replaced by an `UserRegistered` event.

# The Benchmark
There are 7 Benchmarks
* Folding All Events with a mutable State and not skipping over unwanted events (no-filtering 1_000_000_000 events)
* Folding All Events with an immutable State and not skipping over unwanted events (no-filtering 1_000_000_000 events)
* Filtering of the EventStream (in-proc-filtering 800_000_000 events)
* Folding All Events with a mutable State and skipping over unwanted events (in-proc-filtering 800_000_000 events)
* Folding All Events with an immutable State and skipping over unwanted events (in-proc-filtering 800_000_000 events)
* Folding All Events with a mutable State of filtered at source events (at-source-filtering 800_000_000 events)
* Folding All Events with an immutable State of filtered at source events (at-source-filtering 800_000_000 events)

# Runtime And Machine
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.20161
AMD Ryzen Threadripper 3970X, 1 CPU, 64 logical and 32 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]     : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
  Job-SOVMSV : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT

Runtime=.NET Core 5.0  Toolchain=netcoreapp50

# Note
At the time this benchmarks were running, I had other workloads running on my machine

# Results

|                      Method |    Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------- |--------:|---------:|---------:|------:|------:|------:|----------:|
|                 MutableFold | 3.678 s | 0.0086 s | 0.0080 s |     - |     - |     - |    1392 B |
|               ImmutableFold | 5.364 s | 0.0215 s | 0.0191 s |     - |     - |     - |      48 B |
|           FilterEventStream | 2.970 s | 0.0084 s | 0.0070 s |     - |     - |     - |     136 B |
|       MutableFoldWithFilter | 2.969 s | 0.0067 s | 0.0063 s |     - |     - |     - |     184 B |
|     ImmutableFoldWithFilter | 2.957 s | 0.0086 s | 0.0080 s |     - |     - |     - |    1528 B |
|   MutableFoldOnlyUserEvents | 3.699 s | 0.0181 s | 0.0169 s |     - |     - |     - |    1392 B |
| ImmutableFoldOnlyUserEvents | 5.304 s | 0.0134 s | 0.0118 s |     - |     - |     - |      48 B |