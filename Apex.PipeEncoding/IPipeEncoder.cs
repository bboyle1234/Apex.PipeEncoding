using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace Apex.PipeEncoding {

    public interface IPipeEncoder {
        void Encode(PipeWriter writer, object value);
    }

    public interface IPipeEncoder<T> : IPipeEncoder {
        void Encode(PipeWriter writer, T value);
    }
}
