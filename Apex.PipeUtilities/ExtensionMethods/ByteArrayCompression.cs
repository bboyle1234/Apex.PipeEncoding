using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Apex.PipeCompressors {

    public static class ByteArrayCompression {

        public static void WriteByteArray(this PipeWriter writer, byte[] bytes) {
            if (bytes is null) {
                writer.WriteBool(false);
            } else {
                writer.WriteBool(true);
                writer.WriteUInt((uint)bytes.Length);
                bytes.AsSpan().CopyTo(writer.GetSpan(bytes.Length));
                writer.Advance(bytes.Length);
            }
        }

        public static async ValueTask<byte[]> ReadByteArray(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            var length = (int)await reader.ReadUInt().ConfigureAwait(false);
            var bytes = new byte[length];
            var readResult = await reader.ReadAsync(minBufferLength: length).ConfigureAwait(false);
            var buffer = readResult.Buffer;
            buffer.Slice(0, length).CopyTo(bytes);
            reader.AdvanceTo(buffer.GetPosition(length));
            return bytes;
        }
    }
}
