using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Apex.PipeCompressors {

    public interface IPipeReader {
        ValueTask<object> Read(PipeReader reader);
    }

    public interface IPipeReader<T> : IPipeReader {
        ValueTask<T> Read(PipeReader reader);
    }
}
