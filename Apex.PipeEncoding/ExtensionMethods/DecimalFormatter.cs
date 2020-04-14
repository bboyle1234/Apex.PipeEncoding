using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Pipelines;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static Apex.PipeEncoding.Globals;

namespace Apex.PipeEncoding {

    public static class DecimalFormatter {

        //// TODO: Implement "WriteDecimalAsTwoInts" method as well.
        //public static void WriteDecimalAsTwoInts(this PipeWriter writer, decimal value) {
        //    // step 1. Represent the value without a decimal place.
        //    //         Eg: 1000.25 becomes 100025E-2
        //    //         There needs to be a very efficient method used for achieving this.
        //    // step 2. Store 100025 as an int, and then store -2 as an int.
        //    // Disadvantage: When user declares decimal x = 1.2000, the value is stored so that the "000" on the end is preserved
        //    //               within the binary encoding of the deciml struct. 
        //    //               When this algo is used to write and read the decimal, knowledge of the precision represented by the trailing zeros is lost.
        //    //               The need for this knowledge is extremely rare, needed in scientific applications but not in financial applications, so this 
        //    //               disadvantage would rarely be an issue.
        //}

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
