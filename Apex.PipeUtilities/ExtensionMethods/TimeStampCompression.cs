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

    public static class TimeStampCompression {

        public static void WriteTimeStamp(this PipeWriter writer, TimeStamp value) {
            writer.WriteULong((ulong)value.TicksUtc);
        }

        public static ValueTask<TimeStamp> ReadTimeStamp(this PipeReader reader) {
            var readTask = reader.ReadULong();
            if (readTask.IsCompleted) {
                return new ValueTask<TimeStamp>(new TimeStamp((long)readTask.Result));
            }

            return ReadTimeStampAsync(reader, readTask);

            static async ValueTask<TimeStamp> ReadTimeStampAsync(PipeReader reader, ValueTask<ulong> readTask) {
                return new TimeStamp((long)await readTask.ConfigureAwait(false));
            }
        }

        public static void WriteNullableTimeStamp(this PipeWriter writer, TimeStamp? value) {
            if (value is null) {
                writer.WriteBool(false);
            } else {
                writer.WriteBool(true);
                writer.WriteTimeStamp(value.Value);
            }
        }

        public static async ValueTask<TimeStamp?> ReadNullableTimeStamp(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadTimeStamp().ConfigureAwait(false);
        }
    }
}
