namespace Chess.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ChessDbContextSeeder : ISeeder
    {
        public async Task SeedAsync(ChessDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger(typeof(ChessDbContextSeeder));

            var seeders = this.GetInstances<ISeeder>();

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(dbContext, serviceProvider);
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Seeder {seeder.GetType().Name} done.");
            }
        }

        /// <summary>
        /// Create a list of all classes in the running Assembly having a particular interface that doesn't match the current
        /// running class type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private List<T> GetInstances<T>()
        {
            return (from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.GetInterfaces().Contains(typeof(T)) && t.GetConstructor(Type.EmptyTypes) != null && t.Name != this.GetType().Name
                    select (T)Activator.CreateInstance(t)).ToList();
        }
    }
}
