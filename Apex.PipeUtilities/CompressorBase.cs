using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Apex.PipeCompressors {

    public abstract class CompressorBase<T> : IPipeReader<T>, IPipeWriter<T> {
        void IPipeWriter.Write(PipeWriter writer, object value) => Write(writer, (T)value);
        async ValueTask<object> IPipeReader.Read(PipeReader reader) => await Read(reader).ConfigureAwait(false);

        public abstract void Write(PipeWriter writer, T value);
        public abstract ValueTask<T> Read(PipeReader reader);
    }
}
