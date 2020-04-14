using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Apex.PipeCompressors {

    public static class EnumCompression {

        public static void WriteEnum<T>(this PipeWriter writer, T value) where T : struct, Enum
            => writer.WriteInt((int)(object)value);

        public static ValueTask<T> ReadEnum<T>(this PipeReader reader) where T : struct, Enum {
            var readTask = reader.ReadInt();

            if (readTask.IsCompleted)
                return new ValueTask<T>((T)(object)readTask.Result);

            return ReadAsync(readTask);

            static async ValueTask<T> ReadAsync(ValueTask<int> readTask) {
                return (T)(object)await readTask.ConfigureAwait(false);
            }
        }

        public static void WriteNullableEnum<T>(this PipeWriter writer, T? value) where T : struct, Enum {
            if (value.HasValue) {
                writer.WriteBool(true);
                writer.WriteEnum<T>(value.Value);
            } else {
                writer.WriteBool(false);
            }
        }

        public static async ValueTask<T?> ReadNullableEnum<T>(this PipeReader reader) where T : struct, Enum {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadEnum<T>().ConfigureAwait(false);
        }
    }
}
