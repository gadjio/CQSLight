using PGMS.Data.Services;

namespace PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services
{
	public class FakeUnitOfWork : IUnitOfWork
	{
		public void Dispose()
		{
		}

		public bool IsAutoFlush()
		{
			return false;
		}

		public void Save()
		{
		}

		public Task SaveAsync()
		{
			return Task.CompletedTask;
		}

		public IUnitOfWorkTransaction GetTransaction()
		{
			return new FakeTransaction();
		}

		public Task<IUnitOfWorkTransaction> GetTransactionAsync()
		{
			return Task.FromResult<IUnitOfWorkTransaction>(new FakeTransaction());
		}

		public IDbContext GetDbContext()
		{
			return null;
		}

		public ValueTask DisposeAsync()
		{
			return ValueTask.CompletedTask;
		}
	}
}