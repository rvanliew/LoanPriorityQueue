using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanPriorityQueue
{
    public static class Extensions
    {
        public static bool EqualsAnyOf<T>(this T obj, params T[] args)
        {
            return args.Contains(obj);
        }
    }
}
