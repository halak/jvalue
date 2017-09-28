using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Halak
{
    [DebuggerDisplay("{ToDebuggerDisplay(),nq}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    partial struct JValue
    {
        private const int EllipsisCount = 64;

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

        [DebuggerDisplay("{value.ToDebuggerDisplay(),nq}", Name = "[{key}]")]
        private struct ObjectMember
        {
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly string key;
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            private readonly JValue value;

            public ObjectMember(string key, JValue value)
            {
                this.key = key;
                this.value = value;
            }
        }

        private sealed class DebugView
        {
			private readonly ObjectMember[] EmptyItems = new ObjectMember[0];

            private JValue value;

            public DebugView(JValue value)
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
                            return value.Array().ToArray();
                        case TypeCode.Object:
                            return value.Object().Select(it => new ObjectMember(it.Key.AsString(), it.Value)).ToArray();
                        default:
                            return null;
                    }
				}
			}
        }
    }
}
