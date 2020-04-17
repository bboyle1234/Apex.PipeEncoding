using Apex.TimeStamps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace Apex.PipeEncoding.Tests {

    [TestClass]
    public class UIntCompressionTests {

        [TestMethod]
        public async Task UInt_BulkReadWrite() {
            var pipe = new Pipe();
            var reader = pipe.Reader;
            var writer = pipe.Writer;

            var task1 = Task.Run(async () => {
                for (var i = 0u; i < 10000000u; i++) {
                    writer.WriteUInt(i);
                    if (i % 1000 == 0)
                        await writer.FlushAsync().ConfigureAwait(false);
                }
                await writer.FlushAsync().ConfigureAwait(false);
            });
            var task2 = Task.Run(async () => {
                for (var i = 0u; i < 10000000u; i++) {
                    Assert.AreEqual(i, await reader.ReadUInt().ConfigureAwait(false));
                }
            });
            await Task.WhenAll(task1, task2).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ULong_BulkReadWrite() {
            var pipe = new Pipe();
            var reader = pipe.Reader;
            var writer = pipe.Writer;

            var task1 = Task.Run(async () => {
                ulong value = 0;
                for (ulong i = 0; i < 1000000; i++) {
                    writer.WriteULong(value);
                    value += 10000;
                }
                await writer.FlushAsync().ConfigureAwait(false);
            });
            var task2 = Task.Run(async () => {
                ulong value = 0;
                for (ulong i = 0; i < 1000000; i++) {
                    var actual = await reader.ReadULong().ConfigureAwait(false);
                    Assert.AreEqual(value, actual);
                    value += 10000;
                }
            });
            await Task.WhenAll(task1, task2).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task TimeStampWriting() {

            var pipe = new Pipe();
            var reader = pipe.Reader;
            var writer = pipe.Writer;
            var startTime = TimeStamp.Now;

            var task1 = Task.Run(async () => {
                var time = startTime;
                for (var i = 0; i < 100000; i++) {
                    writer.WriteTimeStamp(time);
                    time = time.AddAbsoluteHours(21.65438);
                }
                await writer.FlushAsync().ConfigureAwait(false);
            });
            
            var task2 = Task.Run(async () => {
                var time = startTime;
                for (var i = 0; i < 100000; i++) {
                    Assert.AreEqual(time, await reader.ReadTimeStamp().ConfigureAwait(false));
                    time = time.AddAbsoluteHours(21.65438);
                }
            });

            await Task.WhenAll(task1, task2).ConfigureAwait(false);
        }
    }
}
