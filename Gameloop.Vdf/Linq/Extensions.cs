using Gameloop.Vdf.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Gameloop.Vdf.Linq
{
    public static class Extensions
    {
        public static U Value<U>(this IEnumerable<VToken> value)
        {
            return value.Value<VToken, U>();
        }

        public static U Value<T, U>(this IEnumerable<T> value) where T : VToken
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));

            if (!(value is VToken token))
                throw new ArgumentException("Source value must be a JToken.");

            return token.Convert<VToken, U>();
        }

        internal static U Convert<T, U>(this T token) where T : VToken
        {
            if (token == null)
                return default(U);

            if (token is U
                // don't want to cast JValue to its interfaces, want to get the internal value
                && typeof(U) != typeof(IComparable) && typeof(U) != typeof(IFormattable))
            {
                // HACK (!)
                return (U)(object)token;
            }
            else
            {
                VValue? value = token as VValue;
                if (value == null)
                    throw new InvalidCastException($"Cannot cast {token.GetType()} to {typeof(T)}.");

                if (value.Value is U u)
                    return u;

                Type targetType = typeof(U);

                if (ReflectionUtils.IsNullableType(targetType))
                {
                    if (value.Value == null)
                        return default(U);

                    targetType = Nullable.GetUnderlyingType(targetType);
                }

                if (TryConvertVdf<U>(value.Value, out U resultObj))
                    return resultObj;

                return (U) System.Convert.ChangeType(value.Value, targetType, CultureInfo.InvariantCulture);
            }
        }

        private static bool TryConvertVdf<U>(object value, out U result)
        {
            result = default(U);

            // It won't be null at this point, so just handle the nullable type.
            if ((typeof(U) == typeof(bool) || Nullable.GetUnderlyingType(typeof(U)) == typeof(bool)) && value is string valueString)
            {
                switch (valueString)
                {
                    case "1":
                        result = (U) (object) true;
                        return true;

                    case "0":
                        result = (U) (object) false;
                        return true;
                }
            }

            return false;
        }

        public static VProperty ReadProperty(this VdfReader reader)
        {
            if (reader.Value is null || reader.CurrentState != VdfReader.State.Property)
                throw new VdfException("Property key is not present.");

            string? key = reader.Value as string;
            if (key is null)
                throw new VdfException("Property key is invalid.");

            if (!reader.ReadToken())
                throw new VdfException("Incomplete VDF data.");

            // For now, we discard these comments.
            while (reader.CurrentState == VdfReader.State.Comment)
                if (!reader.ReadToken())
                    throw new VdfException("Incomplete VDF data.");


            if (reader.CurrentState == VdfReader.State.Property)
                return new(key, new VValue(reader.Value));

            return new(key, ReadObject(reader));
        }

        public static VObject ReadObject(this VdfReader reader)
        {
            VObject result = new VObject();

            if (!reader.ReadToken())
                throw new VdfException("Incomplete VDF data.");

            while (!(reader.CurrentState == VdfReader.State.Object && reader.Value as string == VdfStructure.ObjectEnd.ToString()))
            {
                if (reader.CurrentState == VdfReader.State.Comment)
                    result.Add(VValue.CreateComment(reader.Value as string ?? throw new VdfException("Invalid comment")));
                else
                    result.Add(ReadProperty(reader));

                if (!reader.ReadToken())
                    throw new VdfException("Incomplete VDF data.");
            }

            return result;
        }
    }
}
