using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using static Apex.PipeEncoding.Globals;

namespace Apex.PipeEncoding {

    public static class StringFormatter {

        public static void WriteString(this PipeWriter writer, string value) {
            if (value is null) {
                writer.WriteBool(false);
            } else {
                writer.WriteBool(true);
                if (value == string.Empty) {
                    writer.WriteUInt(0);
                } else {
                    var byteCount = UTF8NoBOM.GetByteCount(value);
                    writer.WriteUInt((uint)byteCount);
                    UTF8NoBOM.GetBytes(value, writer.GetSpan(byteCount));
                    writer.Advance(byteCount);
                }
            }
        }

        public static async ValueTask<string> ReadString(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            var byteCount = (int)await reader.ReadUInt().ConfigureAwait(false);
            if (byteCount == 0) return string.Empty;
            var readResult = await reader.ReadMinLengthAsync(minBufferLength: byteCount).ConfigureAwait(false);
            var buffer = readResult.Buffer;
            var result = ReadString(buffer, byteCount);
            reader.AdvanceTo(buffer.GetPosition(byteCount));
            return result;

            static string ReadString(ReadOnlySequence<byte> buffer, int byteCount) {
                if (buffer.First.Length >= byteCount) {
                    return UTF8NoBOM.GetString(buffer.First.Span.Slice(0, byteCount));
                }
                Span<byte> bytes = byteCount <= MaxStackAllockSize ? stackalloc byte[byteCount] : new byte[byteCount];
                buffer.Slice(0, byteCount).CopyTo(bytes);
                return UTF8NoBOM.GetString(bytes);
            }
        }
    }
}
