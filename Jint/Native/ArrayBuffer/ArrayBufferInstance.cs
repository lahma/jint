using System;
using System.Collections.Generic;
using System.Linq;
using Jint.Native.Object;
using Jint.Native.TypedArray;
using Jint.Runtime;

namespace Jint.Native.ArrayBuffer
{
    public class ArrayBufferInstance : ObjectInstance
    {
        internal byte[] _arrayBufferData;
        internal uint _arrayBufferByteLength;
        private JsValue _arrayBufferDetachKey = Undefined;

        internal ArrayBufferInstance(
            Engine engine,
            uint byteLength) : base(engine)
        {
            var block = CreateByteDataBlock(byteLength);
            _arrayBufferData = block;
            _arrayBufferByteLength = byteLength;
        }

        private byte[] CreateByteDataBlock(uint byteLength)
        {
            return new byte[byteLength];
        }

        internal bool IsDetachedBuffer => _arrayBufferData is null;

        internal bool IsSharedArrayBuffer => false; // TODO

        internal void DetachArrayBuffer(JsValue key = null)
        {
            key ??= Undefined;

            if (!SameValue(_arrayBufferDetachKey, key))
            {
                ExceptionHelper.ThrowTypeError(_engine.Realm);
            }

            _arrayBufferData = null;
            _arrayBufferByteLength = 0;
        }

        internal JsValue GetValueFromBuffer(uint byteIndex, TypedArrayElementType type, bool isTypedArray, BufferOrder order, bool? isLittleEndian = null)
        {
            var block = _arrayBufferData;
            var elementSize = type.ToElementSize();
            List<byte> rawValue;
            if (IsSharedArrayBuffer)
            {
                /*
                    Let execution be the [[CandidateExecution]] field of the surrounding agent's Agent EsprimaExtensions.Record.
                    b. Let eventList be the [[EventList]] field of the element in execution.[[EventsRecords]] whose [[AgentSignifier]] is AgentSignifier().
                    c. If isTypedArray is true and IsNoTearConfiguration(type, order) is true, let noTear be true; otherwise let noTear be false.
                    d. Let rawValue be a List of length elementSize whose elements are nondeterministically chosen byte values.
                    e. NOTE: In implementations, rawValue is the result of a non-atomic or atomic read instruction on the underlying hardware. The nondeterminism is a semantic prescription of the memory model to describe observable behaviour of hardware with weak consistency.
                    f. Let readEvent be ReadSharedMemory { [[Order]]: order, [[NoTear]]: noTear, [[Block]]: block, [[ByteIndex]]: byteIndex, [[ElementSize]]: elementSize }.
                    g. Append readEvent to eventList.
                    h. Append Chosen Value EsprimaExtensions.Record { [[Event]]: readEvent, [[ChosenValue]]: rawValue } to execution.[[ChosenValues]].
                */
                throw new NotImplementedException("SharedArrayBuffer not implemented");
            }
            else
            {
                rawValue = new List<byte>(block.Skip((int)byteIndex).Take((int)(byteIndex + elementSize)));
            }

            // If isLittleEndian is not present, set isLittleEndian to the value of the [[LittleEndian]] field of the surrounding agent's Agent Record.

            return RawBytesToNumeric(type, rawValue, isLittleEndian ?? false);
        }

        private JsValue RawBytesToNumeric(TypedArrayElementType type, List<byte> rawBytes, bool isLittleEndian)
        {
            var elementSize = type.ToElementSize();
            if (!isLittleEndian)
            {
                rawBytes.Reverse();
            }

            if (type == TypedArrayElementType.Float32)
            {
                double value = -1; // TODO rawBytes concatenated and interpreted as a little-endian bit string encoding of an IEEE 754-2019 binary32 value.
                // If value is an IEEE 754-2019 binary32 NaN value, return the NaN Number value.
                if (value == double.NaN)
                {
                    return JsNumber.DoubleNaN;
                }

                return JsNumber.Create(value);
            }

            if (type == TypedArrayElementType.Float64)
            {
                double value = -1; // TODO rawBytes concatenated and interpreted as a little-endian bit string encoding of an IEEE 754-2019 binary64 value.
                // If value is an IEEE 754-2019 binary64 NaN value, return the NaN Number value.
                if (value == double.NaN)
                {
                    return JsNumber.DoubleNaN;
                }

                return JsNumber.Create(value);
            }

            JsNumber intValue;
            if (type.IsUnsignedElementType())
            {
                intValue = (JsNumber)(-1); // TODO be the byte elements of rawBytes concatenated and interpreted as a bit string encoding of an unsigned little-endian binary number.
            }
            else
            {
                intValue = (JsNumber)(-1); // TODO be the byte elements of rawBytes concatenated and interpreted as a bit string encoding of a binary little-endian two's complement number of bit length elementSize × 8.
            }

            if (type.IsBigIntElementType())
            {
                // return the BigInt value that corresponds to intValue
                throw new NotImplementedException("BigInt not implemented");
            }

            return intValue;
        }

        internal void SetValueInBuffer(uint byteIndex, TypedArrayElementType type, double value, bool isTypedArray, BufferOrder order, bool isLittleEndian)
        {
            var block = _arrayBufferData;
            var elementSize = type.ToElementSize();
            // If isLittleEndian is not present, set isLittleEndian to the value of the [[LittleEndian]] field of the surrounding agent's Agent Record.

            var rawBytes = NumericToRawBytes(type, value, isLittleEndian);
            if (IsSharedArrayBuffer)
            {
                /*
                a. Let execution be the [[CandidateExecution]] field of the surrounding agent's Agent Record.
                b. Let eventList be the [[EventList]] field of the element in execution.[[EventsRecords]] whose [[AgentSignifier]] is AgentSignifier().
                c. If isTypedArray is true and IsNoTearConfiguration(type, order) is true, let noTear be true; otherwise let noTear be false.
                d. Append WriteSharedMemory { [[Order]]: order, [[NoTear]]: noTear, [[Block]]: block, [[ByteIndex]]: byteIndex, [[ElementSize]]: elementSize, [[Payload]]: rawBytes } to eventList.
                */
                throw new NotImplementedException("SharedArrayBuffer not implemented");
            }
            else
            {
                rawBytes.CopyTo(block, (int)byteIndex);
            }
        }

        private List<byte> NumericToRawBytes(TypedArrayElementType type, double value, bool isLittleEndian)
        {
            List<byte> rawBytes;
            if (type == TypedArrayElementType.Float32)
            {
                // Let rawBytes be a List whose elements are the 4 bytes that are the result of converting value to IEEE 754-2019 binary32 format using roundTiesToEven mode. If isLittleEndian is false, the bytes are arranged in big endian order. Otherwise, the bytes are arranged in little endian order. If value is NaN, rawBytes may be set to any implementation chosen IEEE 754-2019 binary32 format Not-a-Number encoding. An implementation must always choose the same encoding for each implementation distinguishable NaN value.
                rawBytes = new List<byte>();
            }
            else if (type == TypedArrayElementType.Float64)
            {
                // Let rawBytes be a List whose elements are the 8 bytes that are the IEEE 754-2019 binary64 format encoding of value. If isLittleEndian is false, the bytes are arranged in big endian order. Otherwise, the bytes are arranged in little endian order. If value is NaN, rawBytes may be set to any implementation chosen IEEE 754-2019 binary64 format Not-a-Number encoding. An implementation must always choose the same encoding for each implementation distinguishable NaN value.
                rawBytes = new List<byte>();
            }
            else
            {
                var n = type.ToElementSize();
                var convOp = type.GetConversionOperator();
                var intValue = (int) convOp(value);
                if (intValue >= 0)
                {
                    // i. Let rawBytes be a List whose elements are the n-byte binary encoding of intValue. If isLittleEndian is false, the bytes are ordered in big endian order. Otherwise, the bytes are ordered in little endian order.
                    rawBytes = new List<byte>();
                }
                else
                {
                    // i. Let rawBytes be a List whose elements are the n-byte binary two's complement encoding of intValue. If isLittleEndian is false, the bytes are ordered in big endian order. Otherwise, the bytes are ordered in little endian order.
                    rawBytes = new List<byte>();
                }

            }
            return rawBytes;
        }

        internal enum BufferOrder
        {
            Init,
            SecCst,
            Unordered
        }
    }
}