using System;

namespace Apex.PipeCompressors {

    public interface IPipeWriterFactory {
        IPipeWriter<TMessage> GetPipeWriter<TMessage>();
        IPipeWriter GetPipeWriter(Type messageType);
    }
}
