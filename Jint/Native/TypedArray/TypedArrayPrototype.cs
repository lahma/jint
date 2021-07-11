using System;
using Jint.Collections;
using Jint.Native.Iterator;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Jint.Native.TypedArray
{
    /// <summary>
    /// https://tc39.es/ecma262/#sec-properties-of-the-%typedarrayprototype%-object
    /// </summary>
    public sealed class TypedArrayPrototype : TypedArrayInstance
    {
        private readonly Realm _realm;
        private readonly TypedArrayConstructor _constructor;
        private PropertyDescriptor _length;
        private ClrFunctionInstance _originalIteratorFunction;

        internal TypedArrayPrototype(
            Engine engine,
            Realm realm,
            ObjectInstance objectPrototype,
            TypedArrayConstructor constructor) : base(engine, realm.Intrinsics)
        {
            _prototype = objectPrototype;
            _length = new PropertyDescriptor(JsNumber.PositiveZero, PropertyFlag.Writable);
            _realm = realm;
            _constructor = constructor;
        }

        protected override void Initialize()
        {
            const PropertyFlag propertyFlags = PropertyFlag.Writable | PropertyFlag.Configurable;
            var properties = new PropertyDictionary(30, checkExistingKeys: false)
            {
                ["buffer"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(_engine, "buffer", Buffer), Undefined, PropertyFlag.AllForbidden),
                ["byteLength"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(_engine, "byteLength", ByteLength), Undefined, PropertyFlag.AllForbidden),
                ["byteOffset"] = new GetSetPropertyDescriptor(new ClrFunctionInstance(_engine, "byteOffset", ByteOffset), Undefined, PropertyFlag.AllForbidden),
                ["BYTES_PER_ELEMENT"] = new PropertyDescriptor(new PropertyDescriptor(_constructor._bytesPerElement, PropertyFlag.AllForbidden)),
                ["constructor"] = new PropertyDescriptor(_constructor, PropertyFlag.NonEnumerable),
                ["copyWithin"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "copyWithin", CopyWithin, 2, PropertyFlag.Configurable), propertyFlags),
                ["entries"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "entries", Entries, 0, PropertyFlag.Configurable), propertyFlags),
                ["every"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "every", Every, 1, PropertyFlag.Configurable), propertyFlags),
                ["fill"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "fill", Fill, 1, PropertyFlag.Configurable), propertyFlags),
                ["filter"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "filter", Filter, 1, PropertyFlag.Configurable), propertyFlags),
                ["find"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "find", Find, 1, PropertyFlag.Configurable), propertyFlags),
                ["findIndex"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "findIndex", FindIndex, 1, PropertyFlag.Configurable), propertyFlags),
                ["forEach"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "forEach", ForEach, 1, PropertyFlag.Configurable), propertyFlags),
                ["includes"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "includes", Includes, 1, PropertyFlag.Configurable), propertyFlags),
                ["indexOf"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "indexOf", IndexOf, 1, PropertyFlag.Configurable), propertyFlags),
                ["join"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "join", Join, 1, PropertyFlag.Configurable), propertyFlags),
                ["keys"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "keys", Keys, 0, PropertyFlag.Configurable), propertyFlags),
                ["lastIndexOf"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "lastIndexOf", LastIndexOf, 1, PropertyFlag.Configurable), propertyFlags),
                ["map"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "map", Map, 1, PropertyFlag.Configurable), propertyFlags),
                ["reduce"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "reduce", Reduce, 1, PropertyFlag.Configurable), propertyFlags),
                ["reduceRight"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "reduceRight", ReduceRight, 1, PropertyFlag.Configurable), propertyFlags),
                ["reverse"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "reverse", Reverse, 0, PropertyFlag.Configurable), propertyFlags),
                ["set"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "set", Set, 2, PropertyFlag.Configurable), propertyFlags),
                ["slice"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "slice", Slice, 2, PropertyFlag.Configurable), propertyFlags),
                ["some"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "some", Some, 1, PropertyFlag.Configurable), propertyFlags),
                ["sort"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "sort", Sort, 1, PropertyFlag.Configurable), propertyFlags),
                ["subarray"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "subarray", Subarray, 2, PropertyFlag.Configurable), propertyFlags),
                ["toLocaleString"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "toLocaleString", ToLocaleString, 0, PropertyFlag.Configurable), propertyFlags),
                ["toString"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "toLocaleString", _realm.Intrinsics.Array.PrototypeObject.ToString, 0, PropertyFlag.Configurable), propertyFlags),
                ["values"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "values", Values, 0, PropertyFlag.Configurable), propertyFlags),
            };
            SetProperties(properties);

            _originalIteratorFunction = new ClrFunctionInstance(Engine, "iterator", Values, 1);
            var symbols = new SymbolDictionary(2)
            {
                [GlobalSymbolRegistry.Iterator] = new PropertyDescriptor(_originalIteratorFunction, propertyFlags),
                [GlobalSymbolRegistry.ToStringTag] = new GetSetPropertyDescriptor(
                    get: new ClrFunctionInstance(Engine, "values", ToStringTag, 0, PropertyFlag.Configurable),
                    set: Undefined,
                    PropertyFlag.Configurable)
            };
            SetSymbols(symbols);
        }

        private JsValue Buffer(JsValue thisObj, JsValue[] arguments)
        {
            var o = thisObj as TypedArrayInstance;
            if (o is null)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            return o._viewedArrayBuffer;
        }

        private JsValue ByteLength(JsValue thisObj, JsValue[] arguments)
        {
            var o = thisObj as TypedArrayInstance;
            if (o is null)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            if (o._viewedArrayBuffer.IsDetachedBuffer)
            {
                return JsNumber.PositiveZero;
            }

            return JsNumber.Create(o._byteLength);
        }

        private JsValue ByteOffset(JsValue thisObj, JsValue[] arguments)
        {
            var o = thisObj as TypedArrayInstance;
            if (o is null)
            {
                ExceptionHelper.ThrowTypeError(_realm);
            }

            if (o._viewedArrayBuffer.IsDetachedBuffer)
            {
                return JsNumber.PositiveZero;
            }

            return JsNumber.Create(o._byteOffset);
        }

        private JsValue CopyWithin(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance Entries(JsValue thisObj, JsValue[] arguments)
        {
            var o = thisObj as ObjectInstance;
            o.ValidateTypedArray(_realm);
            return CreateArrayIterator(o, ArrayIteratorType.KeyAndValue);
        }

        private ObjectInstance Every(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance Fill(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance Filter(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance Find(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance FindIndex(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance ForEach(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance Includes(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance IndexOf(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance Join(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance Keys(JsValue thisObj, JsValue[] arguments)
        {
            var o = thisObj as ObjectInstance;
            o.ValidateTypedArray(_realm);
            return CreateArrayIterator(o, ArrayIteratorType.Key);
        }

        private ObjectInstance LastIndexOf(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance Map(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance Reduce(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance ReduceRight(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance Reverse(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance Set(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException();
        }

        private ObjectInstance Slice(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

        private ObjectInstance Some(JsValue thisObj, JsValue[] arguments)
        {
            throw new NotImplementedException("same as Array version");
        }

       private ObjectInstance Sort(JsValue thisObj, JsValue[] arguments)
        {
            /*
             * %TypedArray%.prototype.sort is a distinct function that, except as described below,
             * implements the same requirements as those of Array.prototype.sort as defined in 23.1.3.27.
             * The implementation of the %TypedArray%.prototype.sort specification may be optimized with the knowledge that the this value is
             * an object that has a fixed length and whose integer-indexed properties are not sparse.
             */
            throw new NotImplementedException("same as Array version");
        }

       private ObjectInstance Subarray(JsValue thisObj, JsValue[] arguments)
       {
           throw new NotImplementedException();
       }

       private ObjectInstance ToLocaleString(JsValue thisObj, JsValue[] arguments)
       {
           /*
            * %TypedArray%.prototype.toLocaleString is a distinct function that implements the same algorithm as Array.prototype.toLocaleString
            * as defined in 23.1.3.29 except that the this value's [[ArrayLength]] internal slot is accessed in place of performing
            * a [[Get]] of "length". The implementation of the algorithm may be optimized with the knowledge that the this value is an object
            * that has a fixed length and whose integer-indexed properties are not sparse. However, such optimization must not introduce
            * any observable changes in the specified behaviour of the algorithm.
            */

           throw new NotImplementedException();
       }

       private ObjectInstance Values(JsValue thisObj, JsValue[] arguments)
       {
           var o = thisObj as ObjectInstance;
           o.ValidateTypedArray(_realm);
           return CreateArrayIterator(o, ArrayIteratorType.Value);
       }

       private JsValue ToStringTag(JsValue thisObj, JsValue[] arguments)
       {
           throw new NotImplementedException();
       }

       private IteratorInstance CreateArrayIterator(ObjectInstance array, ArrayIteratorType kind)
       {
           return kind switch
           {
               ArrayIteratorType.Key => new IteratorInstance.ArrayLikeKeyIterator(_engine, array),
               ArrayIteratorType.Value => new IteratorInstance.ArrayLikeValueIterator(_engine, array),
               _ => new IteratorInstance.ArrayLikeIterator(_engine, array)
           };
       }

       private enum ArrayIteratorType
       {
           Key,
           Value,
           KeyAndValue
       }
    }
}