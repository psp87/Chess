namespace Chess.Web.Hubs.Configuration
{
    using System;

    using Microsoft.AspNet.SignalR;

    public class CustomDependencyResolver : DefaultDependencyResolver
    {
        private readonly IServiceProvider serviceProvider;

        public CustomDependencyResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override object GetService(Type serviceType)
        {
            var service = this.serviceProvider.GetService(serviceType);

            return service ?? base.GetService(serviceType);
        }
    }
}
