using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Apex.PipeEncoding {

    internal static class PipeReaderExtensions {

        /// <summary>
        /// Reads from the given <paramref name="reader"/>, ensuring that <paramref name="minBufferLength"/> bytes
        /// are available in the buffer before the method returns.
        /// This extension method is made internal because it can have some dangerous side-effects on the reader
        /// if the caller doesn't understand exactly what's happening and why.
        /// Dear coder, here is the background information you should have before you consider modifying this method,
        /// or even using it at all. 
        /// 
        /// 1. Every call to the "reader.ReadAsync" method expects to have a corresponding "reader.AdvanceTo" call
        ///    made directly afterward. If you don't do this, the PipeReader will sometimes give unexpected results.
        /// 
        /// 2. If you call "reader.AdvanceTo", supplying positions that are less than the end of the current buffer,
        ///    the next call to "reader.ReadAsync" will immediately return, with NO extra data.
        ///    Since this method works by repeatedly calling "reader.ReadAsync" until all the required data is
        ///    available, this method needs to make sure that it calls "reader.AdvanceTo(buffer.Start, buffer.End)"
        ///    after every read that yields insufficient data. In this way, we can ensure that calls to 
        ///    "reader.ReadAsync" only return when more data is available.
        /// 
        /// 3. After calling this method, you still need to call "reader.AdvanceTo" one last time.
        ///    Since the reader has already had the "observed" value advanced to "minBufferLength", 
        ///    you need to make sure that after calling this method, you call "reader.AdvanceTo" with
        ///    an observed position greater than or equal to "minBufferLength".
        ///    If you don't, you will cause the reader to behave in unexpected "magical" ways.
        /// </summary>
        internal static ValueTask<ReadResult> ReadAsync(this PipeReader reader, int minBufferLength) {
            var readTask = reader.ReadAsync(default);
            if (readTask.IsCompleted) {
                var readResult = readTask.Result; /// Can only get the Result property once for a ValueTask.
                if (readResult.IsCanceled) throw new OperationCanceledException();
                var buffer = readResult.Buffer;
                if (buffer.Length >= minBufferLength) {
                    return new ValueTask<ReadResult>(readResult);
                }
                reader.AdvanceTo(buffer.Start, buffer.End);
                return SlowReadAsync(reader, minBufferLength, reader.ReadAsync(default));
            }

            return SlowReadAsync(reader, minBufferLength, readTask);


            static async ValueTask<ReadResult> SlowReadAsync(PipeReader reader, int minBufferLength, ValueTask<ReadResult> readTask) {
                var readResult = await readTask.ConfigureAwait(false);
                var buffer = readResult.Buffer;
                while (buffer.Length < minBufferLength) {
                    if (readResult.IsCompleted) throw new IOException("Reader is completed.");
                    reader.AdvanceTo(buffer.Start, buffer.End);
                    readResult = await reader.ReadAsync(default).ConfigureAwait(false);
                    if (readResult.IsCanceled) throw new OperationCanceledException();
                    buffer = readResult.Buffer;
                }
                return readResult;
            }
        }
    }
}
