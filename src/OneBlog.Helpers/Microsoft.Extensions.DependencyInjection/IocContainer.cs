using Autofac;
using Autofac.Extensions.DependencyInjection;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class IocContainer
    {
        private static IContainer _container;

        public static IContainer GetContainer()
        {
            return _container;
        }

        public static IServiceProvider RegisterAutofac(ContainerBuilder builder)
        {
            _container = builder.Build();
            return new AutofacServiceProvider(_container);
        }

        public static T Get<T>()
        {
            try
            {
                return _container.Resolve<T>();
            }
            catch (Exception ex)
            {
                throw new Exception("Ioc 配置失败:" + typeof(T).Namespace + " " + ex.ToString());
            }
        }
    }
}
