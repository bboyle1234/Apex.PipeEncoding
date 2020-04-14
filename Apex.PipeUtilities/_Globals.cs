using System;
using System.Collections.Generic;
using System.Text;

namespace Apex.PipeCompressors {

    internal static class Globals {

        /// <summary>
        /// The maximum size of any byte array to be stockalloc'd.
        /// </summary>
        internal const int MaxStackAllockSize = 1024;

        /// <summary>
        /// UTF8 encoder that does not emit the utf8 byte order marker identifier at the begininning of the bytes.
        /// Use this encoding for slightly shorter byte encoding.
        /// </summary>
        internal static readonly Encoding UTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    }
}
