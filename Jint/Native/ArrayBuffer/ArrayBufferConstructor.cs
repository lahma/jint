using System.Threading;
using Jint.Collections;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Native.TypedArray;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Jint.Native.ArrayBuffer
{
    public sealed class ArrayBufferConstructor : FunctionInstance, IConstructor
    {
        private static readonly JsString _functionName = new("ArrayBuffer");

        internal ArrayBufferConstructor(
            Engine engine,
            Realm realm,
            FunctionPrototype functionPrototype,
            ObjectPrototype objectPrototype)
            : base(engine, realm, _functionName)
        {
            _prototype = functionPrototype;
            PrototypeObject = new ArrayBufferPrototype(engine, realm, this, objectPrototype);
            _length = new PropertyDescriptor(1, PropertyFlag.Configurable);
            _prototypeDescriptor = new PropertyDescriptor(PrototypeObject, PropertyFlag.AllForbidden);
        }

        public ArrayBufferPrototype PrototypeObject { get; }

        protected override void Initialize()
        {
            var properties = new PropertyDictionary(1, checkExistingKeys: false)
            {
                ["isView"] = new PropertyDescriptor(new PropertyDescriptor(new ClrFunctionInstance(Engine, "isView", IsView, 1), PropertyFlag.NonEnumerable)),
            };
            SetProperties(properties);

            var symbols = new SymbolDictionary(1)
            {
                [GlobalSymbolRegistry.Species] = new GetSetPropertyDescriptor(get: new ClrFunctionInstance(Engine, "get [Symbol.species]", Species, 0, PropertyFlag.Configurable), set: Undefined,PropertyFlag.Configurable),
            };
            SetSymbols(symbols);
        }

        private static JsValue IsView(JsValue thisObject, JsValue[] arguments)
        {
            var arg = arguments.At(0);
            return arg is TypedArrayInstance;
        }

        private JsValue Species(JsValue thisObject, JsValue[] arguments)
        {
            return thisObject;
        }

        public override JsValue Call(JsValue thisObject, JsValue[] arguments)
        {
            ExceptionHelper.ThrowTypeError(_realm);
            return Undefined;
        }

        public ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
        {
            if (newTarget.IsUndefined())
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            var byteLength = TypeConverter.ToIndex(_realm, arguments.At(0));
            return AllocateArrayBuffer(newTarget, byteLength);
        }

        private ObjectInstance AllocateArrayBuffer(JsValue constructor, uint byteLength)
        {
            var obj = OrdinaryCreateFromConstructor(
                constructor,
                static intrinsics => intrinsics.ArrayBuffer.PrototypeObject,
                (engine, realm, state) => new ArrayBufferInstance(engine, byteLength));
            return obj;
        }
    }
}