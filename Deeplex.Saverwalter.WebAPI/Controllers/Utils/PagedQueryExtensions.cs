// Copyright (c) 2023-2026 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    public static class PagedQueryExtensions
    {
        /// <summary>
        /// Applies search, count, sort, pagination and projection in one call.
        /// Use the async overload when building each entry requires an extra DB lookup.
        /// </summary>
        public static async Task<PagedResult<TEntry>> PagedAsync<TEntity, TEntry>(
            this IQueryable<TEntity> source,
            PagedQuery query,
            Func<string, Expression<Func<TEntity, bool>>> searchPredicate,
            Func<IQueryable<TEntity>, string, string, IQueryable<TEntity>> applySort,
            Func<TEntity, Task<TEntry>> toEntry)
        {
            source = ApplySearch(source, query.Search, searchPredicate);
            var totalCount = await source.CountAsync();
            source = applySort(source, query.SortBy ?? "", query.SortDir);
            var items = await source.Skip(query.Skip).Take(query.Take).ToListAsync();
            var entries = new List<TEntry>(items.Count);
            foreach (var item in items)
                entries.Add(await toEntry(item));
            return new PagedResult<TEntry>(entries, totalCount);
        }

        /// <summary>
        /// Overload for when each entry can be built synchronously (no extra DB lookup).
        /// </summary>
        public static async Task<PagedResult<TEntry>> PagedAsync<TEntity, TEntry>(
            this IQueryable<TEntity> source,
            PagedQuery query,
            Func<string, Expression<Func<TEntity, bool>>> searchPredicate,
            Func<IQueryable<TEntity>, string, string, IQueryable<TEntity>> applySort,
            Func<TEntity, TEntry> toEntry)
        {
            source = ApplySearch(source, query.Search, searchPredicate);
            var totalCount = await source.CountAsync();
            source = applySort(source, query.SortBy ?? "", query.SortDir);
            var items = await source.Skip(query.Skip).Take(query.Take).ToListAsync();
            return new PagedResult<TEntry>(items.Select(toEntry), totalCount);
        }

        private static IQueryable<TEntity> ApplySearch<TEntity>(
            IQueryable<TEntity> source,
            string? search,
            Func<string, Expression<Func<TEntity, bool>>> predicate)
        {
            foreach (var t in ParseTerms(search))
                source = source.Where(predicate(t));
            return source;
        }

        private static IEnumerable<string> ParseTerms(string? search) =>
            (search ?? "")
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToLower())
                .Where(t => t.Length > 0);

        /// <summary>Order by a key, direction determined by <paramref name="dir"/> ("asc" / anything else → desc).</summary>
        public static IOrderedQueryable<T> SortBy<T, TKey>(
            this IQueryable<T> source,
            Expression<Func<T, TKey>> keySelector,
            string dir) =>
            dir == "asc"
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);

        /// <summary>Secondary sort key, same direction semantics as <see cref="SortBy{T,TKey}"/>.</summary>
        public static IOrderedQueryable<T> ThenSortBy<T, TKey>(
            this IOrderedQueryable<T> source,
            Expression<Func<T, TKey>> keySelector,
            string dir) =>
            dir == "asc"
                ? source.ThenBy(keySelector)
                : source.ThenByDescending(keySelector);
    }
}
