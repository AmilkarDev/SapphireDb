# SapphireDb - Server for Asp.Net Core [![Build Status](https://travis-ci.org/morrisjdev/RealtimeDatabase.svg?branch=master)](https://travis-ci.org/morrisjdev/RealtimeDatabase) [![Maintainability](https://api.codeclimate.com/v1/badges/a80b67f61f2c952d3b49/maintainability)](https://codeclimate.com/github/morrisjdev/RealtimeDatabase/maintainability)

<p align="center">
  <a href="https://sapphire-db.com/">
    <img src="https://sapphire-db.com/assets/banner/SapphireDB%20Banner.png" alt="SapphireDb logo">
  </a>

</p>

SapphireDb is an open source library that enables you to easily create your own application with realtime data synchronization.

Build amazing reactive applications with realtime data synchronization and get the best results of your project.
SapphireDb should serve as a self hosted alternative to firebase and also gives you an alternative to SignalR.

Check out the documentation for more details: [Documentation](https://sapphire-db.com/)

<p align="center">
    <a href="https://www.patreon.com/user?u=27738280"><img src="https://c5.patreon.com/external/logo/become_a_patron_button@2x.png" width="160"></a>
</p>

## Advantages

- :wrench: Dead simple configuration
- :stars: Blazing fast development
- :satellite: Modern technologies
- :computer: Self hosted
- :floppy_disk: Easy CRUD operations
- :key: Authentication/Authorization included
- :heavy_check_mark: Database support
- :open_file_folder: Supports includes/joins
- :electric_plug: Actions
- :globe_with_meridians: NLB support

## Installation

### Install package
To install the package execute the following command in your package manager console

````
PM> Install-Package SapphireDb
````

You can also install the extension using Nuget package manager. The project can be found here: [https://www.nuget.org/packages/SapphireDb/](https://www.nuget.org/packages/SapphireDb/)

### Configure DbContext

You now have to change your DbContext to derive from `SapphireDbContext`. Also make sure to adjust the constructor parameters.

````csharp
// Change DbContext to SapphireDbContext
public class MyDbContext : SapphireDbContext
{
  //Add SapphireDatabaseNotifier for DI
  public MyDbContext(DbContextOptions<MyDbContext> options, SapphireDatabaseNotifier notifier) : base(options, notifier)
  {

  }

  public DbSet<User> Users { get; set; }

  public DbSet<Test> Tests { get; set; }
}
````

### Register services and update pipeline

To use the SapphireDb you also have to make some changes in your `Startup.cs`-File.

````csharp
public class Startup
{
  public void ConfigureServices(IServiceCollection services)
  {
    //Register services
    services.AddSapphireDb(...)
      .AddContext<MyDbContext>(cfg => ...);
  }

  public void Configure(IApplicationBuilder app)
  {
    //Add Middleware
    app.UseSapphireDb();
  }
}
````

## Documentation

Check out the documentation for more details: [Documentation](https://sapphire-db.com/)

## Implementations

[SapphireDb - Server for Asp.Net Core](https://github.com/morrisjdev/SapphireDb)

[ng-sapphiredb - Angular client](https://github.com/morrisjdev/ng-sapphiredb)

## Author

[Morris Janatzek](http://morrisj.net) ([morrisjdev](https://github.com/morrisjdev))

## License

SapphireDb - [MIT License](https://github.com/morrisjdev/SapphireDb/blob/master/LICENSE)
