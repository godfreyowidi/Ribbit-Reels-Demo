```

BenchmarkDotNet v0.15.2, macOS Sequoia 15.6 (24G84) [Darwin 24.6.0]
Apple M4 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 8.0.412
  [Host]     : .NET 8.0.18 (8.0.1825.31117), Arm64 RyuJIT AdvSIMD
  Job-WRIKEU : .NET 8.0.18 (8.0.1825.31117), Arm64 RyuJIT AdvSIMD

IterationCount=5  LaunchCount=1  WarmupCount=2  

```
| Method       | Mean     | Error    | StdDev  | Gen0   | Allocated |
|------------- |---------:|---------:|--------:|-------:|----------:|
| EfCoreLinq   | 343.8 μs |  8.07 μs | 1.25 μs | 1.4648 |  12.95 KB |
| EfCoreRawSql | 342.8 μs |  2.22 μs | 0.34 μs | 1.4648 |  15.52 KB |
| DapperQuery  | 312.1 μs | 18.23 μs | 4.73 μs | 0.4883 |   6.44 KB |
| RawAdoNet    | 314.5 μs | 13.64 μs | 3.54 μs | 0.4883 |   5.42 KB |
