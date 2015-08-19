using System;

namespace GloryS.Common.Resolvers.MapperResolver
{
   public interface IMappingResolver
   {
       void Register<TSource, TDist>(IPropertiesMapper<TSource, TDist> mapper);

       IPropertiesMapper<TSource, TDist> GetMapper<TSource, TDist>(TSource source, TDist dist);
   }
}
