using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Pipelines;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static Apex.PipeCompressors.Globals;

namespace Apex.PipeCompressors {

    public static class DecimalCompression {

        public static void WriteDecimalAsString(this PipeWriter writer, decimal value) {
            ReadOnlySpan<char> valueString = value.ToString(CultureInfo.InvariantCulture);
            var numBytes = UTF8NoBOM.GetByteCount(valueString);
            writer.WriteUInt((uint)numBytes);
            var bytes = writer.GetSpan(numBytes);
            UTF8NoBOM.GetBytes(valueString, bytes);
            writer.Advance(numBytes);
        }

        public static async ValueTask<decimal> ReadDecimalFromString(this PipeReader reader) {
            var numBytes = (int)await reader.ReadUInt().ConfigureAwait(false);
            var readResult = await reader.ReadAsync(minBufferLength: numBytes).ConfigureAwait(false);
            return ReadFromBuffer(reader, numBytes, readResult.Buffer);

            static decimal ReadFromBuffer(PipeReader reader, int numBytes, ReadOnlySequence<byte> buffer) {
                Span<byte> bytes = stackalloc byte[numBytes];
                buffer.Slice(0, numBytes).CopyTo(bytes);
                var valueString = UTF8NoBOM.GetString(bytes);
                reader.AdvanceTo(buffer.GetPosition(numBytes));
                return decimal.Parse(valueString, CultureInfo.InvariantCulture);
            }
        }

        public static void WriteNullableDecimalAsString(this PipeWriter writer, decimal? value) {
            if (value.HasValue) {
                writer.WriteBool(true);
                writer.WriteDecimalAsString(value.Value);
            } else {
                writer.WriteBool(false);
            }
        }

        public static async ValueTask<decimal?> ReadNullableDoubleFromString(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadDecimalFromString().ConfigureAwait(false);
        }
    }
}
