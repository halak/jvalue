using System;
using BenchmarkDotNet.Running;

namespace Halak
{
    class Program
    {
        static void Main(string[] args)
        {
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
