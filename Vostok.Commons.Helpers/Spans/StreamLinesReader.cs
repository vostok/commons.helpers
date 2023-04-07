
using JetBrains.Annotations;
#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Buffers;
using System.IO;
using System.Text.Unicode;
#endif

namespace Vostok.Commons.Helpers.Spans;

#if NETCOREAPP3_1_OR_GREATER
/// <summary>
/// string reader with zero garbage
/// TryReadLine call invalidates content of previously returned lines (ReadOnlySpan's)!!
/// allocates internal char[] buffer. is's size not greater than (maximum line length + 20%)
/// uses UTF-8 encoding for char conversion
/// line delimiters are only "\n" or "\r" or "\r\n" (exactly same as StreamReader)
/// </summary>
[PublicAPI]
internal class StreamLinesReader : IDisposable
{
    private const double growFactor = 1.2;
    private const int maxBytesForChar = 4;
    private readonly byte[] bytesBuffer;
    private readonly bool ownsStream;
    private readonly Stream stream;
    private int bytesBufferLength;
    private int bytesReadFromBuffer;

    private char[] charBuffer; //charBuffer contains chars from 'start' of line. cant contain more than 1 line
    private int charBufferCharsRead;
    private int charBufferLength;
    private int charBufferStringStart;
    private bool consumeN;

    private bool lastByteRead;
    private bool resizeAllowed;

    /// <summary>
    /// correct usage: byteBufferSize big, charsBufferSize not big (and will be auto resized to longest string)
    /// if byteBufferSize is small, stream reading is not effective, and chars converted via small chunks
    /// </summary>
    /// <param name="stream">source stream</param>
    /// <param name="byteBufferSize">buffer size fro stream reading.</param>
    /// <param name="charsBufferSize">initial char bufferr size for string conversion. resized up to 1.2*(max_string_length + 1)</param>
    /// <param name="ownsStream">if true, Dispose will call stream.Dispose</param>
    public StreamLinesReader(
        Stream stream,
        int byteBufferSize = 4096,
        int charsBufferSize = 100,
        bool ownsStream = true)
    {
        if (byteBufferSize < maxBytesForChar)
            byteBufferSize = maxBytesForChar; //must fit 1 char UTF8

        if (charsBufferSize <= 2)
            charsBufferSize = 2; //safe: 2-char code point should fit

        this.stream = stream;
        this.ownsStream = ownsStream;
        bytesBuffer = new byte[byteBufferSize];
        charBuffer = new char[charsBufferSize];
        ResetBuffers();
    }

    /// <summary>
    /// true if end of stream is reached.
    /// becames true after TryReadLine firstly returns true
    /// </summary>
    public bool IsEOF { get; private set; }

    public void Dispose()
    {
        if (ownsStream)
            stream?.Dispose();
    }

    /// <summary>
    /// Reset stream position to 0 and clear internal buffers
    /// </summary>
    public void ResetToZero()
    {
        stream.Position = 0;
        ResetBuffers();
    }

    /// <summary>
    /// clear internal buffers
    /// call after stream position changed externally
    /// next string reads will start from new stream position
    /// </summary>
    public void ResetBuffers()
    {
        bytesBufferLength = 0;
        bytesReadFromBuffer = 0;

        charBufferCharsRead = 0;
        charBufferLength = 0;
        charBufferStringStart = 0;
        consumeN = false;

        resizeAllowed = true;
        lastByteRead = false;
        IsEOF = false;
    }

    /// <summary>
    /// Try read next line
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryReadLine(out ReadOnlySpan<char> result)
    {
        while (!IsEOF)
        {
            ReadNextData();
            var nextStringStart = -1;

            if (IsEOF)
                if (charBufferStringStart < charBufferLength)
                    nextStringStart = charBufferLength + 1;

            for (; charBufferCharsRead < charBufferLength; charBufferCharsRead++)
            {
                var ch = charBuffer[charBufferCharsRead]; //end of line = any of "\n" or "\r" or "\r\n"
                switch (ch)
                {
                    case '\n':
                        if (consumeN)
                        {
                            consumeN = false;
                            charBufferStringStart++;
                            break;
                        }

                        consumeN = false;
                        nextStringStart = charBufferCharsRead + 1;
                        break;
                    case '\r':
                        nextStringStart = charBufferCharsRead + 1;
                        consumeN = true;
                        break;
                    default:
                        consumeN = false;
                        break;
                }

                if (nextStringStart >= 0)
                {
                    charBufferCharsRead++; //skip this char, it was read
                    break;
                }
            }

            if (nextStringStart >= 0)
            {
                resizeAllowed = false;
                var readOnlySpan = new ReadOnlySpan<char>(charBuffer,
                    charBufferStringStart,
                    nextStringStart - charBufferStringStart - 1);
                charBufferStringStart = nextStringStart;
                result = readOnlySpan;
                return true;
            }
        }

        result = ReadOnlySpan<char>.Empty;
        return false;
    }

    internal int CharBufferSize => charBuffer.Length;

    private void ReadNextData()
    {
        if (charBufferCharsRead >= charBufferLength) //check all chars are processed 
        {
            ReadNextBytes();
            AppendChars();
        }
    }

    private void AppendChars()
    {
        //note expect: charBufferCharsRead >= charBufferLength
        var srcToCopy = ReadOnlySpan<char>.Empty;
        if (resizeAllowed)
        {
            if (charBufferLength >= charBuffer.Length)
            {
                //need resize
                //do resize

                var old = charBuffer;
                var newLength =
                    Math.Max((int)(old.Length * growFactor), old.Length + 2); //new buffer should be larger.
                charBuffer = new char[newLength];
                srcToCopy = new ReadOnlySpan<char>(old, 0, charBufferLength);
            } //else - no need to resize. maybe bytesBuffer is small, convert bytes to [charBufferLength, charBuffer.Length)
        }
        else
        {
            //dont resize, shift buffer.
            //copy from [charBufferStringStart to charBufferLength)
            srcToCopy = new ReadOnlySpan<char>(charBuffer,
                charBufferStringStart,
                charBufferLength - charBufferStringStart);
            resizeAllowed = true;
            charBufferCharsRead -= charBufferStringStart;
            charBufferLength -= charBufferStringStart;
            charBufferStringStart = 0;
        }

        if (srcToCopy.Length > 0)
            srcToCopy.CopyTo(new Span<char>(charBuffer));

        var status = Utf8.ToUtf16(
            new ReadOnlySpan<byte>(bytesBuffer, bytesReadFromBuffer, bytesBufferLength - bytesReadFromBuffer),
            new Span<char>(charBuffer, charBufferLength, charBuffer.Length - charBufferLength),
            out var bytesConverted,
            out var charsConverted,
            false,
            false);

        bytesReadFromBuffer += bytesConverted;
        charBufferLength += charsConverted;

        if (status == OperationStatus.InvalidData)
            FailBadBytes();

        if (lastByteRead)
            IsEOF = status == OperationStatus.Done && charsConverted == 0; //also should be bytesConverted==0
    }

    private void ReadNextBytes()
    {
        if (bytesReadFromBuffer + maxBytesForChar <
            bytesBufferLength) //not all buffer drained, do nothing. can convert >=1 valid char
            return;
        if (bytesReadFromBuffer < bytesBufferLength) //something remains. max 2 bytes for UTF8
        {
            //copy to beginning
            new Span<byte>(bytesBuffer, bytesReadFromBuffer, bytesBufferLength - bytesReadFromBuffer).CopyTo(
                new Span<byte>(bytesBuffer));
        }

        bytesBufferLength -= bytesReadFromBuffer;
        bytesReadFromBuffer = 0; //copied bytes will be re-read while char conversion

        var bytesRead = stream.Read(bytesBuffer, bytesBufferLength, bytesBuffer.Length - bytesBufferLength);
        if (bytesRead == 0)
            lastByteRead = true;

        bytesBufferLength += bytesRead;
    }

    private void FailBadBytes()
    {
        throw new InvalidOperationException("Cant convert bytes to chars via UTF8");
    }
}
#endif