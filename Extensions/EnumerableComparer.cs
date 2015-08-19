using System.Collections.Generic;
using System.Linq;

namespace GloryS.Common.Extensions
{
    public class EnumerableComparer<TMember>:IEqualityComparer<IEnumerable<TMember>>
    {
        #region Implementation of IEqualityComparer<in IEnumerable<TMember>>

        public bool Equals(IEnumerable<TMember> x, IEnumerable<TMember> y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(IEnumerable<TMember> obj)
        {
            return obj.Aggregate(0, (r, m) => r ^ m.GetHashCode());
        }

        #endregion

        public static readonly EnumerableComparer<TMember> Default = new EnumerableComparer<TMember>();
    }
}
