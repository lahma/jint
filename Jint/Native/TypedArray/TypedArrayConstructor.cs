using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Descriptors;

namespace Jint.Native.TypedArray
{
    public sealed class TypedArrayConstructor : FunctionInstance, IConstructor
    {
        internal TypedArrayConstructor(
            Engine engine,
            Realm realm,
            FunctionPrototype functionPrototype,
            ObjectPrototype objectPrototype,
            JsString functionName) :  base(engine, realm, functionName)
        {
            _prototype = functionPrototype;
            PrototypeObject = new TypedArrayPrototype(engine, objectPrototype, this);
            _length = new PropertyDescriptor(JsNumber.One, PropertyFlag.Configurable);
            _prototypeDescriptor = new PropertyDescriptor(PrototypeObject, PropertyFlag.AllForbidden);
        }

        public TypedArrayPrototype PrototypeObject { get; private set; }

        public override JsValue Call(JsValue thisObject, JsValue[] arguments)
        {
            throw new System.NotImplementedException();
        }

        public ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
        {
            throw new System.NotImplementedException();
        }
    }
}