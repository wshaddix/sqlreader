using System;
using System.Linq;
using System.Reflection;

namespace SqlReader
{
    //TODO: add a static cache and check the cache in the constructor before using reflection on the given T target
    // to improve performance

    internal class ObjectMapper<T>
    {
        private readonly T _target;
        private readonly PropertyInfo[] _targetProperties;

        internal ObjectMapper(T target)
        {
            // reflect on the public properties and store them for later use
            _targetProperties = target.GetType().GetProperties();

            // store the target for later use
            _target = target;
        }

        internal void Map(string propertyName, object value)
        {
            // if the target has a matching property name set the value
            if (_targetProperties.Any(tp => tp.Name.Equals(propertyName)))
            {
                // if we are mapping a DbNull make sure we skip the property
                if (DBNull.Value.Equals(value))
                {
                    return;
                }

                _targetProperties.First(tp => tp.Name.Equals(propertyName)).SetValue(_target, value, null);
            }
        }
    }
}