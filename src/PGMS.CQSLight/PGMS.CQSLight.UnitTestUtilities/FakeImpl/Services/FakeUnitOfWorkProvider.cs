using PGMS.Data.Services;

namespace PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services;

public class FakeUnitOfWorkProvider : IUnitOfWorkProvider
{
	public IUnitOfWork GetUnitOfWork(bool autoFlush = true)
	{
		return new FakeUnitOfWork();
	}
}