using Gameloop.Vdf.Linq;
using System.IO;

namespace Gameloop.Vdf
{
    public class VdfSerializer
    {
        private readonly VdfSerializerSettings _settings;

        public VdfSerializer() : this(VdfSerializerSettings.Default) { }

        public VdfSerializer(VdfSerializerSettings settings)
        {
            _settings = settings;
        }

        public void Serialize(TextWriter textWriter, VToken value)
        {
            using VdfWriter vdfWriter = new VdfTextWriter(textWriter, _settings);

            value.WriteTo(vdfWriter);
        }

        public VProperty Deserialize(TextReader textReader)
        {
            using VdfReader vdfReader = new VdfTextReader(textReader, _settings);

            if (!vdfReader.ReadToken())
                throw new VdfException("Incomplete VDF data.");

            // For now, we discard these comments.
            while (vdfReader.CurrentState == VdfReader.State.Comment)
                if (!vdfReader.ReadToken())
                    throw new VdfException("Incomplete VDF data.");

            return vdfReader.ReadProperty();
        }
    }
}
