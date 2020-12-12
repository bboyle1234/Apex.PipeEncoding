using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Apex.PipeEncoding {

    public static class PipeReaderExtensions {

        /// <summary>
        /// Reads from the given <paramref name="reader"/>, ensuring that <paramref name="minBufferLength"/> bytes
        /// are available in the buffer before the method returns.
        /// </summary>
        /// <exception cref="OperationCanceledException">Thrown when the <paramref name="cancellationToken"/> is cancelled before the read operation is completed.</exception>
        /// <exception cref="IOException">Thrown when the reader completes before the <paramref name="minBufferLength"/> is achieved.</exception>
        public static ValueTask<ReadResult> ReadMinLengthAsync(this PipeReader reader, int minBufferLength, CancellationToken cancellationToken = default) {

            // try the synchronous completion case first, for performance reasons
            var readTask = reader.ReadAsync(cancellationToken);
            if (readTask.IsCompleted) {
                var readResult = readTask.Result; /// Can only get the Result property once for a ValueTask.
                if (readResult.IsCanceled) throw new OperationCanceledException(cancellationToken);
                var buffer = readResult.Buffer;
                if (buffer.Length >= minBufferLength) {
                    return new ValueTask<ReadResult>(readResult);
                }

                if (readResult.IsCompleted) throw new IOException("Reader is completed.");
                // fall back to asynchronous completion
                reader.AdvanceTo(buffer.Start, buffer.End); // it is required to call "AdvanceTo" before calling "ReadAsync" again
                return SlowReadAsync(reader, minBufferLength, cancellationToken, reader.ReadAsync(cancellationToken));
            }

            // fall back to asynchronous completion
            return SlowReadAsync(reader, minBufferLength, cancellationToken, readTask);


            static async ValueTask<ReadResult> SlowReadAsync(PipeReader reader, int minBufferLength, CancellationToken cancellationToken, ValueTask<ReadResult> readTask) {
                var readResult = await readTask.ConfigureAwait(false);
                var buffer = readResult.Buffer;
                while (buffer.Length < minBufferLength) {
                    if (readResult.IsCompleted) throw new IOException("Reader is completed.");
                    reader.AdvanceTo(buffer.Start, buffer.End);  // it is required to call "AdvanceTo" before calling "ReadAsync" again
                    readResult = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    if (readResult.IsCanceled) throw new OperationCanceledException(cancellationToken);
                    buffer = readResult.Buffer;
                }
                return readResult;
            }
        }
    }
}
