using System.Collections.Generic;

namespace System
{
    public class DelegateEqualityComparer<TEntity, TKey>: EqualityComparer<TEntity>
        where TKey: IEquatable<TKey>
    {
        private readonly Func<TEntity, TKey> _keySelector;

        public DelegateEqualityComparer(Func<TEntity, TKey> keySelector)
        {
            _keySelector = keySelector;
        }

        public override bool Equals(TEntity x, TEntity y)
        {
            return _keySelector(x).Equals(_keySelector(y));
        }

        public override int GetHashCode(TEntity obj)
        {
            return _keySelector(obj).GetHashCode();
        }
    }
}
