using System;

namespace Apex.PipeCompressors {

    public interface IPipeReaderFactory {
        IPipeReader<T> GetPipeReader<T>();
        IPipeReader GetPipeReader(Type messageType);
    }
}
