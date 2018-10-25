using CrossLayerHelpers.Enumerators;
using CrossLayerHelpers.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Dynamic.Core;
using System.Globalization;

namespace LinqQueryHelpers
{
	public class GenericFilterHelper<TEntity> where TEntity : class
	{
		public static IQueryable<TEntity> GenericFilter(IQueryable<TEntity> query, IList<Filter> filters)
		{
			try
			{
				var entityType = typeof(TEntity);

				if (filters != null && filters.Count > 0)
				{
					var queryBuilder = new StringBuilder();

					foreach (var filterItem in filters)
					{
						if (!string.IsNullOrEmpty(filterItem.Field))
						{
							foreach (var property in entityType.GetProperties())
							{
								string propertyType = GetPropertyType(filterItem, property);

								if (property.Name.ToLower().Equals(filterItem.Field.ToLower().Split('.').Count() > 1 ? filterItem.Field.ToLower().Split('.')[0] : filterItem.Field.ToLower()))
								{
									AddLogicOperator(queryBuilder, filterItem.LogicOperator);

									if (string.IsNullOrEmpty(filterItem.Value) || filterItem.Value.ToLower() == "null")
										queryBuilder.Append(QueryConstructor(filterItem.Field, " = ", filterItem.Value));
									else
									{
										switch (propertyType)
										{
											case nameof(String):
											case nameof(Char):
												queryBuilder.Append(QueryForString(filterItem));
												break;

											case nameof(Boolean):
												bool isBoolean;
												if (Boolean.TryParse(filterItem.Value, out isBoolean))
													queryBuilder.Append(QueryForBoolean(filterItem));
												break;

											case nameof(Int16):
											case nameof(Int32):
											case nameof(Int64):
												int isInt;
												if (int.TryParse(filterItem.Value, out isInt))
													queryBuilder.Append(QueryForInteger(filterItem));
												break;

											case nameof(DateTime):
												DateTime isDateTime;
												if (DateTime.TryParse(filterItem.Value, out isDateTime))
													queryBuilder.Append(QueryForDateTime(filterItem));
												break;

											case nameof(Guid):
												Guid isGuid;
												if (Guid.TryParse(filterItem.Value, out isGuid))
													queryBuilder.Append(QueryForGuid(filterItem));
												break;

											default:
												queryBuilder.Append(QueryConstructor(filterItem.Field, " = ", filterItem.Value));
												break;
										}
									}
								}
							}
						}
					}

					if (queryBuilder.Length > 0)
						query = query.Where(queryBuilder.ToString());
				}
			}
			catch { }

			return query;
		}

		#region Privates
		private static string GetPropertyType(Filter filterItem, PropertyInfo property)
		{
			var propertyType = string.Empty;

			if (filterItem.Field.ToLower().Split('.').Count() > 1)
			{
				var teste = filterItem.Field.ToLower().Split('.');

				if (property.Name.ToLower().Equals(teste[0].ToLower()))
				{
					var childObjectType = property.PropertyType;

					var childObjectProperty = childObjectType.GetProperties();

					foreach (var propertyInfo in childObjectProperty)
					{
						if (propertyInfo.Name.ToLower().Equals(teste[1].ToLower()))
						{
							propertyType = propertyInfo.PropertyType.Name;
						}
					}
				}
			}
			else
			{
				propertyType = property.PropertyType.Name;
			}

			return propertyType;
		}

		private static void AddLogicOperator(StringBuilder queryBuilder, ELogicOperator logicOperator)
		{
			if (!string.IsNullOrEmpty(queryBuilder.ToString()))
				switch (logicOperator)
				{
					case ELogicOperator.And:
						queryBuilder.Append(" AND ");
						break;
					case ELogicOperator.Or:
						queryBuilder.Append(" OR ");
						break;
					default:
						break;
				}
		}

		private static string QueryConstructor(string field, string compareOperator, string value)
		{
			return string.Format(" {0}{1}{2} ", field, compareOperator, value);
		}

		private static string QueryForString(Filter filterItem)
		{
			var query = string.Empty;

			switch (filterItem.CompareOperator)
			{
				case ECompareOperator.Like:
					query = QueryConstructor(filterItem.Field, ".Contains", string.Format("(\"{0}\")", filterItem.Value));
					break;
				case ECompareOperator.NotLike:
					query = QueryConstructor(string.Format("!{0}", filterItem.Field), ".Contains", string.Format("(\"{0}\")", filterItem.Value));
					break;
				case ECompareOperator.NotEqual:
					query = QueryConstructor(filterItem.Field, ".Equals", string.Format("(\"{0}\")", filterItem.Value));
					break;
				default:
					query = QueryConstructor(string.Format("!{0}", filterItem.Field), ".Equals", string.Format("(\"{0}\")", filterItem.Value));
					break;
			}

			return query;
		}

		private static string QueryForBoolean(Filter filterItem)
		{
			var query = string.Empty;

			switch (filterItem.CompareOperator)
			{
				case ECompareOperator.NotEqual:
					query = QueryConstructor(filterItem.Field, "<>", string.Format(" cast('{0}' as bit) ", filterItem.Value));
					break;
				default:
					query = QueryConstructor(filterItem.Field, "=", string.Format(" cast('{0}' as bit) ", filterItem.Value));
					break;
			}

			return query;
		}

		private static string QueryForInteger(Filter filterItem)
		{
			var query = string.Empty;

			switch (filterItem.CompareOperator)
			{
				case ECompareOperator.GreaterThan:
					query = QueryConstructor(filterItem.Field, ">", filterItem.Value);
					break;
				case ECompareOperator.GreaterThanOrEqual:
					query = QueryConstructor(filterItem.Field, ">=", filterItem.Value);
					break;
				case ECompareOperator.LessThan:
					query = QueryConstructor(filterItem.Field, "<", filterItem.Value);
					break;
				case ECompareOperator.LessThanOrEqual:
					query = QueryConstructor(filterItem.Field, "<=", filterItem.Value);
					break;
				case ECompareOperator.NotEqual:
					query = QueryConstructor(filterItem.Field, "<>", filterItem.Value);
					break;
				default:
					query = QueryConstructor(filterItem.Field, "=", filterItem.Value);
					break;
			}

			return query;
		}

		private static string QueryForDateTime(Filter filterItem)
		{
			var query = string.Empty;

			switch (filterItem.CompareOperator)
			{
				case ECompareOperator.GreaterThan:
					query = QueryConstructor(filterItem.Field, ">", string.Format(" DateTime({0}, 0, 0, 0) ", Convert.ToDateTime(filterItem.Value).ToString("yyyy, MM, dd", CultureInfo.InvariantCulture)));
					break;
				case ECompareOperator.GreaterThanOrEqual:
					query = QueryConstructor(filterItem.Field, ">=", string.Format(" DateTime({0}, 0, 0, 0) ", Convert.ToDateTime(filterItem.Value).ToString("yyyy, MM, dd", CultureInfo.InvariantCulture)));
					break;
				case ECompareOperator.LessThan:
					query = QueryConstructor(filterItem.Field, "<", string.Format(" DateTime({0}, 23, 59, 59) ", Convert.ToDateTime(filterItem.Value).ToString("yyyy, MM, dd", CultureInfo.InvariantCulture)));
					break;
				case ECompareOperator.LessThanOrEqual:
					query = QueryConstructor(filterItem.Field, "<=", string.Format(" DateTime({0}, 23, 59, 59) ", Convert.ToDateTime(filterItem.Value).ToString("yyyy, MM, dd", CultureInfo.InvariantCulture)));
					break;
				case ECompareOperator.NotEqual:
					query = QueryConstructor(filterItem.Field, "<>", string.Format(" DateTime({0}) ", Convert.ToDateTime(filterItem.Value).ToString("yyyy, MM, dd", CultureInfo.InvariantCulture)));
					break;
				default:
					query = QueryConstructor(filterItem.Field, "=", string.Format(" DateTime({0}) ", Convert.ToDateTime(filterItem.Value).ToString("yyyy, MM, dd", CultureInfo.InvariantCulture)));
					break;
			}

			return query;
		}

		private static string QueryForGuid(Filter filterItem)
		{
			return filterItem.CompareOperator == ECompareOperator.NotEqual ?
					QueryConstructor(filterItem.Field, "<>", $"Guid(\"{ filterItem.Value }\")") :
					QueryConstructor(filterItem.Field, "==", $"Guid(\"{ filterItem.Value }\")");
		}
		#endregion
	}
}
