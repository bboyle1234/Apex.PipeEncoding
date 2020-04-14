using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Apex.PipeEncoding {

    public static class DoubleFormatter {

        public static void WriteDouble(this PipeWriter writer, double value) {
            var span = writer.GetSpan(8);
            BitConverter.TryWriteBytes(span, value);
            writer.Advance(8);
        }

        public static ValueTask<double> ReadDouble(this PipeReader reader) {
            var readTask = reader.ReadAsync(minBufferLength: 8);
            if (readTask.IsCompleted) {
                var buffer = readTask.Result.Buffer;
                return new ValueTask<double>(ReadFromBuffer(reader, buffer));
            }

            return ReadAsync(reader, readTask);

            static async ValueTask<double> ReadAsync(PipeReader reader, ValueTask<ReadResult> readTask) {
                var buffer = (await readTask.ConfigureAwait(false)).Buffer;
                return ReadFromBuffer(reader, buffer);
            }

            static double ReadFromBuffer(PipeReader reader, ReadOnlySequence<byte> buffer) {
                Span<byte> bytes = stackalloc byte[8];
                buffer.Slice(0, 8).CopyTo(bytes);
                reader.AdvanceTo(buffer.GetPosition(8));
                return BitConverter.ToDouble(bytes);
            }
        }

        public static void WriteNullableDouble(this PipeWriter writer, double? value) {
            if (value.HasValue) {
                writer.WriteBool(true);
                writer.WriteDouble(value.Value);
            } else {
                writer.WriteBool(false);
            }
        }

        public static async ValueTask<double?> ReadNullableDouble(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadDouble().ConfigureAwait(false);
        }
    }
}
