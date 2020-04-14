using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System;

namespace Apex.PipeEncoding.Benchmarking {
    class Program {
        static void Main(string[] args) {
            var summary = BenchmarkRunner.Run<UIntBenchmarking>(new DebugBuildConfig());
        }
    }
}
