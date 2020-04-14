using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using static Apex.PipeCompressors.Globals;

namespace Apex.PipeCompressors {

    public static class GuidCompression {

        public static void WriteGuid(this PipeWriter writer, Guid value) {
            var span = writer.GetSpan(16);
            value.TryWriteBytes(span);
            writer.Advance(16);
        }

        public static ValueTask<Guid> ReadGuid(this PipeReader reader) {
            var readTask = reader.ReadAsync(minBufferLength: 16);
            if (readTask.IsCompleted) {
                var readResult = readTask.Result;
                var buffer = readResult.Buffer;
                return new ValueTask<Guid>(ReadGuid(reader, buffer));
            } else {
                return ReadGuidAsync(reader, readTask);
            }

            static async ValueTask<Guid> ReadGuidAsync(PipeReader reader, ValueTask<ReadResult> readTask) {
                var readResult = await readTask.ConfigureAwait(false);
                var buffer = readResult.Buffer;
                return ReadGuid(reader, buffer);
            }

            static Guid ReadGuid(PipeReader reader, ReadOnlySequence<byte> buffer) {
                Span<byte> bytes = stackalloc byte[16];
                buffer.Slice(0, 16).CopyTo(bytes);
                reader.AdvanceTo(buffer.GetPosition(16));
                return new Guid(bytes);
            }
        }

        public static void WriteNullableGuid(this PipeWriter writer, Guid? value) {
            if (value is null) {
                writer.WriteBool(false);
            } else {
                writer.WriteBool(true);
                writer.WriteGuid(value.Value);
            }
        }

        public static async ValueTask<Guid?> ReadNullableGuid(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadGuid().ConfigureAwait(false);
        }
    }
}
