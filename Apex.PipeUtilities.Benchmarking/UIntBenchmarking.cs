using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Apex.PipeCompressors.Benchmarking {

    [MemoryDiagnoser]
    public class UIntBenchmarking {

        PipeReader reader;

        [GlobalSetup]
        public void IterationSetup() {
            var pipe = new Pipe();
            reader = pipe.Reader;
            var writer = pipe.Writer;
            for (var i = 0u; i < 100000000u; i++) {
                writer.WriteUInt(i);
            }
            _ = Task.Run(() => writer.FlushAsync());
        }

        [Benchmark]
        public ValueTask<uint> BulkRead1() {
            return reader.ReadUInt();
        }
    }
}
