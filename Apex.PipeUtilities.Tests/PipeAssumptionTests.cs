using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Apex.PipeCompressors.Tests {

    [TestClass]
    public class PipeAssumptionTests {

        [TestMethod]
        public void TestDecimalConversion () {
            var value = 1645.25M;
            var bits = Decimal.GetBits(value);
        }

        [TestMethod]
        public async Task ObservingWithoutConsuming() {
            var pipe = new Pipe();
            var tWrite = Task.Run(async () => {
                var writer = pipe.Writer;

                var memory = writer.GetMemory(4);
                memory.Span[0] = 0;
                memory.Span[1] = 1;
                memory.Span[2] = 2;
                memory.Span[3] = 3;
                writer.Advance(4);
                await writer.FlushAsync().ConfigureAwait(false);

                memory = writer.GetMemory(6);
                memory.Span[0] = 4;
                memory.Span[1] = 5;
                memory.Span[2] = 6;
                memory.Span[3] = 7;
                memory.Span[4] = 8;
                memory.Span[5] = 9;
                writer.Advance(6);
                await writer.FlushAsync().ConfigureAwait(false);
            });

            var tRead = Task.Run(async () => {
                try {
                    var reader = pipe.Reader;
                    var readResult = await reader.ReadAsync(default).ConfigureAwait(false);
                    if (readResult.IsCanceled) throw new OperationCanceledException();
                    var buffer = readResult.Buffer;
                    while (buffer.Length < 8) {
                        reader.AdvanceTo(buffer.Start, buffer.End);
                        readResult = await reader.ReadAsync(default).ConfigureAwait(false);
                        if (readResult.IsCanceled) throw new OperationCanceledException();
                        buffer = readResult.Buffer;
                    }
                    Memory<byte> bytes = new byte[8];
                    buffer.Slice(0,8).CopyTo(bytes.Span);
                } catch (Exception x) {
                    throw;
                }
            });


            await Task.WhenAll(tWrite, tRead).ConfigureAwait(false);
        }
    }
}
