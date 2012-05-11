using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data.Objects;

namespace WKSATournament.Extensions
{
    public static class JQGridExtensions
    {
        public static string CreateJQGridFilter<T>(this IEnumerable<T> items, string idName, string descriptionName)
        {
            StringBuilder filter = new StringBuilder(":All");

            foreach (Object item in items)
            {
                filter.AppendFormat(";{0}:{1}", item.GetType().GetProperty(idName).GetValue(item, null), item.GetType().GetProperty(descriptionName).GetValue(item, null));
            }

            return filter.ToString();
        }
    }
}