using Jint.Native.Object;
using Jint.Runtime;

namespace Jint.Native.TypedArray
{
    internal static class TypeArrayHelper
    {
        internal static void ValidateTypedArray(this ObjectInstance o, Realm realm)
        {
            var typedArrayInstance = o as TypedArrayInstance;
            if (typedArrayInstance is null)
            {
                ExceptionHelper.ThrowTypeError(realm);
            }

            var buffer = typedArrayInstance._viewedArrayBuffer;
            if (buffer.IsDetachedBuffer)
            {
                ExceptionHelper.ThrowTypeError(realm);
            }
        }
    }
}