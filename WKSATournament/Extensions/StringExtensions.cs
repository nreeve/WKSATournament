using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WKSATournament.Extensions
{
    public static class StringExtensions
    {
        public static string DecimalToString(this decimal? input, string format)
        {
            return input.HasValue ? input.Value.ToString(format): string.Empty;
        }
    }
}