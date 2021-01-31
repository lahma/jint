using Jint.Native.Object;
using Jint.Runtime.Descriptors;

namespace Jint.Native.TypedArray
{
    /// <summary>
    /// https://tc39.es/ecma262/#sec-properties-of-the-%typedarrayprototype%-object
    /// </summary>
    public sealed class TypedArrayPrototype : TypedArrayInstance
    {
        private TypedArrayConstructor _constructor;
        private PropertyDescriptor _length;

        internal TypedArrayPrototype(
            Engine engine,
            ObjectPrototype objectPrototype,
            TypedArrayConstructor constructor) : base(engine, 123)
        {
            _prototype = objectPrototype;
            _length = new PropertyDescriptor(JsNumber.PositiveZero, PropertyFlag.Writable);
            _constructor = constructor;
        }
    }
}