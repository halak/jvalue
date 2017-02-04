using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Halak
{
    /// <summary>
    /// Dynamic JValue.
    /// </summary>
    public sealed class DJValue : DynamicObject, IEnumerable<KeyValuePair<object, DJValue>>
    {
        #region Static Fields
        public static readonly DJValue Null = new DJValue();
        #endregion

        #region Fields
        private readonly JValue value;
        #endregion

        #region Constructors
        public DJValue()
            : this(JValue.Null)
        {
        }

        public DJValue(string source)
            : this(JValue.Parse(source))
        {
        }

        public DJValue(JValue v)
        {
            this.value = v;
        }
        #endregion

        #region Methods
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            switch (Type.GetTypeCode(binder.Type))
            {
                case TypeCode.Object:
                    result = this;
                    return true;
                case TypeCode.Boolean:
                    result = value.AsBoolean();
                    return true;
                case TypeCode.Char:
                    string s = value.AsString();
                    result = s.Length > 0 ? s[0] : '\0';
                    return true;
                case TypeCode.SByte:
                    result = (sbyte)value.AsInt();
                    return true;
                case TypeCode.Byte:
                    result = (byte)value.AsInt();
                    return true;
                case TypeCode.Int16:
                    result = (short)value.AsInt();
                    return true;
                case TypeCode.UInt16:
                    result = (ushort)value.AsInt();
                    return true;
                case TypeCode.Int32:
                    result = value.AsInt();
                    return true;
                case TypeCode.UInt32:
                    result = (uint)value.AsLong();
                    return true;
                case TypeCode.Int64:
                    result = value.AsLong();
                    return true;
                case TypeCode.UInt64:
                    result = (ulong)value.AsLong();
                    return true;
                case TypeCode.Single:
                    result = value.AsFloat();
                    return true;
                case TypeCode.Double:
                    result = value.AsDouble();
                    return true;
                case TypeCode.Decimal:
                    decimal d;
                    if (Decimal.TryParse(value.AsString(), out d))
                        result = d;
                    else
                        result = decimal.Zero;
                    return true;
                case TypeCode.DateTime:
                    DateTime dt;
                    if (DateTime.TryParse(value.AsString(), out dt))
                        result = dt;
                    else
                        result = DateTime.Now;
                    return true;
                case TypeCode.String:
                    result = value.AsString();
                    return true;
                default:
                    result = DJValue.Null;
                    return true;
            }
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            int index = 0;
            if (indexes.Length == 1 && TryConvertToInt32(indexes[0], out index))
            {
                result = Convert(value.Get(index));
                return true;
            }
            else
            {
                result = DJValue.Null;
                return true;
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = Convert(value.Get(binder.Name));
            return true;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = this;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = this;
            return true;
        }

        private static DJValue Convert(JValue v)
        {
            if (v.Type != JValue.TypeCode.Null)
                return new DJValue(v);
            else
                return DJValue.Null;
        }

        private static bool TryConvertToInt32(object value, out int result)
        {
            try
            {
                result = System.Convert.ToInt32(value);
                return true;
            }
            catch (Exception)
            {
                result = 0;
                return false;
            }
        }

        #region Object
        public override string ToString()
        {
            switch (value.Type)
            {
                case JValue.TypeCode.Array:
                case JValue.TypeCode.Object:
                    return value.ToString();
                default:
                    return value.AsString();
            }
        }
        #endregion

        #region IEnumerable<KeyValuePair<object, DJValue>>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<object, DJValue>>)this).GetEnumerator();
        }

        public IEnumerator<KeyValuePair<object, DJValue>> GetEnumerator()
        {
            switch (value.Type)
            {
                case JValue.TypeCode.Array:
                    foreach (var item in value.IndexedArray())
                        yield return new KeyValuePair<object, DJValue>(item.Key, Convert(item.Value));
                    break;
                case JValue.TypeCode.Object:
                    foreach (var item in value.Object())
                        yield return new KeyValuePair<object, DJValue>(item.Key, Convert(item.Value));
                    break;
            }
        }
        #endregion
        #endregion
    }
}
