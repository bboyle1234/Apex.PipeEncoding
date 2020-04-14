using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Apex.PipeEncoding {

    public static class ULongFormatter {

        const uint DATA_MASK = 0b_0111_1111;
        const uint LAST_BYTE = 0b_1000_0000;

        public static void WriteULong(this PipeWriter writer, ulong value) {
            var bytesWritten = 0;
            var span = writer.GetSpan(9);
            while (value > DATA_MASK) {
                span[bytesWritten++] = (byte)(value & DATA_MASK);
                value >>= 7;
            }
            span[bytesWritten++] = (byte)(value | LAST_BYTE);
            writer.Advance(bytesWritten);
        }

        public static ValueTask<ulong> ReadULong(this PipeReader reader) {
            ulong result = 0;
            var shiftBits = 0;
            while (true) {
                var readTask = reader.ReadAsync(default);
                if (readTask.IsCompleted) {
                    if (ProcessResult(reader, readTask.Result, ref result, ref shiftBits)) {
                        return new ValueTask<ulong>(result);
                    }
                } else {
                    return ReadAsync(reader, readTask, result, shiftBits);
                }
            }

            static async ValueTask<ulong> ReadAsync(PipeReader reader, ValueTask<ReadResult> readTask, ulong result, int shiftBits) {
                var readResult = await readTask.ConfigureAwait(false);
                if (ProcessResult(reader, readResult, ref result, ref shiftBits)) {
                    return result;
                } else {
                    return result + ((await reader.ReadULong().ConfigureAwait(false)) << shiftBits);
                }
            }

            static bool ProcessResult(PipeReader reader, ReadResult readResult, ref ulong result, ref int shiftBits) {
                var bytesRead = 0;
                if (readResult.IsCanceled) throw new OperationCanceledException();
                var buffer = readResult.Buffer;
                if (buffer.Length == 0) throw new IOException("Buffer was empty.");
                foreach (var segment in buffer) {
                    var span = segment.Span;
                    for (var i = 0; i < span.Length; i++) {
                        bytesRead++;
                        var inputByte = (uint)span[i];
                        if ((inputByte & LAST_BYTE) == LAST_BYTE) {
                            reader.AdvanceTo(buffer.GetPosition(bytesRead));
                            result += ((inputByte & DATA_MASK) << shiftBits);
                            return true;
                        } else {
                            result += inputByte << shiftBits;
                            shiftBits += 7;
                        }
                    }
                }
                reader.AdvanceTo(buffer.GetPosition(bytesRead));
                return false;
            }
        }

        public static void WriteNullableULong(this PipeWriter writer, ulong? value) {
            if (value.HasValue) {
                writer.WriteBool(true);
                writer.WriteULong(value.Value);
            } else {
                writer.WriteBool(false);
            }
        }

        public static async ValueTask<ulong?> ReadNullableULong(this PipeReader reader) {
            if (!await reader.ReadBool().ConfigureAwait(false)) return null;
            return await reader.ReadULong().ConfigureAwait(false);
        }
    }
}
