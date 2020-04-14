using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Apex.PipeCompressors {

    public static class IntCompression {

        public static void WriteInt(this PipeWriter writer, int value)
            => writer.WriteUInt(Convert(value));

        public static ValueTask<int> ReadInt(this PipeReader reader) {
            var readTask = reader.ReadUInt();

            if (readTask.IsCompleted)
                return new ValueTask<int>(Convert(readTask.Result));

            return ReadAsync(readTask);

            static async ValueTask<int> ReadAsync(ValueTask<uint> readTask) {
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
        static uint Convert(int value) => (uint)((value << 1) ^ (value >> 31));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int Convert(uint value) => (int)((value >> 1) ^ (~(value & 1) + 1));


        public static void WriteNullableInt(this PipeWriter writer, int? value) {
            if (value.HasValue) {
                writer.WriteBool(true);
                writer.WriteInt(value.Value);
            } else {
                writer.WriteBool(false);
            }
        }

        public static async ValueTask<int?> ReadNullableInt(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadInt().ConfigureAwait(false);
        }
    }
}
