using System.IO;
using System.Text;

namespace Destrospean.Misc
{
    public class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter(StringBuilder stringBuilder) : base(stringBuilder)
        {
        }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
    }
}
