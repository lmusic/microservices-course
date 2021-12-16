using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlatformService.Models;

namespace PlatformService.Data 
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProduction) 
        {
            using(var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProduction);
            }
        }
        private static void SeedData(AppDbContext context, bool isProduction)
        {

            if(isProduction)
            {
                Console.WriteLine("--> Applying migrations...");
                try
                {
                    context.Database.Migrate();
                    Console.WriteLine("--> Migrations were applyed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("--> Migrations were NOT applyed");
                    Console.WriteLine(ex.Message);
                }
            }

            if(!context.Platforms.Any())
            {
                Console.WriteLine("--> Seeding data...");
                context.AddRange(
                    new Platform{Name = "Dot net", Publisher = "Microsoft", Cost = "Free"},
                    new Platform{Name = "SQL express", Publisher = "Microsoft", Cost = "Free"},
                    new Platform{Name = "Kubernates", Publisher = "CNCF", Cost = "Free"}
                );
                context.SaveChanges();
            }
            else{
                Console.WriteLine("--> Data already exist.");
            }
        }
    }
}