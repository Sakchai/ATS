using ATS.Model;
using ATS.Services;
using Autofac;
using System;
using System.IO;

namespace ATS.Scheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<BaseDataProvider>().AsImplementedInterfaces().InstancePerLifetimeScope();
            //data layer
            builder.RegisterType<DataProviderManager>().As<IDataProviderManager>().InstancePerDependency();
            builder.Register(context => context.Resolve<IDataProviderManager>().DataProvider).As<IATSDataProvider>().InstancePerDependency();

            builder.RegisterType<ATSContext>().AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(EntityRepository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();

            builder.RegisterType<PersonTrackingService>().As<IPersonTrackingService>().InstancePerLifetimeScope();

            using (var container = builder.Build())
            {
                var personService = container.Resolve<IPersonTrackingService>();

                var personTracker = new PersonTracker(container, personService);

                
                personTracker.Create();


            }
        }

  
    }
}
