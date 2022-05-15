using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using API.Helpers;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, int currentPage,
            int itemsPerPage, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

            //when you add your custom header to the http response - you MUST add CORS header to make ur custom header available
            var options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; //in order to get json object with camelCase properties

            response.Headers.Add("Pagination", JsonSerializer.Serialize<PaginationHeader>(paginationHeader, options));

            //you CANNOT specify random name here - only EXACT same strings as below
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
    }
}