namespace Gameloop.Vdf
{
    public class VdfSerializerSettings
    {
        public static VdfSerializerSettings Default => new VdfSerializerSettings()
        {
            IsWindows = true
        };
        public static VdfSerializerSettings Common => new VdfSerializerSettings
        {
            IsWindows = true,
            UsesEscapeSequences = true,
            UsesConditionals = true
        };

        /// <summary>
        /// Determines whether the parser should translate escape sequences (/n, /t, etc.).
        /// </summary>
        public bool UsesEscapeSequences = false;

        /// <summary>
        /// Determines whether the parser should evaluate conditional blocks ([$WINDOWS], etc.).
        /// </summary>
        public bool UsesConditionals = true;


        /// <summary>
        /// Determines whether the parser should read cpp comments.
        /// </summary>
        public bool UsesComments = false;

        /// <summary>
        /// Sets the size of the initial token buffer used for deserialization. If token is larger, reallocation will occur.
        /// </summary>
        public int PreferableTokenSize = 4096;

        /// <summary>
        /// Default size of buffer which is loaded from Reader.
        /// </summary>
        public int DefaultBufferSize = 1024;

        // System information

        public bool IsXBox360
        {
            get => ConditionalsArray[0];
            set => ConditionalsArray[0] = value;
        }

        public bool IsWin32
        {
            get => ConditionalsArray[1];
            set => ConditionalsArray[1] = value;
        }
        public bool IsWindows
        {
            get => ConditionalsArray[2];
            set => ConditionalsArray[2] = value;
        }
        public bool IsOSX
        {
            get => ConditionalsArray[3];
            set => ConditionalsArray[3] = value;
        }
        public bool IsLinux
        {
            get => ConditionalsArray[4];
            set => ConditionalsArray[4] = value;
        }
        public bool IsPosix
        {
            get => ConditionalsArray[5];
            set => ConditionalsArray[5] = value;
        }

        internal bool[] ConditionalsArray = new bool[6];
    }
}
