using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactsTableCosmosWebApp.Models;
using ContactsTableCosmosWebApp.Models.Abstract;
using ContactsTableCosmosWebApp.Models.Concrete;
using ContactsTableCosmosWebApp.Models.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace ContactsTableCosmosWebApp
{
  public class Startup
  {
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
      _configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
      #region Utilities
      services.Configure<StorageUtility>(cfg =>
      {
        cfg.StorageAccountName = _configuration["StorageAccountInformation:StorageAccountName"];
        cfg.StorageAccountAccessKey = _configuration["StorageAccountInformation:StorageAccountAccessKey"];
      });
      services.Configure<CosmosUtility>(cfg =>
      {
        cfg.CosmosEndpoint = _configuration["CosmosConnectionString:CosmosEndpoint"];
        cfg.CosmosKey = _configuration["CosmosConnectionString:CosmosKey"];
      });
      #endregion

      #region RedisCache
      if (_configuration["EnableRedisCaching"] == "true")
      {
        services.AddDistributedRedisCache(cfg =>
        {
          cfg.Configuration = _configuration["ConnectionStrings:RedisConnection"];
          cfg.InstanceName = "master";
        });
      }
      #endregion

      #region ApplicationInsights
      services.AddApplicationInsightsTelemetry(cfg =>
      {
        cfg.InstrumentationKey = _configuration["ApplicationInsights:InstrumentationKey"];
      });
      services.AddLogging(cfg =>
      {
        cfg.AddApplicationInsights(_configuration["ApplicationInsights:InstrumentationKey"]);
        // Optional: Apply filters to configure LogLevel Information or above is sent to
        // ApplicationInsights for all categories.
        cfg.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);

        // Additional filtering For category starting in "Microsoft",
        // only Warning or above will be sent to Application Insights.
        //cfg.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Warning);
      });
      #endregion

      #region Database
      if (string.IsNullOrEmpty(_configuration["DatabaseType"]) || _configuration["DatabaseType"] == "AzureTable")
      { services.AddScoped<IContactRepository, TableContactRepository>(); }
      if (_configuration["DatabaseType"] == "CosmosDb")
      { services.AddScoped<IContactRepository, CosmosContactRepository>(); }
      #endregion

      #region Swagger
      services.AddSwaggerGen(cfg =>
      {
        cfg.SwaggerDoc(name: "V1", info: new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Contacts API", Version = "V1" });
      });
      #endregion

      services.AddControllersWithViews();
      services.AddRazorPages().AddRazorRuntimeCompilation();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      var appInsightsFlag = app.ApplicationServices.GetService<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>();
      if (_configuration["DisableAppInsightsTelemetry"] == "false")
      {
        appInsightsFlag.DisableTelemetry = false;
      }
      else
      {
        appInsightsFlag.DisableTelemetry = true;
      }

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      app.UseFileServer();

      app.UseRouting();

      #region Database
      var contactSource = app.ApplicationServices.CreateScope().ServiceProvider.GetService<IContactRepository>();
      if (string.IsNullOrEmpty(_configuration["DatabaseType"]) || _configuration["DatabaseType"] == "AzureTable")
      {
        SeedAzTableData(contactSource, "Table");
      }
      if (_configuration["DatabaseType"] == "CosmosDb")
      {
        SeedAzTableData(contactSource, "Cosmos");
      }
      #endregion

      app.UseSwagger();
      app.UseSwaggerUI(cfg =>
      {
        cfg.SwaggerEndpoint(url: "/swagger/V1/swagger.json", name: "Contact API");
      });

      app.UseEndpoints(endpoints =>
      {

        endpoints.MapControllerRoute("default", "{controller}/{action}/{id?}", new { controller = "Contact", action = "Index" });
        // endpoints.MapGet("/", async context =>
        //       {
        //         await context.Response.WriteAsync("Hello World!");
        //       });
      });
    }

    private void SeedAzTableData(IContactRepository contactRepository, string databaseType)
    {
      var contactList = contactRepository.GetAllContactsAsync().GetAwaiter().GetResult();
      if (contactList.Count == 0)
      {
        if (databaseType == "Table")
        {
          contactList = new List<Contact>
          {
            new Contact {ContactName="Tintin", ContactType = "Friend", Email = "tintin@t.com", Phone = "1234567890" },
            new Contact {ContactName="Snowy", ContactType = "Family", Email = "snowy@s.com", Phone = "2345678901" },
            new Contact {ContactName="Haddock", ContactType = "Friend", Email = "haddock@h.com", Phone = "3456789012" },
            new Contact {ContactName="Thomson", ContactType = "Professional", Email = "thomson@t.com", Phone = "4567890123" },
            new Contact {ContactName="Calculus", ContactType = "Friend", Email = "calculus@c.com", Phone = "5678901234" }
          };
        }
        if (databaseType == "Cosmos")
        {
          contactList = new List<Contact>
          {
            new Contact {ContactName="Mowgli", ContactType = "Family", Email = "mowgli@m.com", Phone = "1234567890" },
            new Contact {ContactName="Bagheera", ContactType = "Friend", Email = "bagheera@b.com", Phone = "2345678901" },
            new Contact {ContactName="Sherekhan", ContactType = "Professional", Email = "sherekhan.com", Phone = "3456789012" },
            new Contact {ContactName="Kaa", ContactType = "Professional", Email = "kaa@k.com", Phone = "4567890123" },
            new Contact {ContactName="Baloo", ContactType = "Friend", Email = "Baloo@b.com", Phone = "5678901234" }


          };
        }
        foreach (Contact item in contactList)
        { contactRepository.CreateAsync(item); }
      }
    }
  }
}
