using System;

namespace Apex.PipeEncoding {

    public interface IPipeEncoderFactory {
        IPipeEncoder<TMessage> GetPipeWriter<TMessage>();
        IPipeEncoder GetPipeWriter(Type messageType);
    }
}
