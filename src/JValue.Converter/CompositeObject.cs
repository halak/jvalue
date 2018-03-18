using System;
using System.Collections.Generic;

namespace Halak
{
    public struct CompositeObject
    {
        public IList<object> Elements { get; }

        public CompositeObject(params object[] elements)
        {
            this.Elements = elements;
        }
    }
}
