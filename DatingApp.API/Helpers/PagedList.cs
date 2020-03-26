using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Helpers
{
    // class paging for list users
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            // TotalPages = or > ( count / pageSize )
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        // this method return new class PagedList
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            // count of list source 
            var count = await source.CountAsync();
            // items that return on 
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            // return pageDlist with item & count & pageNumber & PageSize
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }



    }

}