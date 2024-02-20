using System;
using System.Runtime.CompilerServices;
using System.Text;
using WrenSharp.Memory;

namespace WrenSharp.Native
{
    internal static class WrenInternal
    {
        public unsafe static string WrenStringBytesToString(byte* bytes, int byteCount)
        {
            // FIXME: Wren is not returning UTF8 code points from the slot API, even though it accepts them. 
            // So just using ASCII encoding for now, as that's all we can do without modifying the Wren source.
            var encoding = Encoding.ASCII;
            return encoding.GetString(bytes, byteCount);
        }

        public unsafe static ReadOnlySpan<byte> GetSlotStringBytes(IntPtr vm, int slot)
        {
            int length;
            var ptr = (byte*)Wren.GetSlotBytes(vm, slot, (IntPtr)(&length));
            return new ReadOnlySpan<byte>(ptr, length);
        }

        public unsafe static string GetSlotString(IntPtr vm, int slot)
        {
            int length;
            var bytes = (byte*)Wren.GetSlotBytes(vm, slot, (IntPtr)(&length));
            return WrenStringBytesToString(bytes, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void SetSlotString(IntPtr vm, int slot, ReadOnlySpan<byte> bytes)
        {
            fixed (byte* bytePtr = bytes)
            {
                Wren.SetSlotBytes(vm, slot, (IntPtr)bytePtr, (ulong)bytes.Length);
            }
        }

        public unsafe static void EncodeTextBufferFromStringBuilder(
            StringBuilder stringBuilder, Encoding encoding, IAllocator allocator,
            ref byte* buffer, ref int bufferSize,
            out int charCount, out int byteCount)
        {
            charCount = stringBuilder.Length;

            // We don't know how big the buffer needs to be until the chars are encoded,
            // so need to allocate the maximum number of bytes required to store a string
            // of n length in the specified encoding + 1 for the null terminator
            int requiredBufferSize = encoding.GetMaxByteCount(charCount) + 1;

            // Free existing buffer if it isn't large enough to hold the maximum possible encoded size
            if (requiredBufferSize > bufferSize && buffer != null)
            {
                allocator.Free((IntPtr)buffer);
                buffer = null;
            }

            // Allocate a new buffer if required
            if (buffer == null)
            {
                buffer = (byte*)allocator.Allocate(requiredBufferSize);
                bufferSize = requiredBufferSize;

                if (buffer == null)
                {
                    // Buffer couldn't be allocated
                    bufferSize = 0;
                    byteCount = 0;
                    return;
                }
            }

            var charSpan = new Span<char>((char*)buffer, charCount);
            var byteSpan = new Span<byte>(buffer, bufferSize);

            // Copy chars from the builder into the buffer
            stringBuilder.CopyTo(0, charSpan, charCount);

            // Encode chars in buffer
            // Note that this is reading and writing to the same buffer, this should be ok since
            // C# chars are two bytes wide, so the read cursor will always be ahead of the write cursor.
            byteCount = encoding.GetBytes(charSpan, byteSpan);

            // Add the null terminator
            buffer[byteCount++] = (byte)'\0';
        }

        public unsafe static void EncodeTextBufferFromString(
            ReadOnlySpan<char> str, Encoding encoding, IAllocator allocator,
            ref byte* buffer, ref int bufferSize,
            out int charCount, out int byteCount)
        {
            charCount = str.Length;

            // We don't know how big the buffer needs to be until the chars are encoded,
            // so need to allocate the maximum number of bytes required to store a string
            // of n length in the specified encoding + 1 for the null terminator
            int requiredBufferSize = encoding.GetMaxByteCount(charCount) + 1;

            // Free existing buffer if it isn't large enough to hold the maximum possible encoded size
            if (requiredBufferSize > bufferSize && buffer != null)
            {
                allocator.Free((IntPtr)buffer);
                buffer = null;
            }

            // Allocate a new buffer if required
            if (buffer == null)
            {
                buffer = (byte*)allocator.Allocate(requiredBufferSize);
                bufferSize = requiredBufferSize;

                if (buffer == null)
                {
                    // Buffer couldn't be allocated
                    bufferSize = 0;
                    byteCount = 0;
                    return;
                }
            }

            byteCount = encoding.GetBytes(str, new Span<byte>(buffer, bufferSize));

            // Add the null terminator
            buffer[byteCount++] = (byte)'\0';
        }
    }
}
