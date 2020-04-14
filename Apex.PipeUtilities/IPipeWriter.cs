using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace Apex.PipeCompressors {

    public interface IPipeWriter {
        void Write(PipeWriter writer, object value);
    }

    public interface IPipeWriter<T> : IPipeWriter {
        void Write(PipeWriter writer, T value);
    }
}
