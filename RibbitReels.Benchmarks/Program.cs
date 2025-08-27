using BenchmarkDotNet.Running;

namespace RibbitReels.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<EfVsDapperBenchmarks>();
        }
    }
}
