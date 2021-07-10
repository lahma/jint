using System;
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

            var mapfn = !mapFunction.IsUndefined();
            if (mapfn)
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
                a. Let values be ? IterableToList(source, usingIterator).
                    b. Let len be the number of elements in values.
                    c. Let targetObj be ? TypedArrayCreate(C, « 𝔽(len) »).
                d. Let k be 0.
                    e. Repeat, while k < len,
                i. Let Pk be ! ToString(𝔽(k)).
                ii. Let kValue be the first element of values and remove that element from values.
                    iii. If mapping is true, then
                1. Let mappedValue be ? Call(mapfn, thisArg, « kValue, 𝔽(k) »).
                iv. Else, let mappedValue be kValue.
                    v. Perform ? Set(targetObj, Pk, mappedValue, true).
                    vi. Set k to k + 1.
                    f. Assert: values is now an empty List.
                    g. Return targetObj.
                    */
                throw new NotImplementedException();
            }

            if (source.IsNullOrUndefined())
            {
                ExceptionHelper.ThrowTypeError(_realm, "Cannot convert undefined or null to object");
            }

            var arrayLike = TypeConverter.ToObject(_realm, source);
            var len = arrayLike.Length;

            var argumentList = new JsValue[] { JsNumber.Create(len) };
            var targetObj = TypedArrayCreate((IConstructor) c, argumentList);

            var mappingArgs = mapfn ? new JsValue[2] : null;
            for (uint k = 0; k < len; ++k)
            {
                var Pk = JsNumber.Create(k);
                var kValue = arrayLike.Get(Pk);
                JsValue mappedValue;
                if (mapfn)
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
            throw new System.NotImplementedException();
        }

        public ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
        {
            if (_arrayConstructor is null)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            var array = _arrayConstructor(_engine, 0);
            return array;
        }
    }
}