using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Apex.PipeEncoding {

    public static class DateTimeFormatter {

        public static void WriteDateTime(this PipeWriter writer, DateTime value) {
            writer.WriteLong(value.Ticks);
            writer.WriteEnum(value.Kind);
        }

        public static async ValueTask<DateTime> ReadDateTime(this PipeReader reader) {
            var ticks = await reader.ReadLong().ConfigureAwait(false);
            var kind = await reader.ReadEnum<DateTimeKind>().ConfigureAwait(false);
            return new DateTime(ticks, kind);
        }

        public static void WriteNullableDateTime(this PipeWriter writer, DateTime? value) {
            if (value.HasValue) {
                writer.WriteBool(true);
                writer.WriteDateTime(value.Value);
            } else {
                writer.WriteBool(false);
            }
        }

        public static async ValueTask<DateTime?> ReadNullableDateTime(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadDateTime().ConfigureAwait(false);
        }
    }
}
