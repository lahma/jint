using Jint.Native.ArrayBuffer;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Descriptors;

namespace Jint.Native.DataView
{
    public sealed class DataViewConstructor : FunctionInstance, IConstructor
    {
        private static readonly JsString _functionName = new("DataView");

        internal DataViewConstructor(
            Engine engine,
            Realm realm,
            FunctionPrototype functionPrototype,
            ObjectPrototype objectPrototype)
            : base(engine, realm, _functionName)
        {
            _prototype = functionPrototype;
            PrototypeObject = new DataViewPrototype(engine, realm, this, objectPrototype);
            _length = new PropertyDescriptor(1, PropertyFlag.Configurable);
            _prototypeDescriptor = new PropertyDescriptor(PrototypeObject, PropertyFlag.AllForbidden);
        }

        public DataViewPrototype PrototypeObject { get; }

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

            var buffer = arguments.At(0) as ArrayBufferInstance;
            var byteOffset = arguments.At(1);
            var byteLength = arguments.At(2);

            if (buffer is null)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }


            var offset = TypeConverter.ToIndex(_realm, byteOffset);

            if (buffer.IsDetachedBuffer)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            var bufferByteLength = buffer._arrayBufferByteLength;
            if (offset > bufferByteLength)
            {
                ExceptionHelper.ThrowRangeError(_realm);
            }

            uint viewByteLength;
            if (byteLength.IsUndefined())
            {
                viewByteLength = bufferByteLength - offset;
            }
            else
            {
                viewByteLength = TypeConverter.ToIndex(_realm, byteLength);
                if (offset + viewByteLength > bufferByteLength)
                {
                 ExceptionHelper.ThrowRangeError(_realm);
                }
            }

            var O = OrdinaryCreateFromConstructor(
                newTarget, intrinsics => intrinsics.DataView.PrototypeObject,
                (engine, realm, state) => new DataViewInstance(engine));

            if (buffer.IsDetachedBuffer)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }


            O._viewedArrayBuffer = buffer;
            O._byteLength = viewByteLength;
            O._byteOffset = offset;
            return O;
        }
    }
}