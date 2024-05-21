using System;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;

namespace WrenSharp.Internal
{
    internal unsafe struct Utf8StringBuilder : IDisposable
    {
        private static readonly Encoding Encoding = Encoding.UTF8;

        #region Static

        public static Utf8StringBuilder Create(int initialCapacityBytes)
        {
            return new Utf8StringBuilder()
            {
                m_Buffer = ArrayPool<byte>.Shared.Rent(initialCapacityBytes),
                m_Length = 0
            };
        }

        #endregion

        private const int MinCapacity = 64;
        private const int MaxCharByteCount = 4;

        private byte[] m_Buffer;
        private int m_Length;

        #region Properties

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => m_Length;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLength(value);
        }

        #endregion

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(m_Buffer);
            m_Buffer = default;
            m_Length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan() => m_Buffer.AsSpan(0, m_Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Memory<byte> AsMemory() => m_Buffer.AsMemory(0, m_Length);

        public void Clear()
        {
            m_Length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(bool value)
        {
            if (!Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out int byteCount))
            {
                EnsureCapacity(m_Length + byteCount);
                Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out byteCount);
            }

            m_Length += byteCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(byte value)
        {
            if (!Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out int byteCount))
            {
                EnsureCapacity(m_Length + byteCount);
                Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out byteCount);
            }

            m_Length += byteCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(sbyte value)
        {
            if (!Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out int byteCount))
            {
                EnsureCapacity(m_Length + byteCount);
                Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out byteCount);
            }

            m_Length += byteCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(short value)
        {
            if (!Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out int byteCount))
            {
                EnsureCapacity(m_Length + byteCount);
                Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out byteCount);
            }

            m_Length += byteCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(ushort value)
        {
            if (!Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out int byteCount))
            {
                EnsureCapacity(m_Length + byteCount);
                Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out byteCount);
            }

            m_Length += byteCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(int value)
        {
            if (!Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out int byteCount))
            {
                EnsureCapacity(m_Length + byteCount);
                Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out byteCount);
            }

            m_Length += byteCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(uint value)
        {
            if (!Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out int byteCount))
            {
                EnsureCapacity(m_Length + byteCount);
                Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out byteCount);
            }

            m_Length += byteCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(long value)
        {
            if (!Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out int byteCount))
            {
                EnsureCapacity(m_Length + byteCount);
                Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out byteCount);
            }

            m_Length += byteCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(ulong value)
        {
            if (!Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out int byteCount))
            {
                EnsureCapacity(m_Length + byteCount);
                Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out byteCount);
            }

            m_Length += byteCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(float value)
        {
            if (!Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out int byteCount))
            {
                EnsureCapacity(m_Length + byteCount);
                Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out byteCount);
            }

            m_Length += byteCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(double value)
        {
            if (!Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out int byteCount))
            {
                EnsureCapacity(m_Length + byteCount);
                Utf8Formatter.TryFormat(value, m_Buffer.AsSpan(m_Length), out byteCount);
            }

            m_Length += byteCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char c)
        {
            byte* bytes = stackalloc byte[sizeof(char)];
            int size = Encoding.GetBytes(&c, 1, bytes, sizeof(char));
            Append(new ReadOnlySpan<byte>(bytes, size));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char* chars, int charCount) => Append(new ReadOnlySpan<char>(chars, charCount));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(string str) => Append(str.AsSpan());

        public void Append(ReadOnlySpan<char> chars)
        {
            int maxByteCount = MaxCharByteCount * chars.Length;
            if (maxByteCount < 256)
            {
                // Fast path via stackalloc for small sequences
                Span<byte> bytes = stackalloc byte[maxByteCount];
                int size = Encoding.GetBytes(chars, bytes);
                Append(bytes.Slice(0, size));
            }
            else
            {
                byte[] tmp = ArrayPool<byte>.Shared.Rent(maxByteCount);
                try
                {
                    int size = Encoding.GetBytes(chars, tmp);
                    Append(tmp.AsSpan(0, size));
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(tmp);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(ReadOnlySpan<byte> bytes) => InternalAppend(bytes);

        public void Append(StringBuilder sb)
        {
            if (sb == null || sb.Length == 0)
                return;

            const int BlockSize = 32;

            Span<char> tmp = stackalloc char[BlockSize];
            for (int i = 0; i < sb.Length; i += BlockSize)
            {
                int charCount = Math.Min(BlockSize, sb.Length - i);
                sb.CopyTo(i, tmp, charCount);
                
                Append(tmp.Slice(0, charCount));
            }
        }

        public void SetLength(int length)
        {
            if (length < 0 || length >= int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(length));

            EnsureCapacity(length, zeroMemory: true);
            m_Length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalAppend(ReadOnlySpan<byte> bytes)
        {
            EnsureCapacity(m_Length + bytes.Length);
            bytes.CopyTo(m_Buffer.AsSpan(m_Length));
            m_Length += bytes.Length;
        }

        #region Object

        public readonly override string ToString() => Encoding.GetString(m_Buffer, 0, m_Length);

        #endregion

        private void EnsureCapacity(int capacity, bool zeroMemory = false)
        {
            if (capacity <= m_Buffer.Length)
                return;

            int newCapacity = m_Buffer.Length * 2;
            if (newCapacity < capacity)
            {
                newCapacity = capacity;
            }

            byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newCapacity);
            Buffer.BlockCopy(m_Buffer, 0, newBuffer, 0, m_Length);

            if (zeroMemory)
            {
                Array.Clear(m_Buffer, m_Length, newCapacity);
            }

            ArrayPool<byte>.Shared.Return(m_Buffer);
            m_Buffer = newBuffer;
        }
    }
}
