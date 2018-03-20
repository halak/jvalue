using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Halak
{
    partial class JsonConverter
    {
        private IEnumerable<KeyValuePair<Type, TypeContract>> GetBuiltinContracts()
            => CreateBuiltinContracts().Select(it => new KeyValuePair<Type, TypeContract>(it.Type, it));

        private TypeContract[] CreateBuiltinContracts()
        {
            return new TypeContract[]
            {
                new TypeContract<bool>(v => new JValue(v)),
                new TypeContract<sbyte>(v => new JValue(v)),
                new TypeContract<byte>(v => new JValue(v)),
                new TypeContract<short>(v => new JValue(v)),
                new TypeContract<ushort>(v => new JValue(v)),
                new TypeContract<int>(v => new JValue(v)),
                new TypeContract<uint>(v => new JValue(v)),
                new TypeContract<long>(v => new JValue(v)),
                new TypeContract<ulong>(v => new JValue(v)),
                new TypeContract<float>(v => new JValue(v)),
                new TypeContract<double>(v => new JValue(v)),
                new TypeContract<decimal>(v => new JValue(v)),
                new TypeContract<string>(v => new JValue(v)),
                new TypeContract<DateTime>(v => new JValue(v.ToString(DateTimeFormat))),
                new CompositeObjectContract(this),
            };
        }
    }
}
