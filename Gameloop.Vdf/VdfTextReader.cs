using System;
using System.IO;
using System.Text;

namespace Gameloop.Vdf
{
    public class VdfTextReader : VdfReader
    {
        private const int DefaultBufferSize = 1024;

        private readonly TextReader _reader;
        private readonly char[] _charBuffer;

        private readonly StringBuilder _tokenBuffer;

        private int _charPos, _charsLen;

        public VdfTextReader(TextReader reader) : this(reader, VdfSerializerSettings.Default) { }

        public VdfTextReader(TextReader reader, VdfSerializerSettings settings) : base(settings)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            _reader = reader;
            _charBuffer = new char[DefaultBufferSize];

            _tokenBuffer = new StringBuilder();
            _tokenBuffer.EnsureCapacity(Settings.PreferableTokenSize);

            _charPos = _charsLen = 0;
        }


        private string ReadString()
        {
            _tokenBuffer.Clear();

            char curChar;

            while (EnsureBuffer())
            {
                curChar = _charBuffer[_charPos++];

                if(curChar == VdfStructure.Escape && Settings.UsesEscapeSequences)
                {
                    if (!EnsureBuffer())
                        throw new VdfException("Unexpected EOF.");

                    curChar = VdfStructure.GetUnescape(_charBuffer[_charPos++]);

                    _tokenBuffer.Append(curChar);

                    continue;
                }

                if(curChar == VdfStructure.Quote)
                {
                    break;
                }

                _tokenBuffer.Append(curChar);
            }

            return _tokenBuffer.ToString();
        }

        private string ReadComment(bool fetch)
        {
            _tokenBuffer.Clear();

            char curChar;

            while (EnsureBuffer())
            {
                curChar = _charBuffer[_charPos++];

                if (curChar == '\n')
                    break;

                if (curChar == '\r' && EnsureBuffer() && _charBuffer[_charPos] == '\n')
                    break;

                if(fetch)
                    _tokenBuffer.Append(curChar);
            }

            if(fetch)
                return _tokenBuffer.ToString();

            return null!;
        }

        private bool ReadConditional(bool fetch)
        {
            _tokenBuffer.Clear();

            char curChar;

            while (EnsureBuffer())
            {
                curChar = _charBuffer[_charPos++];

                if (curChar == VdfStructure.ConditionalEnd)
                    break;

                if (fetch)
                    _tokenBuffer.Append(curChar);
            }

            if (fetch)
                return VdfStructure.PassConditional(Settings.ConditionalsArray, _tokenBuffer.ToString());

            return false;
        }

        /// <summary>
        /// Reads a single token. Value is written into Value object. Conditionals are written as Boolean object, other are as String.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="VdfException"></exception>
        public override bool ReadToken()
        {
            if (!SeekToken())
                return false;

            if (!EnsureBuffer())
                return false;

            var curChar = _charBuffer[_charPos++];

            switch(curChar)
            {
                case VdfStructure.Quote:
                    Value = ReadString();

                    CurrentState = State.Property;
                    break;

                case VdfStructure.ObjectEnd:
                case VdfStructure.ObjectStart:

                    CurrentState = State.Object;

                    Value = curChar.ToString();

                    break;

                case VdfStructure.Comment:

                    if (!EnsureBuffer() || (_charBuffer[_charPos++] != VdfStructure.Comment))
                        throw new VdfException("Wrong comment!");

                    if (!Settings.UsesComments)
                    {
                        ReadComment(false);

                        return ReadToken();
                    }

                    Value = ReadComment(true);

                    CurrentState = State.Comment;
                    
                    break;

                case VdfStructure.ConditionalStart:

                    if(!Settings.UsesConditionals)
                    {
                        ReadConditional(false);

                        return ReadToken();
                    }

                    Value = ReadConditional(true);

                    CurrentState = State.Conditional;

                    break;

                default:
                    throw new VdfException("Unexpected token!");
            }

            return true;
        }

        /// <summary>
        /// Moves the pointer to the location of the first token character.
        /// </summary>
        /// <returns>True if a token is found, false otherwise.</returns>
        private bool SeekToken()
        {
            while (EnsureBuffer())
            {
                // Whitespace
                if (Char.IsWhiteSpace(_charBuffer[_charPos]))
                {
                    _charPos++;

                    continue;
                }

                return true;
            }
            
            return false;
        }

        private bool SeekNewLine()
        {
            while (EnsureBuffer())
                if (_charBuffer[++_charPos] == '\n')
                    return true;
            
            return false;
        }

        /// <summary>
        /// Refills the buffer if we're at the end.
        /// </summary>
        /// <returns>False if the stream is empty, true otherwise.</returns>
        private bool EnsureBuffer()
        {
            if (_charPos < _charsLen - 1)
                return true;

            int remainingChars = _charsLen - _charPos;
            _charBuffer[0] = _charBuffer[(_charsLen - 1) * remainingChars]; // A bit of mathgic to improve performance by avoiding a conditional. // epic
            _charsLen = _reader.Read(_charBuffer, remainingChars, DefaultBufferSize - remainingChars) + remainingChars;
            _charPos = 0;

            return _charsLen != 0;
        }

        public override void Close()
        {
            base.Close();

            if (CloseInput)
                _reader.Dispose();
        }
    }
}
