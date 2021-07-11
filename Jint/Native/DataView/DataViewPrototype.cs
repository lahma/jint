using System;
using Jint.Collections;
using Jint.Native.ArrayBuffer;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Native.TypedArray;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Jint.Native.DataView
{
    public class DataViewPrototype : DataViewInstance
    {
        private readonly Realm _realm;
        private readonly DataViewConstructor _constructor;

        internal DataViewPrototype(
            Engine engine,
            Realm realm,
            DataViewConstructor constructor,
            ObjectPrototype objectPrototype) : base(engine)
        {
            _prototype = objectPrototype;
            _realm = realm;
            _constructor = constructor;
        }

        protected override void Initialize()
        {
            var properties = new PropertyDictionary(23, checkExistingKeys: false)
            {
                ["buffer"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(_engine, "buffer", Buffer), Undefined, PropertyFlag.AllForbidden),
                ["byteLength"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(_engine, "byteLength", ByteLength), Undefined, PropertyFlag.AllForbidden),
                ["byteOffset"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "byteOffset", ByteOffset), Undefined, PropertyFlag.AllForbidden),
                ["getBigInt64"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "getBigInt64", GetBigInt64), Undefined, PropertyFlag.AllForbidden),
                ["getBigUint64"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "getBigUint64", GetBigUint64), Undefined, PropertyFlag.AllForbidden),
                ["getFloat32"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "getFloat32", GetFloat32), Undefined, PropertyFlag.AllForbidden),
                ["getFloat64"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "getFloat64", GetFloat64), Undefined, PropertyFlag.AllForbidden),
                ["getInt8"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "getInt8", GetInt8), Undefined, PropertyFlag.AllForbidden),
                ["getInt16"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "getInt16", GetInt16), Undefined, PropertyFlag.AllForbidden),
                ["getInt32"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "getInt32", GetInt32), Undefined, PropertyFlag.AllForbidden),
                ["getUint8"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "getUint8", GetUint8), Undefined, PropertyFlag.AllForbidden),
                ["getUint16"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "getUint16", GetUint16), Undefined, PropertyFlag.AllForbidden),
                ["getUint32"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "getUint32", GetUint32), Undefined, PropertyFlag.AllForbidden),
                ["setBigInt64"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "setBigInt64", SetBigInt64), Undefined, PropertyFlag.AllForbidden),
                ["setBigUint64"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "setBigUint64", SetBigUint64), Undefined, PropertyFlag.AllForbidden),
                ["setFloat32"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "setFloat32", SetFloat32), Undefined, PropertyFlag.AllForbidden),
                ["setFloat64"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "setFloat64", SetFloat64), Undefined, PropertyFlag.AllForbidden),
                ["setInt8"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "setInt8", SetInt8), Undefined, PropertyFlag.AllForbidden),
                ["setInt16"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "setInt16", SetInt16), Undefined, PropertyFlag.AllForbidden),
                ["setInt32"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "setInt32", SetInt32), Undefined, PropertyFlag.AllForbidden),
                ["setUint8"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "setUint8", SetUint8), Undefined, PropertyFlag.AllForbidden),
                ["setUint16"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "setUint16", SetUint16), Undefined, PropertyFlag.AllForbidden),
                ["setUint32"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(Engine, "setUint32", SetUint32), Undefined, PropertyFlag.AllForbidden)
            };
            SetProperties(properties);

            var symbols = new SymbolDictionary(1)
            {
                [GlobalSymbolRegistry.ToStringTag] = new PropertyDescriptor("DataView", PropertyFlag.Configurable)
            };
            SetSymbols(symbols);
        }

        private JsValue Buffer(JsValue thisObj, JsValue[] arguments)
        {
            var o = thisObj as DataViewInstance;
            if (o is null)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            return o._viewedArrayBuffer;
        }

        private JsValue ByteLength(JsValue thisObj, JsValue[] arguments)
        {
            var o = thisObj as DataViewInstance;
            if (o is null)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            var buffer = o._viewedArrayBuffer;
            if (buffer.IsDetachedBuffer)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            return JsNumber.Create(o._byteLength);
        }

        private JsValue ByteOffset(JsValue thisObj, JsValue[] arguments)
        {
            var o = thisObj as DataViewInstance;
            if (o is null)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            var buffer = o._viewedArrayBuffer;
            if (buffer.IsDetachedBuffer)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            return JsNumber.Create(o._byteOffset);
        }

        private JsValue GetBigInt64(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue(thisObj, arguments.At(0), arguments.At(1), TypedArrayElementType.BigInt64);
        }

        private JsValue GetBigUint64(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue(thisObj, arguments.At(0), arguments.At(1), TypedArrayElementType.BigUint64);
        }

        private JsValue GetFloat32(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue(thisObj, arguments.At(0), arguments.At(1, JsBoolean.False), TypedArrayElementType.Float32);
        }

        private JsValue GetFloat64(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue(thisObj, arguments.At(0), arguments.At(1, JsBoolean.False), TypedArrayElementType.Float64);
        }

        private JsValue GetInt8(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue(thisObj, arguments.At(0), JsBoolean.True, TypedArrayElementType.Int8);
        }

        private JsValue GetInt16(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue(thisObj, arguments.At(0), arguments.At(1, JsBoolean.False), TypedArrayElementType.Int16);
        }

        private JsValue GetInt32(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue(thisObj, arguments.At(0), arguments.At(1, JsBoolean.False), TypedArrayElementType.Int32);
        }

        private JsValue GetUint8(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue(thisObj, arguments.At(0), JsBoolean.True, TypedArrayElementType.Uint8);
        }

        private JsValue GetUint16(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue(thisObj, arguments.At(0), arguments.At(1, JsBoolean.False), TypedArrayElementType.Uint16);
        }

        private JsValue GetUint32(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue(thisObj, arguments.At(0), arguments.At(1, JsBoolean.False), TypedArrayElementType.Uint32);
        }

        private JsValue SetBigInt64(JsValue thisObj, JsValue[] arguments)
        {
            return SetViewValue(thisObj, arguments.At(0), arguments.At(2), TypedArrayElementType.BigInt64, arguments.At(1));
        }

        private JsValue SetBigUint64(JsValue thisObj, JsValue[] arguments)
        {
            return SetViewValue(thisObj, arguments.At(0), arguments.At(2), TypedArrayElementType.BigUint64, arguments.At(1));
        }

        private JsValue SetFloat32 (JsValue thisObj, JsValue[] arguments)
        {
            return SetViewValue(thisObj, arguments.At(0), arguments.At(2, JsBoolean.False), TypedArrayElementType.Float32, arguments.At(1));
        }

        private JsValue SetFloat64(JsValue thisObj, JsValue[] arguments)
        {
            return SetViewValue(thisObj, arguments.At(0), arguments.At(2, JsBoolean.False), TypedArrayElementType.Float64, arguments.At(1));
        }

        private JsValue SetInt8 (JsValue thisObj, JsValue[] arguments)
        {
            return SetViewValue(thisObj, arguments.At(0), JsBoolean.True, TypedArrayElementType.Int8, arguments.At(1));
        }

        private JsValue SetInt16(JsValue thisObj, JsValue[] arguments)
        {
            return SetViewValue(thisObj, arguments.At(0), arguments.At(2, JsBoolean.False), TypedArrayElementType.Int16, arguments.At(1));
        }

        private JsValue SetInt32(JsValue thisObj, JsValue[] arguments)
        {
            return SetViewValue(thisObj, arguments.At(0), arguments.At(2, JsBoolean.False), TypedArrayElementType.Int32, arguments.At(1));
        }

        private JsValue SetUint8(JsValue thisObj, JsValue[] arguments)
        {
            return SetViewValue(thisObj, arguments.At(0), JsBoolean.True, TypedArrayElementType.Uint8, arguments.At(1));
        }

        private JsValue SetUint16(JsValue thisObj, JsValue[] arguments)
        {
            return SetViewValue(thisObj, arguments.At(0), arguments.At(2, JsBoolean.False), TypedArrayElementType.Uint16, arguments.At(1));
        }

        private JsValue SetUint32(JsValue thisObj, JsValue[] arguments)
        {
            return SetViewValue(thisObj, arguments.At(0), arguments.At(2, JsBoolean.False), TypedArrayElementType.Uint32, arguments.At(1));
        }

        private JsValue GetViewValue(JsValue view, JsValue requestIndex, JsValue isLittleEndian, TypedArrayElementType type)
        {
            var dataView = view as DataViewInstance;
            if (dataView is null)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            var getIndex = TypeConverter.ToIndex(_realm, requestIndex);
            var isLittleEndianBoolean = TypeConverter.ToBoolean(isLittleEndian);
            var buffer = dataView._viewedArrayBuffer;

            if (buffer.IsDetachedBuffer)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            var viewOffset = dataView._byteOffset;
            var viewSize = dataView._byteLength;
            var elementSize = type.ToElementSize();
            if (getIndex + elementSize > viewSize)
            {
                ExceptionHelper.ThrowRangeError(_realm);
            }

            var bufferIndex = getIndex + viewOffset;
            return buffer.GetValueFromBuffer(bufferIndex, type, false, ArrayBufferInstance.BufferOrder.Unordered, isLittleEndianBoolean);
        }

        private JsValue SetViewValue(JsValue view, JsValue requestIndex, JsValue isLittleEndian, TypedArrayElementType type, JsValue value)
        {
            var dataView = view as DataViewInstance;
            if (dataView is null)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            var getIndex = TypeConverter.ToIndex(_realm, requestIndex);

            double numberValue;
            if (type.IsBigIntElementType())
            {
                // let numberValue be ? ToBigInt(value).
                throw new NotImplementedException("BigInt not implemented");
            }
            else
            {
                numberValue = TypeConverter.ToNumber(value);
            }

            var isLittleEndianBoolean = TypeConverter.ToBoolean(isLittleEndian);
            var buffer = dataView._viewedArrayBuffer;

            if (buffer.IsDetachedBuffer)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            var viewOffset = dataView._byteOffset;
            var viewSize = dataView._byteLength;
            var elementSize = type.ToElementSize();
            if (getIndex + elementSize > viewSize)
            {
                ExceptionHelper.ThrowRangeError(_realm);
            }

            var bufferIndex = getIndex + viewOffset;
            buffer.SetValueInBuffer(bufferIndex, type, numberValue, false, ArrayBufferInstance.BufferOrder.Unordered, isLittleEndianBoolean);
            return Undefined;
        }

    }
}