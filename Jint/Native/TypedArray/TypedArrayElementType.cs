using System;
using Jint.Runtime;

namespace Jint.Native.TypedArray
{
    internal enum TypedArrayElementType
    {
        Int8,
        Uint8,
        Uint8C,
        Int16,
        Uint16,
        Int32,
        Uint32,
        BigInt64,
        BigUint64,
        Float32,
        Float64
    }

    internal static class TypedArrayExtensions
    {
        internal static int ToElementSize(this TypedArrayElementType type)
        {
            return type switch
            {
                TypedArrayElementType.Int8 => 1,
                TypedArrayElementType.Uint8 => 1,
                TypedArrayElementType.Uint8C => 1,
                TypedArrayElementType.Int16 => 2,
                TypedArrayElementType.Uint16 => 2,
                TypedArrayElementType.Int32 => 4,
                TypedArrayElementType.Uint32 => 4,
                TypedArrayElementType.BigInt64 => 8,
                TypedArrayElementType.BigUint64 => 8,
                TypedArrayElementType.Float32 => 4,
                TypedArrayElementType.Float64 => 8,
                _ => -1
            };
        }

        internal static Func<JsValue, double> GetConversionOperator(this TypedArrayElementType type)
        {
            return type switch
            {
                TypedArrayElementType.Int8 => value => TypeConverter.ToInt8(value),
                TypedArrayElementType.Uint8 => value => TypeConverter.ToUint8(value),
                TypedArrayElementType.Uint8C => value => TypeConverter.ToUint8Clamp(value),
                TypedArrayElementType.Int16 => value => TypeConverter.ToInt16(value),
                TypedArrayElementType.Uint16 => value => TypeConverter.ToUint16(value),
                TypedArrayElementType.Int32 => value => TypeConverter.ToInt32(value),
                TypedArrayElementType.Uint32 => value => TypeConverter.ToUint32(value),
                TypedArrayElementType.BigInt64 => value => TypeConverter.ToBigInt64(value),
                TypedArrayElementType.BigUint64 => value => TypeConverter.ToBigUint64(value),
                _ => _ =>
                {
                    ExceptionHelper.ThrowArgumentOutOfRangeException();
                    return 0;
                }
            };
        }

        internal static bool IsUnsignedElementType(this TypedArrayElementType type)
        {
            return type switch
            {
                TypedArrayElementType.Uint8 => true,
                TypedArrayElementType.Uint8C => true,
                TypedArrayElementType.Uint16 => true,
                TypedArrayElementType.Uint32 => true,
                TypedArrayElementType.BigUint64 => true,
                _ => false
            };
        }

        internal static bool IsBigIntElementType(this TypedArrayElementType type)
        {
            return type is TypedArrayElementType.BigUint64 or TypedArrayElementType.BigInt64;
        }
    }
}