using System.Diagnostics;
using System.Linq;

namespace Halak
{
    [DebuggerDisplay("{ToDebuggerDisplay(),nq}", Type = "{ToDebuggerType(),nq}")]
    [DebuggerTypeProxy(typeof(JValueDebugView))]
    partial struct JValue
    {
        private const int EllipsisCount = 64;

        private string ToDebuggerType()
        {
            switch (Type)
            {
                case TypeCode.Null: return "JValue.Null";
                case TypeCode.Boolean: return "JValue.Boolean";
                case TypeCode.Number: return "JValue.Number";
                case TypeCode.String: return "JValue.String";
                case TypeCode.Array: return "JValue.Array";
                case TypeCode.Object: return "JValue.Object";
                default: return "JValue.Null";
            }
        }

        private string ToDebuggerDisplay()
        {
            switch (Type)
            {
                case TypeCode.Array:
                case TypeCode.Object:
                    var serialized = Serialize(0);
                    if (serialized.Length > EllipsisCount)
                        serialized = serialized.Substring(0, EllipsisCount - 3) + "...";
                    var elementCount = GetElementCount();
                    return string.Format("{0} ({1} {2})", serialized, elementCount.ToString(), elementCount != 1 ? "items" : "item");
                default:
                    return ToString();
            }
        }

        [DebuggerDisplay("{valueString,nq}", Type = "{valueTypeString,nq}")]
        private struct ArrayElement
        {
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public readonly JValue value;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public readonly string valueString;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public readonly string valueTypeString;

            public ArrayElement(JValue value)
            {
                this.value = value;
                this.valueString = value.ToDebuggerDisplay();
                this.valueTypeString = value.ToDebuggerType();
            }
        }

        [DebuggerDisplay("{valueString,nq}", Name = "[{key,nq}]", Type = "{valueTypeString,nq}")]
        private struct ObjectMember
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public readonly string key;
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public readonly JValue value;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public readonly string valueString;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public readonly string valueTypeString;

            public ObjectMember(string key, JValue value)
            {
                this.key = key;
                this.value = value;
                this.valueString = value.ToDebuggerDisplay();
                this.valueTypeString = value.ToDebuggerType();
            }
        }

        private sealed class JValueDebugView
        {
            private readonly JValue value;

            public JValueDebugView(JValue value)
            {
                this.value = value;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object Items
            {
                get
                {
                    switch (value.Type)
                    {
                        case TypeCode.Null:
                        case TypeCode.Boolean:
                        case TypeCode.Number:
                        case TypeCode.String:
                            return null;
                        case TypeCode.Array:
                            return value.Array().Select(it => new ArrayElement(it)).ToArray();
                        case TypeCode.Object:
                            return value.Object().Select(it => new ObjectMember(it.Key.ToString(), it.Value)).ToArray();
                        default:
                            return null;
                    }
                }
            }
        }
    }
}
