using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject.Extensions.Interception;
using Ninject.Extensions.Interception.Infrastructure.Language;

namespace AopDynamicProxy.ConsoleAppNinject
{
    class Class1
    {
        private static Dictionary<string, object> cache = new Dictionary<string, object>();

        static void Main(string[] args)
        {
            var Kernel = new StandardKernel(new NinjectSettings { LoadExtensions = true }, new DynamicProxyModule());
            Kernel.Bind<ICalculator>().To<Calculator>();

            Kernel.InterceptAround<Calculator>(
                s => s.Add(0,0),
                invocation => Console.WriteLine("Add Start"),
                invocation => Console.WriteLine("Add Stop"));

            Kernel.InterceptAfter<Calculator>(s=>s.Add(0,0),
                invocation => Console.WriteLine("The result was {0}", (int)invocation.ReturnValue));

            
            Kernel.InterceptReplace<Calculator>(c=>c.Add(0,0),
                invocation =>
                {
                    var name = $"{invocation.Request.Method.DeclaringType}_{invocation.Request.Method.Name}";
                    var arg = string.Join(", ", invocation.Request.Arguments.Select(a => (a ?? "").ToString()));
                    var cacheKey = $"{name}|{arg}";

                    cache.TryGetValue(cacheKey, out object returnValue);
                    if (returnValue == null)
                    {
                        invocation.Proceed();
                        returnValue = invocation.ReturnValue;
                        cache.Add(cacheKey, returnValue);
                    }
                    else
                    {
                        invocation.ReturnValue = returnValue;
                    }
                });

            var calc = Kernel.Get<ICalculator>();
            Console.WriteLine(calc.Add(2, 2));
            Console.WriteLine(calc.Add(2, 2));
            Console.WriteLine(calc.Add(2, 3));


            //Kernel.InterceptReplace<SqlCustomerRepository>(
            //    r => r.GetAll(),
            //    invocation =>
            //    {
            //        const string cacheKey = "customers";
            //        if (HttpRuntime.Cache[cacheKey] == null)
            //        {
            //            invocation.Proceed();
            //            if (invocation.ReturnValue != null)
            //                HttpRuntime.Cache[cacheKey] = invocation.ReturnValue;
            //        }
            //        else
            //            invocation.ReturnValue = HttpRuntime.Cache[cacheKey];
            //    });

            //public class ExceptionInterceptor : IInterceptor
            //{
            //    private IExceptionHandlerService exceptionHandlerService;
            //    public ExceptionInterceptor(IExceptionHandlerService handlerService)
            //    {
            //        this.exceptionHandlerService = handlerService;
            //    }

            //    public void Intercept(IInvocation invocation)
            //    {
            //        try
            //        {
            //            invocation.Proceed();
            //        }
            //        catch (Exception exception)
            //        {
            //            exceptionHandlerService.HandleException(exception);
            //        }
            //    }
            //}
            //Kernel.Bind(x => x.FromAssembliesMatching("Northwind.*")
            //    .SelectAllClasses()
            //    .BindAllInterfaces()
            //    .Configure(b =>
            //        b.Intercept()
            //            .With<ExceptionInterceptor>()
            //    ));


            Console.WriteLine("hello world!");
            Console.ReadLine();
        }
    }
}
