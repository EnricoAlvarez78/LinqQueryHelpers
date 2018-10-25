using CrossLayerHelpers.Enumerators;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;

namespace LinqQueryHelpers
{
	public class GenericSortHelper<TEntity> where TEntity : class
	{
		public static IQueryable<TEntity> GenericSort(IQueryable<TEntity> query, IDictionary<ESortDirection, string> sort)
		{
			if (sort != null && sort.Any())
			{
				var queryBuilder = new StringBuilder();

				foreach (var sortItem in sort)
				{
					if (!string.IsNullOrEmpty(sortItem.Value))
					{
						queryBuilder.Append(string.Format(" {0} {1} ", sortItem.Value, sortItem.Key.ToString().ToUpper()));
					}
				}

				if (queryBuilder.Length > 0)
					query = query.OrderBy(queryBuilder.ToString());
				else
					query = query.OrderBy(" Id ASC ");
			}

			return query;
		}
	}
}
