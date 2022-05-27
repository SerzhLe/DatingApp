using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public PagedList(IEnumerable<T> items, int pageNumber, int pageSize, int count)
        {
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalCount = count;
            TotalPages = (int)Math.Ceiling((double)(count / pageSize)); //e.g. 22 / 5 = 4,.. - return 5
            AddRange(items); //add items to the collection (PagedList)
        }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();

            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedList<T>(items, pageNumber, pageSize, count);
        }
    }
}