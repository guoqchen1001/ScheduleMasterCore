using System;
using System.Linq;
using System.Reflection;


namespace Hos.ScheduleMaster.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AutowiredAttribute : Attribute
    {

    }

    public class AutowiredServiceProvider
    {
        
        public void PropertyActivate(object service, IServiceProvider provider)
        {
            
            var serviceType = service.GetType();
         
            //属性赋值
            var properties = serviceType.GetProperties().AsEnumerable().Where(x => x.Name.StartsWith("_"));
            foreach (var property in properties)
            {
                var autowiredAttr = property.GetCustomAttribute<AutowiredAttribute>();
                if (autowiredAttr == null) continue;
                
                var innerService = provider.GetService(property.PropertyType);
                PropertyActivate(innerService, provider);
                
                property.SetValue(service, innerService);
            }

        }

    }
}

