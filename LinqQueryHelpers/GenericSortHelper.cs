using CrossLayerHelpers.Enumerators;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;

namespace LinqQueryHelpers
{
	public class GenericSortHelper<TEntity> where TEntity : class
	{
		public static IQueryable<TEntity> GenericSort(IQueryable<TEntity> query, IDictionary<string, ESortDirection> sort)
		{
			if (sort != null && sort.Any())
			{
				var queryBuilder = new StringBuilder();

				foreach (var sortItem in sort)				
					if (!string.IsNullOrEmpty(sortItem.Key))					
						queryBuilder.Append(string.Format(" {0} {1} ", sortItem.Key, sortItem.Value.ToString().ToUpper()));

				query = queryBuilder.Length > 0 ? query.OrderBy(queryBuilder.ToString()) : query = query.OrderBy(" Id ASC ");
			}

			return query;
		}
	}
}
