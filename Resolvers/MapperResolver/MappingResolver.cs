using System;
using System.Collections.Generic;
using GloryS.Common.Model;

namespace GloryS.Common.Resolvers.MapperResolver
{
    public class MappingResolver : IMappingResolver
    {
        #region Private Fields

        private readonly object _sync = new object();
        private readonly Dictionary<PairId, object> _cache = new Dictionary<PairId, object>();

        #endregion

        #region Implemetation Of IMappingResolver

        public void Register<TSource, TDist>(IPropertiesMapper<TSource, TDist> mapper)
        {
            _cache.Add(PairId.GetId<TSource, TDist>(), mapper);
        }

        public IPropertiesMapper<TSource, TDist> GetMapper<TSource, TDist>(TSource source, TDist dist)
        {
            PairId pairId = PairId.GetId<TSource, TDist>();

            IPropertiesMapper<TSource, TDist> mapper;
            object mappingObject;

            if (_cache.TryGetValue(pairId, out mappingObject))
            {
                mapper = (IPropertiesMapper<TSource, TDist>) mappingObject;
            }
            else
            {
                lock (_sync)
                {
                    if (_cache.TryGetValue(pairId, out mappingObject))
                    {
                        mapper = (IPropertiesMapper<TSource, TDist>) mappingObject;
                    }
                    else
                    {
                        if ((mapper = dist as IPropertiesMapper<TSource, TDist>) != null)
                        {
                            _cache.Add(pairId, mapper);
                        }
                        else
                        {
                            if ((mapper = source as IPropertiesMapper<TSource, TDist>) != null)
                            {
                                _cache.Add(pairId, mapper);
                            }
                            else
                            {
                                throw new NotSupportedException(
                                    String.Format("Convert expression for {0}->{1} does not exist.",
                                        typeof (TSource).FullName, typeof (TDist).FullName));
                            }
                        }
                    }
                }
            }

            return mapper;
        }

        #endregion

        #region Singleton

        private static readonly Object StaticSyncRoot = new Object();
        private static MappingResolver _instance;

        public static MappingResolver Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (StaticSyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new MappingResolver();
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion
    }
}
