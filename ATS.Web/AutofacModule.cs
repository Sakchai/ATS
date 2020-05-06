using Autofac;
using ATS.Model;
using ATS.Services;
using LinqToDB.DataProvider.Oracle;

namespace ATS.Web
{
    public class AutofacModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            // The generic ILogger<TCategoryName> service was added to the ServiceCollection by ASP.NET Core.
            // It was then registered with Autofac using the Populate method. All of this starts
            // with the services.AddAutofac() that happens in Program and registers Autofac
            // as the service provider.


            builder.RegisterType<BaseDataProvider>().AsImplementedInterfaces().InstancePerLifetimeScope();
            //data layer
            builder.RegisterType<DataProviderManager>().As<IDataProviderManager>().InstancePerDependency();
            builder.Register(context => context.Resolve<IDataProviderManager>().DataProvider).As<IATSDataProvider>().InstancePerDependency();

            builder.RegisterType<ATSContext>().AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(EntityRepository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();

            builder.RegisterType<PersonTrackingService>().As<IPersonTrackingService>().InstancePerLifetimeScope();

		}
    }
}
