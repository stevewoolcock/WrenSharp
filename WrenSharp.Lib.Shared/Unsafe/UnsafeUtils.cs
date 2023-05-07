namespace WrenSharp.Unsafe
{
    internal class UnsafeUtils
    {
        public unsafe static int Memcmp(void* str1, void* str2, int count)
        {
            byte* s1 = (byte*)str1;
            byte* s2 = (byte*)str2;
            while (count-- > 0)
            {
                if (*s1++ != *s2++)
                    return s1[-1] < s2[-1] ? -1 : 1;
            }

            return 0;
        }

        public unsafe static bool Memeq(void* str1, void* str2, int count)
        {
            if (str1 == str2)
                return true;

            byte* s1 = (byte*)str1;
            byte* s2 = (byte*)str2;
            while (count-- > 0)
            {
                if (*s1++ != *s2++)
                    return false;
            }

            return true;
        }
    }
}
