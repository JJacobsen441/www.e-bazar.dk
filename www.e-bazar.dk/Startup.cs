using Microsoft.Owin;
using Owin;
using System.Data.Entity;
using www.e_bazar.dk.Models.Identity;

[assembly: OwinStartupAttribute(typeof(www.e_bazar.dk.Startup))]
namespace www.e_bazar.dk
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // CreateDatabaseIfNotExists is the default initializer if not specified
            //Database.SetInitializer<ApplicationDbContext>(new CreateDatabaseIfNotExists<ApplicationDbContext>());
            Database.SetInitializer<ApplicationDbContext>(new NullDatabaseInitializer<ApplicationDbContext>());

            // NullDatabaseInitializer will turn off migration. 
            // You need to manually create the data tables
            //Database.SetInitializer<ApplicationDbContext>(new NullDatabaseInitializer<ApplicationDbContext>());
        }
    }
}
