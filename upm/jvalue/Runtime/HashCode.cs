#if (!NETCOREAPP2_0 && !NETCOREAPP2_1 && !NETCOREAPP2_2 && !NETCOREAPP3_0)
namespace System
{
    using System.Runtime.CompilerServices;

    internal struct HashCode
    {
        public static int Combine<T1>(T1 value1)
            => value1?.GetHashCode() ?? 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Combine(int hash1, int hash2)
        {
            // https://github.com/dotnet/coreclr/blob/master/src/System.Private.CoreLib/shared/System/Numerics/Hashing/HashHelpers.cs
            unchecked
            {
                var rol5 = ((uint)hash1 << 5) | ((uint)hash2 >> 27);
                return ((int)rol5 + hash1) ^ hash2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Combine<T1, T2>(T1 value1, T2 value2)
            => Combine(value1?.GetHashCode() ?? 0, value2?.GetHashCode() ?? 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
            => Combine(Combine(value1, value2), value3?.GetHashCode() ?? 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
            => Combine(Combine(value1, value2, value3), value4?.GetHashCode() ?? 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Combine<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
            => Combine(Combine(value1, value2, value3, value4), value5?.GetHashCode() ?? 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Combine<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
            => Combine(Combine(value1, value2, value3, value4, value5), value6?.GetHashCode() ?? 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
            => Combine(Combine(value1, value2, value3, value4, value5, value6), value7?.GetHashCode() ?? 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
            => Combine(Combine(value1, value2, value3, value4, value5, value6, value7), value8?.GetHashCode() ?? 0);
    }
}
#endif
