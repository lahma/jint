using Jint.Native.Object;

namespace Jint.Native.TypedArray
{
    public abstract class TypedArrayInstance : ObjectInstance
    {
        protected TypedArrayInstance(Engine engine, int bytesPerElement) : base(engine)
        {
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
        public Uint8ClampedArray(Engine engine, int bytesPerElement) : base(engine, 1)
        {
        }
    }

    internal class Int16Array : TypedArrayInstance
    {
        public Int16Array(Engine engine, int bytesPerElement) : base(engine, 2)
        {
        }
    }

    internal class Uint16Array : TypedArrayInstance
    {
        public Uint16Array(Engine engine, int bytesPerElement) : base(engine, 2)
        {
        }
    }

    internal class Int32Array : TypedArrayInstance
    {
        public Int32Array(Engine engine, int bytesPerElement) : base(engine, 4)
        {
        }
    }

    internal class Uint32Array : TypedArrayInstance
    {
        public Uint32Array(Engine engine, int bytesPerElement) : base(engine, 4)
        {
        }
    }

    internal class BigInt64Array : TypedArrayInstance
    {
        public BigInt64Array(Engine engine, int bytesPerElement) : base(engine, 8)
        {
        }
    }

    internal class BigUint64Array : TypedArrayInstance
    {
        public BigUint64Array(Engine engine, int bytesPerElement) : base(engine, 8)
        {
        }
    }

    internal class Float32Array : TypedArrayInstance
    {
        public Float32Array(Engine engine, int bytesPerElement) : base(engine, 4)
        {
        }
    }

    internal class Float64Array : TypedArrayInstance
    {
        public Float64Array(Engine engine, int bytesPerElement) : base(engine, 8)
        {
        }
    }
}