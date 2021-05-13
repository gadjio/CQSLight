﻿using Microsoft.EntityFrameworkCore;

namespace PGMS.DataProvider.EFCore.Services
{
    public abstract class ContextFactory<T> where T : DbContext
    {
	   
        public virtual T Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<T>();
            optionsBuilder.UseSqlServer(connectionString);

            var context = CreateContext(optionsBuilder.Options);
            
            return context;
        }

        public abstract T CreateContext(DbContextOptions<T> options);
    }


    public interface IConnectionStringProvider
    {
	    string GetConnectionString();
    }

    public class ConnectionStringProvider : IConnectionStringProvider
    {
	    private readonly string connectionString;

	    public ConnectionStringProvider(string connectionString)
	    {
		    this.connectionString = connectionString;
	    }

	    public string GetConnectionString()
	    {
		    return connectionString;
	    }
    }
}