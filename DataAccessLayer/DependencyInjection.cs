using DataTransferObject.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DataAccessLayer
{
    public static class DependencyInjection
    {


        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
           


            return services;
        }
    }
}
