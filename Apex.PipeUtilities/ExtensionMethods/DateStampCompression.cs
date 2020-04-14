using Apex.TimeStamps;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using static Apex.PipeCompressors.Globals;

namespace Apex.PipeCompressors {

    public static class DateStampCompression {

        public static void WriteDateStamp(this PipeWriter writer, DateStamp value) {
            writer.WriteUInt(ToUint(value));
        }

        public static ValueTask<DateStamp> ReadDateStamp(this PipeReader reader) {
            var readTask = reader.ReadUInt();
            if (readTask.IsCompleted) {
                return new ValueTask<DateStamp>(ToDateStamp(readTask.Result));
            }

            return ReadDateStampAsync(reader, readTask);

            static async ValueTask<DateStamp> ReadDateStampAsync(PipeReader reader, ValueTask<uint> readTask) {
                return ToDateStamp(await readTask.ConfigureAwait(false));
            }
        }

        public static void WriteNullableDateStamp(this PipeWriter writer, DateStamp? value) {
            if (value is null) {
                writer.WriteBool(false);
            } else {
                writer.WriteBool(true);
                writer.WriteDateStamp(value.Value);
            }
        }

        public static async ValueTask<DateStamp?> ReadNullableDateStamp(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadDateStamp().ConfigureAwait(false);
        }

        static uint ToUint(DateStamp value) {
            return (uint)((value.Year * 12 + value.Month - 1) * 100 + value.Day);
        }

        static DateStamp ToDateStamp(uint value) {
            var intValue = (int)value;
            return new DateStamp(intValue / 100 / 12, (intValue / 100 % 12) + 1, intValue % 100);
        }
    }
}
