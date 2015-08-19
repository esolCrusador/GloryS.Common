using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GloryS.Common.Extensions
{
    public static class CollectionExtensions
    {
        public static void UpdateCollection<TEntity, TKey>(this ICollection<TEntity> collection, Func<IEnumerable<TKey>, List<TEntity>> getElements, Func<TEntity, TKey> keySelector, IEnumerable<TKey> keys)
        {
            var role2Remove = collection.Where(e => !keys.Contains(keySelector(e))).ToList();
            var role2Insert = getElements(keys.Where(rId => !collection.Select(keySelector).Contains(rId)));

            foreach (var role in role2Remove)
            {
                collection.Remove(role);
            }

            foreach (var role in role2Insert)
            {
                collection.Add(role);
            }
        }

        public static List<TItem> BuildHierarchy<TItem, TKey>(this IEnumerable<TItem> items,
                                                             Func<TItem, TKey> idSelector,
                                                             Func<TItem, TKey?> parentIdSelector,
                                                             Action<TItem, TItem> setParent,
                                                             Action<TItem, IEnumerable<TItem>> setChildren
    )
    where TKey : struct, IEquatable<TKey>
        {
            var groups = items.GroupBy(parentIdSelector).ToList();

            var topBranch = groups.Where(g => !g.Key.HasValue).SelectMany(g => g).ToList();
            topBranch.ForEach(i => BuildHierarchy(groups, idSelector, setParent, setChildren, i));

            return topBranch;
        }

        private static void BuildHierarchy<TItem, TKey>(this List<IGrouping<TKey?, TItem>> groups,
                                                        Func<TItem, TKey> idSelector,
                                                        Action<TItem, TItem> setParent,
                                                        Action<TItem, IEnumerable<TItem>> setChildren,
                                                        TItem parentItem)
            where TKey : struct, IEquatable<TKey>
        {
            TKey itemId = idSelector(parentItem);

            var children = groups.Where(g => g.Key.HasValue && itemId.Equals(g.Key.Value)).SelectMany(g => g).ToList();

            children.ForEach(itm =>
            {
                setParent(itm, parentItem);
                BuildHierarchy(groups, idSelector, setParent, setChildren, itm);
            });

            setChildren(parentItem, children);
        }

        public static IReadOnlyCollection<TItem> ToReadOnlyCollection<TItem>(this IEnumerable<TItem> enumerable)
        {
            return new ReadOnlyCollection<TItem>(enumerable.ToList());
        }
    }
}
