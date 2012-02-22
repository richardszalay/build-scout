using System;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class NotifyPropertyAttribute : Attribute
    {
        public bool PerformEqualityCheck { get; set; }
        public string[] AlsoNotifyFor { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NotifyForAllAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DoNotNotifyAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class DependsOnAttribute : Attribute
    {
        public DependsOnAttribute(string dependency, params string[] otherDependencies)
        {
        }
    }
}
