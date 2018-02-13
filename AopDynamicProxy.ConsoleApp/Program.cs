using System;
using Autofac;
using Autofac.Extras.DynamicProxy;

namespace AopDynamicProxy.ConsoleApp
{
    class Program
    {
        private static IContainer Container { get; set; }
        static void Main(string[] args)
        {
            var b = new ContainerBuilder();

            b.Register(i => new MemoryCaching());
            b.Register(i => new Logger(Console.Out));
            b.RegisterType<Calculator>()
                .As<ICalculator>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(MemoryCaching))
                .InterceptedBy(typeof(Logger));

            Container = b.Build();

            using (var scope = Container.BeginLifetimeScope())
            {
                var calculator = scope.Resolve<ICalculator>();
                calculator.Add(1, 2);
                calculator.Add(1, 2);
                calculator.Add(5, 6);
            }

            Console.WriteLine("Hello World!");
        }
    };
}
