using System;

namespace PGMS.Data.Services
{
	public interface IDataService
    {
        long GenerateId();
    }

    public class IsComplexTypeAttribute : Attribute
    { }

    [Obsolete("Will be remove (this attribute set the Whole class to be in a LazyLoad mode) - Use LazyLoadAttribute on class or property")]
    public class IsLazyLoadingAttribute : Attribute
    { }

    public class LazyLoadAttribute : Attribute
    {}
}