using System.Collections.Generic;

namespace Gameloop.Vdf
{
    public static class VdfStructure
    {

        // Format
        public const char Quote = '"', Escape = '\\', Comment = '/', Assign = ' ', Indent = '\t';
        public const char ConditionalStart = '[', ConditionalEnd = ']';
        public const char ObjectStart = '{', ObjectEnd = '}';

        // Conditionals
        public const string ConditionalXbox360 = "$X360", ConditionalWin32 = "$WIN32";
        public const string ConditionalWindows = "$WINDOWS", ConditionalOSX = "$OSX", ConditionalLinux = "$LINUX", ConditionalPosix = "$POSIX";

        private static readonly Dictionary<string, int> ConditionalsMap = new();

        // Escapes
        private const uint EscapeMapLength = 128;
        private static readonly bool[] EscapeExistsMap;
        private static readonly char[] EscapeMap, UnescapeMap;
        private static readonly char[,] EscapeConversions =
        {
            { '\n', 'n'  },
            { '\t', 't'  },
            { '\v', 'v'  },
            { '\b', 'b'  },
            { '\r', 'r'  },
            { '\f', 'f'  },
            { '\a', 'a'  },
            { '\\', '\\' },
            { '?' , '?'  },
            { '\'', '\'' },
            { '\"', '\"' }
        };

        static VdfStructure()
        {
            EscapeExistsMap = new bool[EscapeMapLength];
            EscapeMap = new char[EscapeMapLength];
            UnescapeMap = new char[EscapeMapLength];

            for (int index = 0; index < EscapeMapLength; index++)
                EscapeMap[index] = UnescapeMap[index] = (char) index;

            for (int index = 0; index < EscapeConversions.GetLength(0); index++)
            {
                char unescaped = EscapeConversions[index, 0], escaped = EscapeConversions[index, 1];

                EscapeExistsMap[unescaped] = true;
                EscapeMap[unescaped] = escaped;
                UnescapeMap[escaped] = unescaped;
            }

            ConditionalsMap[ConditionalXbox360] = 0;
            ConditionalsMap[ConditionalWin32] = 1;
            ConditionalsMap[ConditionalWindows] = 2;
            ConditionalsMap[ConditionalOSX] = 3;
            ConditionalsMap[ConditionalLinux] = 4;
            ConditionalsMap[ConditionalPosix] = 5;
        }

        public static bool IsEscapable(char ch) => (ch < EscapeMapLength && EscapeExistsMap[ch]);
        public static char GetEscape(char ch) => (ch < EscapeMapLength) ? EscapeMap[ch] : ch;
        public static char GetUnescape(char ch) => (ch < EscapeMapLength) ? UnescapeMap[ch] : ch;

        internal static bool PassConditional(bool[] bols, string str)
        {
            if (!ConditionalsMap.TryGetValue(str, out var cond))
                return false;

            return bols[cond];
        }
    }
}
