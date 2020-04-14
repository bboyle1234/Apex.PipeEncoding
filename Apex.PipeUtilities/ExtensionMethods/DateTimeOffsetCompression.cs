using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Apex.PipeCompressors {

    public static class DateTimeOffsetCompression {

        public static void WriteDateTimeOffset(this PipeWriter writer, DateTimeOffset value) {
            writer.WriteLong(value.Ticks);
            writer.WriteLong(value.Offset.Ticks);
        }

        public static async ValueTask<DateTimeOffset> ReadDateTimeOffset(this PipeReader reader) {
            var ticks = await reader.ReadLong().ConfigureAwait(false);
            var offsetTicks = await reader.ReadLong().ConfigureAwait(false);
            return new DateTimeOffset(ticks, new TimeSpan(offsetTicks));
        }

        public static void WriteNullableDateTimeOffset(this PipeWriter writer, DateTimeOffset? value) {
            if (value.HasValue) {
                writer.WriteBool(true);
                writer.WriteDateTimeOffset(value.Value);
            } else {
                writer.WriteBool(false);
            }
        }

        public static async ValueTask<DateTimeOffset?> ReadNullableDateTimeOffset(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadDateTimeOffset().ConfigureAwait(false);
        }
    }
}
