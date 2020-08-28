using System;

namespace PGMS.Data.Services
{
	public interface IDataService
    {
        long GenerateId();
    }

    public class IsComplexTypeAttribute : Attribute
    { }

    public class IsLazyLoadingAttribute : Attribute
    { }
}