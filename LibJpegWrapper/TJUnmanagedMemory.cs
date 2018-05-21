using System;
using System.Runtime.InteropServices;

namespace TurboJpegWrapper
{
    /// <summary>
    /// This class wraps a block of unmanaged memory allocated using <see cref="TurboJpegImport.tjAlloc(int)"/>
    /// ensuring that the memory is freed using <see cref="TurboJpegImport.tjFree(IntPtr)"/> when it is manually
    /// disposed, goes out of context or is garbage collected. An instance of this class can be implicitly converted
    /// to either an <see cref="IntPtr"/> or a <see cref="byte[]"/>.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class TJUnmanagedMemory : IDisposable
    {
        private bool _isDisposed;
        private IntPtr _buffer;
        private ulong _size;

        /// <summary>
        /// Initializes a new instance of the <see cref="TJUnmanagedMemory"/> class and allocates the 
        /// requested amount of unmanaged memory using <see cref="TurboJpegImport.tjAlloc(int)"/>.
        /// </summary>
        /// <param name="size">The number of bytes of unmanaged memory to allocate.</param>
        public TJUnmanagedMemory(int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));
            _buffer = TurboJpegImport.tjAlloc(size);
            if (_buffer == IntPtr.Zero)
                throw new OutOfMemoryException("Could not allocate memory for buffer");
            _size = (ulong)size;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TJUnmanagedMemory"/> class without allocating
        /// new memory.
        /// </summary>
        /// <param name="buffer">The buffer to wrap (must be allocated using <see cref="TurboJpegImport.tjAlloc(int)"/> or returned from a native TJ method).</param>
        /// <param name="size">The size of the buffer (in bytes).</param>
        internal TJUnmanagedMemory(IntPtr buffer, ulong size)
        {
            _buffer = buffer;
            _size = size;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TJUnmanagedMemory"/> class, allocating memory for the bytes
        /// int the providied array and copying them to the unmanaged memory using <see cref="Marshal.Copy(byte[], int, IntPtr, int)"/>.
        /// </summary>
        /// <param name="buffer">The buffer which to convert to unmanaged memory.</param>
        internal TJUnmanagedMemory(byte[] buffer)
            :this(buffer.Length)
        {
            Marshal.Copy(buffer, 0, _buffer, (int)_size);
        }

        /// <summary>
        /// Gets the size of the allocated native memory block (in bytes).
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int Size { get { return (int)_size; } }

        /// <summary>
        /// Performs an implicit conversion from <see cref="TJUnmanagedMemory"/> to <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="source">The source object to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        /// <exception cref="ArgumentNullException">source</exception>
        /// <exception cref="ObjectDisposedException">this</exception>
        public static implicit operator IntPtr(TJUnmanagedMemory source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (source._isDisposed)
                throw new ObjectDisposedException("this");
            return source._buffer;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="TJUnmanagedMemory"/> to <see cref="byte[]" />.
        /// </summary>
        /// <param name="source">The source object to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        /// <exception cref="ArgumentNullException">source</exception>
        /// <exception cref="ObjectDisposedException">this</exception>
        public static implicit operator byte[](TJUnmanagedMemory source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (source._isDisposed)
                throw new ObjectDisposedException("this");
            var buffer = new byte[source._size];
            Marshal.Copy(source._buffer, buffer, 0, (int)source._size);
            return buffer;
        }

        #region IDisposable Support
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (_buffer != IntPtr.Zero)
                    TurboJpegImport.tjFree(_buffer);
                _isDisposed = true;
                _buffer = IntPtr.Zero;
                _size = 0;
            }
        }

        ~TJUnmanagedMemory()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
