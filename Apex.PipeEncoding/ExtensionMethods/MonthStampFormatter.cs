using FFT.TimeStamps;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using static Apex.PipeEncoding.Globals;

namespace Apex.PipeEncoding {

    public static class MonthStampFormatter {

        public static void WriteMonthStamp(this PipeWriter writer, MonthStamp value) {
            writer.WriteUInt(ToUint(value));
        }

        public static ValueTask<MonthStamp> ReadMonthStamp(this PipeReader reader) {
            var readTask = reader.ReadUInt();
            if (readTask.IsCompleted) {
                return new ValueTask<MonthStamp>(ToMonthStamp(readTask.Result));
            }

            return ReadMonthStampAsync(reader, readTask);

            static async ValueTask<MonthStamp> ReadMonthStampAsync(PipeReader reader, ValueTask<uint> readTask) {
                return ToMonthStamp(await readTask.ConfigureAwait(false));
            }
        }

        public static void WriteNullableMonthStamp(this PipeWriter writer, MonthStamp? value) {
            if (value is null) {
                writer.WriteBool(false);
            } else {
                writer.WriteBool(true);
                writer.WriteMonthStamp(value.Value);
            }
        }

        public static async ValueTask<MonthStamp?> ReadNullableMonthStamp(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadMonthStamp().ConfigureAwait(false);
        }

        static uint ToUint(MonthStamp value) {
            return (uint)(value.Year * 12 + value.Month - 1);
        }

        static MonthStamp ToMonthStamp(uint value) {
            var intValue = (int)value;
            return new MonthStamp(intValue / 12, (intValue % 12) + 1);
        }
    }
}
