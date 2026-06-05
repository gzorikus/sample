using System.Runtime.CompilerServices;

namespace YourCompany.CompilerServices
{
    public static class StructHelper
    {
        public static bool IsZeroed<T>(T value) where T : struct
        {
            unsafe
            {
                int size = Unsafe.SizeOf<T>();
                byte* ptr = (byte*)Unsafe.AsPointer(ref value);
                for (int i = 0; i < size; i++) if (ptr[i] != 0) return false;
                return true;
            }
        }
    }
}