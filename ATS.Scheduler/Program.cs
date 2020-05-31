using ATS.Model;
using ATS.Services;
using Autofac;
using System;
using System.Configuration;
using System.IO;

namespace ATS.Scheduler
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            bool useAPI = ConfigurationManager.AppSettings["UseAPI"].Equals("Y");
            bool useFile = ConfigurationManager.AppSettings["UseFile"].Equals("Y");

            if (useFile)
            {
                var personTracker = new PersonTrackerFile();
                await personTracker.Create();
            }
            else
            {
                if (!useAPI)
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
                else
                {
                    var personTracker = new PersonTrackerAPI();

                    await personTracker.CreateV2Async();
                }
            }
        }

  
    }
}
