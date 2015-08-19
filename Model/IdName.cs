using System;
using System.Collections.Generic;
using System.Globalization;

namespace GloryS.Common.Model
{
    public class IdNameBase<TKey> : IEqualityComparer<IdNameBase<TKey>>, IEquatable<IdNameBase<TKey>>
    {
        public IdNameBase()
        {
            
        }

        public IdNameBase(TKey id, string name)
        {
            Id = id;
            Name = name;
        }

        public TKey Id { get; set; }

        public string Name { get; set; }

        public bool Equals(IdNameBase<TKey> x, IdNameBase<TKey> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return EqualityComparer<TKey>.Default.Equals(x.Id, y.Id) && string.Equals(x.Name, y.Name);
        }

        public int GetHashCode(IdNameBase<TKey> obj)
        {
            unchecked
            {
                return (EqualityComparer<TKey>.Default.GetHashCode(obj.Id) * 397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
            }
        }

        public bool Equals(IdNameBase<TKey> other)
        {
            return Equals(this, other);
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        public override bool Equals(object obj)
        {
            return Equals((IdNameBase<TKey>) obj);
        }
    }

    public class IdName<TKey> : IdNameBase<TKey>, IEqualityComparer<IdName<TKey>>, IEquatable<IdName<TKey>>
        where TKey: IEquatable<TKey>
    {
        public IdName()
        {
            
        }

        public IdName(TKey id, string name)
            :base(id, name)
        {
            
        }

        #region Implementation of IEqualityComparer<IdName<TKey>>, IEquatable<IdName<TKey>>

        public bool Equals(IdName<TKey> x, IdName<TKey> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return EqualityComparer<TKey>.Default.Equals(x.Id, y.Id) && string.Equals(x.Name, y.Name);
        }

        public int GetHashCode(IdName<TKey> obj)
        {
            unchecked
            {
                return (EqualityComparer<TKey>.Default.GetHashCode(obj.Id) * 397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
            }
        }

        public bool Equals(IdName<TKey> other)
        {
            return Equals(this, other);
        }

        #endregion
    }

    public class CultureName : IdNameBase<Culture>, IEqualityComparer<CultureName>, IEquatable<CultureName>
    {
        public CultureName()
        {

        }

        public CultureName(Culture id, string name)
            : base(id, name)
        {

        }

        #region Implementation of IEqualityComparer<IdName<TKey>>, IEquatable<IdName<TKey>>

        public bool Equals(CultureName x, CultureName y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return EqualityComparer<Culture>.Default.Equals(x.Id, y.Id) && string.Equals(x.Name, y.Name);
        }

        public int GetHashCode(CultureName obj)
        {
            unchecked
            {
                return (EqualityComparer<Culture>.Default.GetHashCode(obj.Id) * 397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
            }
        }

        public bool Equals(CultureName other)
        {
            return Equals(this, other);
        }

        #endregion
    }

    public class IdName : IdName<int>
    {
        public IdName()
        {
            
        }

        public IdName(int id, string name)
            :base(id, name)
        {
            
        }
    }

    public class IdNameNullable<TKey> : IdNameBase<TKey?>, IEqualityComparer<IdNameNullable<TKey>>, IEquatable<IdNameNullable<TKey>>
    where TKey : struct, IEquatable<TKey>
    {
        public IdNameNullable()
        {

        }

        public IdNameNullable(TKey? id, string name)
            : base(id, name)
        {

        }

        #region Implementation of IEqualityComparer<IdName<TKey>>, IEquatable<IdName<TKey>>

        public bool Equals(IdNameNullable<TKey> x, IdNameNullable<TKey> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;

            return EqualityComparer<TKey?>.Default.Equals(x.Id, y.Id) && string.Equals(x.Name, y.Name);
        }

        public int GetHashCode(IdNameNullable<TKey> obj)
        {
            unchecked
            {
                return (EqualityComparer<TKey?>.Default.GetHashCode(obj.Id) * 397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
            }
        }

        public bool Equals(IdNameNullable<TKey> other)
        {
            return Equals(this, other);
        }

        #endregion
    }

    public class IdNameNullable : IdNameNullable<int>
    {
        public IdNameNullable()
        {
        }

        public IdNameNullable(int? id, string name) : base(id, name)
        {
        }
    }
}
