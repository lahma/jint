using Jint.Native.ArrayBuffer;
using Jint.Native.Object;

namespace Jint.Native.TypedArray
{
    public abstract class TypedArrayInstance : ObjectInstance
    {
        internal ArrayBufferInstance ViewArrayBuffer;
        internal ArrayBufferInstance ViewedArrayBuffer;
        internal uint byteLength;
        internal int byteOffset;

        protected TypedArrayInstance(Engine engine, uint byteLength) : base(engine)
        {
            this.byteLength = byteLength;
            ViewArrayBuffer = new ArrayBufferInstance(engine, byteLength);
            ViewedArrayBuffer = new ArrayBufferInstance(engine, byteLength);
        }
    }

    internal class Int8Array : TypedArrayInstance
    {
        public Int8Array(Engine engine) : base(engine, 1)
        {
        }
    }

    internal class Uint8Array : TypedArrayInstance
    {
        public Uint8Array(Engine engine) : base(engine, 1)
        {
        }
    }

    internal class Uint8ClampedArray : TypedArrayInstance
    {
        public Uint8ClampedArray(Engine engine) : base(engine, 1)
        {
        }
    }

    internal class Int16Array : TypedArrayInstance
    {
        public Int16Array(Engine engine) : base(engine, 2)
        {
        }
    }

    internal class Uint16Array : TypedArrayInstance
    {
        public Uint16Array(Engine engine) : base(engine, 2)
        {
        }
    }

    internal class Int32Array : TypedArrayInstance
    {
        public Int32Array(Engine engine) : base(engine, 4)
        {
        }
    }

    internal class Uint32Array : TypedArrayInstance
    {
        public Uint32Array(Engine engine) : base(engine, 4)
        {
        }
    }

    internal class BigInt64Array : TypedArrayInstance
    {
        public BigInt64Array(Engine engine) : base(engine, 8)
        {
        }
    }

    internal class BigUint64Array : TypedArrayInstance
    {
        public BigUint64Array(Engine engine) : base(engine, 8)
        {
        }
    }

    internal class Float32Array : TypedArrayInstance
    {
        public Float32Array(Engine engine) : base(engine, 4)
        {
        }
    }

    internal class Float64Array : TypedArrayInstance
    {
        public Float64Array(Engine engine) : base(engine, 8)
        {
        }
    }
}