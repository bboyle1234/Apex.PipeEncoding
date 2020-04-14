using System;

namespace Apex.PipeEncoding {

    public interface IPipeDecoderFactory {
        IPipeDecoder<T> GetPipeReader<T>();
        IPipeDecoder GetPipeReader(Type messageType);
    }
}
