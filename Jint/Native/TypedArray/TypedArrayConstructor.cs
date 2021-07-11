using System;
using System.Collections.Generic;
using Jint.Collections;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Jint.Native.TypedArray
{
    public sealed class TypedArrayConstructor : FunctionInstance, IConstructor
    {
        internal readonly int _bytesPerElement;
        private readonly Func<Engine, int, TypedArrayInstance> _arrayConstructor;

        internal TypedArrayConstructor(
            Engine engine,
            Realm realm,
            ObjectInstance functionPrototype,
            ObjectInstance objectPrototype,
            string functionName,
            int bytesPerElement,
            Func<Engine, int, TypedArrayInstance> arrayConstructor) : base(engine, realm, new JsString(functionName))
        {
            _bytesPerElement = bytesPerElement;
            _arrayConstructor = arrayConstructor;
            _prototype = functionPrototype;
            PrototypeObject = new TypedArrayPrototype(engine, realm, objectPrototype, this);
            _length = new PropertyDescriptor(JsNumber.Three, PropertyFlag.Configurable);
            _prototypeDescriptor = new PropertyDescriptor(PrototypeObject, PropertyFlag.AllForbidden);
        }

        public TypedArrayPrototype PrototypeObject { get; }

        protected override void Initialize()
        {
            var properties = new PropertyDictionary(3, checkExistingKeys: false)
            {
                ["BYTES_PER_ELEMENT"] = new PropertyDescriptor(new PropertyDescriptor(_bytesPerElement, PropertyFlag.AllForbidden)),
                ["from"] = new PropertyDescriptor(new PropertyDescriptor(new ClrFunctionInstance(Engine, "from", From, 1, PropertyFlag.Configurable), PropertyFlag.NonEnumerable)),
                ["of"] = new PropertyDescriptor(new PropertyDescriptor(new ClrFunctionInstance(Engine, "of", Of, 0, PropertyFlag.Configurable), PropertyFlag.NonEnumerable))
            };
            SetProperties(properties);

            var symbols = new SymbolDictionary(1)
            {
                [GlobalSymbolRegistry.Species] = new GetSetPropertyDescriptor(get: new ClrFunctionInstance(Engine, "get [Symbol.species]", Species, 0, PropertyFlag.Configurable), set: Undefined,PropertyFlag.Configurable),
            };
            SetSymbols(symbols);
        }

        private JsValue From(JsValue thisObj, JsValue[] arguments)
        {
            var c = thisObj;
            if (!c.IsConstructor)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            var source = arguments.At(0);
            var mapFunction = arguments.At(1);
            var thisArg = arguments.At(2);

            var mapping = !mapFunction.IsUndefined();
            if (mapping)
            {
                if (!mapFunction.IsCallable)
                {
                    ExceptionHelper.ThrowTypeError(_realm);
                }
            }

            var usingIterator = GetMethod(_realm, source, GlobalSymbolRegistry.Iterator);
            if (usingIterator is not null)
            {
            	/*
                var values = IterableToList(source, usingIterator);
                var iteratorLen = values.Count;
                var iteratorTarget = TypedArrayCreate((IConstructor) c,  new JsValue[] { iteratorLen });
                for (var k = 0; k < iteratorLen; ++k)
                {
                    var Pk = TypeConverter.ToJsString(k);
                    var kValue = values[k];
                    var mappedValue = mapping
                        ? ((ICallable) mapFunction).Call(thisArg, new[] { kValue, Pk })
                        : kValue;
                    iteratorTarget.Set(Pk, mappedValue, true);
                }

                return iteratorTarget;
*/
            }

            if (source.IsNullOrUndefined())
            {
                ExceptionHelper.ThrowTypeError(_realm, "Cannot convert undefined or null to object");
            }

            var arrayLike = TypeConverter.ToObject(_realm, source);
            var len = arrayLike.Length;

            var argumentList = new JsValue[] { JsNumber.Create(len) };
            var targetObj = TypedArrayCreate((IConstructor) c, argumentList);

            var mappingArgs = mapping ? new JsValue[2] : null;
            for (uint k = 0; k < len; ++k)
            {
                var Pk = JsNumber.Create(k);
                var kValue = arrayLike.Get(Pk);
                JsValue mappedValue;
                if (mapping)
                {
                    mappingArgs[0] = kValue;
                    mappingArgs[1] = Pk;
                    mappedValue = ((ICallable) mapFunction).Call(thisArg, mappingArgs);
                }
                else
                {
                    mappedValue = kValue;

                }

                targetObj.Set(Pk, mappedValue, true);
            }

            return targetObj;
        }

        private JsValue Of(JsValue thisObj, JsValue[] items)
        {
            var len = items.Length;

            if (!thisObj.IsConstructor)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            var newObj = TypedArrayCreate((IConstructor) thisObj, new JsValue[] { len });

            for (uint k = 0; k < items.Length; k++)
            {
                var kValue = items[k];
                var key = JsString.Create(k);
                newObj.Set(key, kValue, true);
            }

            newObj.Set(CommonProperties.Length, len, true);

            return newObj;
        }

        private TypedArrayInstance TypedArrayCreate(IConstructor constructor, JsValue[] argumentList)
        {
            var newTypedArray = Construct(constructor, argumentList);
            newTypedArray.ValidateTypedArray(_realm);
            if (argumentList.Length == 1 && argumentList[0] is JsNumber number)
            {
                if (newTypedArray.Length < number._value)
                {
                    ExceptionHelper.ThrowTypeError(_realm);
                }
            }

            return (TypedArrayInstance) newTypedArray;
        }

        private static JsValue Species(JsValue thisObject, JsValue[] arguments)
        {
            return thisObject;
        }

        public override JsValue Call(JsValue thisObject, JsValue[] arguments)
        {
            if (_arrayConstructor is null)
            {
                ExceptionHelper.ThrowTypeError(_realm, "Abstract class TypedArray not directly constructable");
            }

            return Undefined;
        }

        public ObjectInstance Construct(JsValue[] args, JsValue newTarget)
        {
            if (_arrayConstructor is null)
            {
                ExceptionHelper.ThrowTypeError(_realm, "Abstract class TypedArray not directly constructable");
            }

            if (newTarget.IsUndefined())
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            Func<Intrinsics, ObjectInstance> proto = static Intrinsics => Intrinsics.TypedArray.PrototypeObject;
            var numberOfArgs = args.Length;
            if (numberOfArgs == 0)
            {
                return AllocateTypedArray(newTarget, proto, 0);
            }

            var firstArgument = args[0];
            if (firstArgument.IsObject())
            {
                var o = AllocateTypedArray(newTarget, proto);
                if (firstArgument is TypedArrayInstance typedArrayInstance)
                {
                    InitializeTypedArrayFromTypedArray(o, typedArrayInstance);
                }
                else if (firstArgument is ArrayBuffer.ArrayBufferInstance arrayBuffer)
                {
                   var byteOffset = numberOfArgs > 1 ? args[1] : Undefined;
                   var length = numberOfArgs > 2 ? args[2] : Undefined;
                   InitializeTypedArrayFromArrayBuffer(o, arrayBuffer, byteOffset, length);
                }
                else
                {
                    var usingIterator = GetMethod(_realm, firstArgument, GlobalSymbolRegistry.Iterator);
                    if (usingIterator is not null)
                    {
                        var values = IterableToList(firstArgument, usingIterator);
                        InitializeTypedArrayFromList(o, values);
                    }
                    else
                    {
                        InitializeTypedArrayFromArrayLike(o, firstArgument);
                    }
                }
                return o;
            }
            else
            {
                var elementLength = TypeConverter.ToIndex(_realm, firstArgument);
                return AllocateTypedArray(newTarget, proto, elementLength);
            }
        }

        private List<JsValue> IterableToList(JsValue items, ICallable usingIterator)
        {
            var iteratorRecord = items.GetIterator(_realm);
            var values = new List<JsValue>();
            while (iteratorRecord.TryIteratorStep(out var nextItem))
            {
                values.Add(nextItem);
            }
            return values;
        }

        private void InitializeTypedArrayFromTypedArray(TypedArrayInstance o, TypedArrayInstance srcArray)
        {
            throw new NotImplementedException();
        }

        private void InitializeTypedArrayFromArrayBuffer(TypedArrayInstance o, ArrayBuffer.ArrayBufferInstance buffer, JsValue byteOffset, JsValue length)
        {
            throw new NotImplementedException();
        }

        private void InitializeTypedArrayFromList(TypedArrayInstance o, List<JsValue> values)
        {
            throw new NotImplementedException();
        }

        private void InitializeTypedArrayFromArrayLike(TypedArrayInstance o, JsValue arrayLike)
        {
            throw new NotImplementedException();
        }

        private TypedArrayInstance AllocateTypedArray(JsValue newTarget, Func<Intrinsics, ObjectInstance> defaultProto, uint length = 0)
        {
            var proto = GetPrototypeFromConstructor(newTarget, defaultProto);
            var obj = _arrayConstructor(_engine, 0);
            obj._prototype = proto;
            return obj;
        }


    }
}