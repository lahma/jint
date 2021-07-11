using Jint.Native.ArrayBuffer;
using Jint.Native.Object;
using Jint.Runtime;

namespace Jint.Native.TypedArray
{
    public class TypedArrayInstance : ObjectInstance
    {
        private TypedArrayElementType _arrayElementType;
        internal ArrayBufferInstance _viewedArrayBuffer;
        internal uint _byteLength;
        internal int _byteOffset;
        private int _arrayLength;
        private Intrinsics _intrinsics;

        internal TypedArrayInstance(
            Engine engine,
            Intrinsics intrinsics) : base(engine)
        {
            _intrinsics = intrinsics;
        }

        internal TypedArrayInstance(
            Engine engine,
            Intrinsics intrinsics,
            TypedArrayElementType type,
            int length) : this(engine, intrinsics)
        {
            _arrayElementType = type;
        }

        internal void AllocateTypedArrayBuffer(int len)
        {
            var elementSize = _arrayElementType.GetElementSize();
            _byteLength = (uint)(elementSize * len);
            var data = _intrinsics.ArrayBuffer.AllocateArrayBuffer(_intrinsics.ArrayBuffer, _byteLength);
            _viewedArrayBuffer = data;
            _arrayLength = len;
        }
    }
}