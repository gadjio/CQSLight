using System;

namespace SMI.Data.Services
{
	public interface ITradeBoardDataService : IDataService
    { }

    public interface IWorkplanManagerService : IDataService
    { }

    public interface IDataService
    {
        long GenerateId();
    }

    public class IsComplexTypeAttribute : Attribute
    { }

    public class IsLazyLoadingAttribute : Attribute
    { }
}