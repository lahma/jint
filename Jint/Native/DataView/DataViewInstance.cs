using Jint.Native.ArrayBuffer;
using Jint.Native.Object;

namespace Jint.Native.DataView
{
    public class DataViewInstance : ObjectInstance
    {
        internal ArrayBufferInstance _viewedArrayBuffer;
        internal uint _byteLength;
        internal uint _byteOffset;

        internal DataViewInstance(Engine engine) : base(engine)
        {
        }
    }
}