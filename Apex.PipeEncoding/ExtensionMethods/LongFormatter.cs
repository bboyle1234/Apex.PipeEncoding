using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Apex.PipeEncoding {

    public static class LongFormatter {

        public static void WriteLong(this PipeWriter writer, long value)
            => writer.WriteULong(Convert(value));

        public static ValueTask<long> ReadLong(this PipeReader reader) {
            var readTask = reader.ReadULong();

            if (readTask.IsCompleted)
                return new ValueTask<long>(Convert(readTask.Result));

            return ReadAsync(readTask);

            static async ValueTask<long> ReadAsync(ValueTask<ulong> readTask) {
                return Convert(await readTask.ConfigureAwait(false));
            }
        }

        /*************************************************************************************************
        * Uses Zig-Zag encoding to achieve small compression for negative as well as positive numbers.
        * See: 
        *   https://developers.google.com/protocol-buffers/docs/encoding#signed-integers
        *   https://stackoverflow.com/questions/2210923/zig-zag-decoding
        *************************************************************************************************/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ulong Convert(long value) => (uint)((value << 1) ^ (value >> 63));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static long Convert(ulong value) => (int)((value >> 1) ^ (~(value & 1) + 1));


        public static void WriteNullableLong(this PipeWriter writer, long? value) {
            if (value.HasValue) {
                writer.WriteBool(true);
                writer.WriteLong(value.Value);
            } else {
                writer.WriteBool(false);
            }
        }

        public static async ValueTask<long?> ReadNullableLong(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadLong().ConfigureAwait(false);
        }
    }
}
