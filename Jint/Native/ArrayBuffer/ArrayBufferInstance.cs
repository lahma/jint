using Jint.Native.Object;
using Jint.Runtime;

namespace Jint.Native.ArrayBuffer
{
    public class ArrayBufferInstance : ObjectInstance
    {
        internal byte[] _arrayBufferData;
        internal uint _arrayBufferByteLength;
        private JsValue _arrayBufferDetachKey = null;

        internal ArrayBufferInstance(
            Engine engine,
            uint byteLength) : base(engine)
        {
            var block = CreateByteDataBlock(byteLength);
            _arrayBufferData = block;
            _arrayBufferByteLength = byteLength;
        }

        private byte[] CreateByteDataBlock(uint byteLength)
        {
            return new byte[byteLength];
        }

        internal bool IsDetachedBuffer => _arrayBufferData is null;

        internal bool IsSharedArrayBuffer => false; // TODO

        internal void DetachArrayBuffer(JsValue key = null)
        {
            key ??= Undefined;

            if (!SameValue(_arrayBufferDetachKey, key))
            {
                ExceptionHelper.ThrowTypeError(_engine.Realm);
            }

            _arrayBufferData = null;
            _arrayBufferByteLength = 0;
        }
    }
}