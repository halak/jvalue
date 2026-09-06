// Unity 2021.2+ 는 두 API 프로필 모두 System.HashCode 를 갖고 있으면서 NET_STANDARD_2_0 이나 NET_4_6 도
// 함께 정의하므로, 그대로 두면 이 폴리필이 진짜와 충돌한다(CS0436). Unity 신버전과 .NET Standard 2.1 은 뺀다.
#if ((NET_4_6 || NET_STANDARD_2_0) || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6 || NETSTANDARD2_0 || NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETFRAMEWORK) && !NET_STANDARD_2_1 && !UNITY_2021_2_OR_NEWER
#define NETSTANDARD2_0_OR_OLDER
#endif

#if NETSTANDARD2_0_OR_OLDER
namespace System
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal struct HashCode
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
        public static int Combine(int hash1, int hash2)
        {
            // https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/Numerics/Hashing/HashHelpers.cs
            unchecked
            {
                var rol5 = ((uint)hash1 << 5) | ((uint)hash2 >> 27);
                return ((int)rol5 + hash1) ^ hash2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
        public static int Combine<T1>(T1 value1)
            => value1?.GetHashCode() ?? 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
        public static int Combine<T1, T2>(T1 value1, T2 value2)
            => Combine(value1?.GetHashCode() ?? 0, value2?.GetHashCode() ?? 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
        public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
            => Combine(Combine(value1, value2), value3?.GetHashCode() ?? 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
        public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
            => Combine(Combine(value1, value2, value3), value4?.GetHashCode() ?? 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
        public static int Combine<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
            => Combine(Combine(value1, value2, value3, value4), value5?.GetHashCode() ?? 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
        public static int Combine<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
            => Combine(Combine(value1, value2, value3, value4, value5), value6?.GetHashCode() ?? 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
        public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
            => Combine(Combine(value1, value2, value3, value4, value5, value6), value7?.GetHashCode() ?? 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
        public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
            => Combine(Combine(value1, value2, value3, value4, value5, value6, value7), value8?.GetHashCode() ?? 0);
    }
}
#endif