using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Apex.PipeCompressors.Tests {

    [TestClass]
    public class UIntCompressionTests {

        [TestMethod]
        public async Task BulkReadWrite() {
            var pipe = new Pipe();
            var reader = pipe.Reader;
            var writer = pipe.Writer;

            _ = Task.Run(async () => {
                for (var i = 0u; i < 10000000u; i++) {
                    writer.WriteUInt(i);
                    if (i > 1000000) {
                        await Task.Delay(1000).ConfigureAwait(false);
                        await writer.FlushAsync().ConfigureAwait(false);
                    }
                }

            });

            for (var i = 0u; i < 10000000u; i++) {
                Assert.AreEqual(i, await reader.ReadUInt().ConfigureAwait(false));
            }
        }
    }
}
