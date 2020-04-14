using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Apex.PipeCompressors {

    public static class TimeSpanCompression {

        public static void WriteTimeSpan(this PipeWriter writer, TimeSpan value) {
            writer.WriteLong(value.Ticks);
        }

        public static async ValueTask<TimeSpan> ReadTimeSpan(this PipeReader reader) {
            var ticks = await reader.ReadLong().ConfigureAwait(false);
            return new TimeSpan(ticks);
        }

        public static void WriteNullableTimeSpan(this PipeWriter writer, TimeSpan? value) {
            if (value.HasValue) {
                writer.WriteBool(true);
                writer.WriteTimeSpan(value.Value);
            } else {
                writer.WriteBool(false);
            }
        }

        public static async ValueTask<TimeSpan?> ReadNullableTimeSpan(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadTimeSpan().ConfigureAwait(false);
        }
    }
}
