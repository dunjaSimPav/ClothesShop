using Microsoft.AspNetCore.Components.Forms;
using System;

namespace ClothesShop.Components
{
    public class ExtendedInputSelect<TValue> : InputSelect<TValue>
    {
        protected override bool TryParseValueFromString(string value, out TValue result, out string validationErrorMessage)
        {
            Type t = typeof(TValue);

            if (t == typeof(int))
            {
                if (long.TryParse(value, out var resultInt))
                {
                    result = (TValue)(object)resultInt;
                    validationErrorMessage = null;
                    return true;
                }
                else
                {
                    result = default;

                    validationErrorMessage = $"The selected value {value} is not of type int.";
                    return false;
                }
            } 
            else if (t == typeof(long))
            {
                if (long.TryParse(value, out var resultLong))
                {
                    result = (TValue)(object)resultLong;
                    validationErrorMessage = null;
                    return true;
                }
                else
                {
                    result = default;

                    validationErrorMessage = $"The selected value {value} is not of type long.";
                    return false;
                }
            } 
            else
            {
                return base.TryParseValueFromString(value, out result, out validationErrorMessage);
            }
        }
    }
}
