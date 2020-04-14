using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Apex.PipeEncoding {

    public static class BoolFormatter {

        public static void WriteBool(this PipeWriter writer, bool value) {
            writer.GetSpan(1)[0] = value ? (byte)1 : (byte)0;
            writer.Advance(1);
        }

        public static void WriteNullableBool(this PipeWriter writer, bool? value) {
            var span = writer.GetSpan(2);
            if (value.HasValue) {
                span[0] = (byte)1;
                span[1] = value.Value ? (byte)1 : (byte)0;
                writer.Advance(2);
            } else {
                span[0] = (byte)0;
                writer.Advance(1);
            }
        }

        public static ValueTask<bool> ReadBool(this PipeReader reader) {

            var readTask = reader.ReadAsync(default);
            if (readTask.IsCompleted) {
                return new ValueTask<bool>(GetBool(reader, readTask.Result));
            }
            return ReadBoolAsync(reader, readTask);

            static async ValueTask<bool> ReadBoolAsync(PipeReader reader, ValueTask<ReadResult> readTask) {
                var readResult = await readTask.ConfigureAwait(false);
                return GetBool(reader, readResult);
            }

            static bool GetBool(PipeReader reader, ReadResult readResult) {
                if (readResult.IsCanceled) throw new OperationCanceledException();
                var buffer = readResult.Buffer;
                if (buffer.Length == 0) throw new IOException("Buffer was empty.");
                var result = buffer.First.Span[0] != 0;
                reader.AdvanceTo(buffer.GetPosition(1));
                return result;
            }
        }

        public static async ValueTask<bool?> DecodeNullableBool(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadBool().ConfigureAwait(false);
        }
    }
}
