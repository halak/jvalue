using System;
using BenchmarkDotNet.Running;

namespace Halak
{
    class Program
    {
        static void Main(string[] args)
        {
            void AreEqual(int expected, string s)
            {
                var actual = JsonHelper.ParseInt32NewNew(s, 0);
                if (actual != expected)
                {
                    Console.WriteLine($"expected: {expected}, actual: {actual}");
                }
            }

            AreEqual(1, "123e-2");
            AreEqual(0, "123e+100");
            AreEqual(1230000, "123e+4");
            AreEqual(100000000, "1.0e8");
            AreEqual(100000000, "1.0e+8");
            AreEqual(100000000, "1e8");

            AreEqual(10000, "10000");
            AreEqual(0, "4294967295");  // overflow
            AreEqual(0, "2147483648");  // overflow
            AreEqual(2147483647, "2147483647");  // max
            AreEqual(0, "12387cs831");  // invalid
            AreEqual(0, "0");
            AreEqual(-12938723, "-12938723");
            AreEqual(3948222, "3948222");
            AreEqual(int.MinValue, int.MinValue.ToString());
            AreEqual(int.MaxValue, int.MaxValue.ToString());
            AreEqual(0, (int.MinValue - 1L).ToString());
            AreEqual(0, (int.MaxValue + 1L).ToString());




            var switcher = new BenchmarkSwitcher(new[] {
                typeof(ParseInt32Benchmark),
                typeof(ParseSingleBenchmark),
                typeof(ParseDoubleBenchmark),
                typeof(SerializeObjectBenchmark),
            });
            switcher.Run(args);
        }
    }
}

// 불변 클래스에서 자식간의 상호 참조를 하는 경우를 스마트하게 해결해보기 위해 다음과 같은 프로토타이핑을 해보았습니다.
// 추후 ToObject를 구현할 때 사용해봅시다.
/*
public class BaseClass
{
    public OtherChildClass otherChild;

    protected BaseClass(Func<BaseClass, OtherChildClass> otherChild)
    {
        this.otherChild = otherChild(this);
    }

    protected BaseClass()
    {
    }

    public class OtherChildClass
    {
        public BaseClass baseClass;

        public OtherChildClass()
        {
        }
    }
}

public sealed class ParentClass : BaseClass
{
    public int x;
    public int y;
    public int z;
    public ChildClass child;

    public ParentClass(Func<BaseClass, OtherChildClass> otherChild, int x, int y, int z, Func<ParentClass, ChildClass> child)
        : base(otherChild)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.child = child(this);
    }

    public ParentClass(int x, int y, int z, ChildClass child)
        : this(_ => null, x, y, z, _ => child)
    {
    }

    public sealed class ChildClass
    {
        public ChildClass(ParentClass parent)
        {
        }
    }
}
*/
