using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Apex.PipeEncoding {

    public interface IPipeDecoder {
        ValueTask<object> Decode(PipeReader reader);
    }

    public interface IPipeDecoder<T> : IPipeDecoder {
        new ValueTask<T> Decode(PipeReader reader);
    }
}
