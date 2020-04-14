using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Apex.PipeEncoding {

    public abstract class EncoderBase<T> : IPipeDecoder<T>, IPipeEncoder<T> {
        void IPipeEncoder.Encode(PipeWriter writer, object value) => Encode(writer, (T)value);
        async ValueTask<object> IPipeDecoder.Decode(PipeReader reader) => await Decode(reader).ConfigureAwait(false);

        public abstract void Encode(PipeWriter writer, T value);
        public abstract ValueTask<T> Decode(PipeReader reader);
    }
}
