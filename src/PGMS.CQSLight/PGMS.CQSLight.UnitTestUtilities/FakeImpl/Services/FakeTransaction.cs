using PGMS.Data.Services;

namespace PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services
{
	public class FakeTransaction : IUnitOfWorkTransaction
	{
		public void Dispose()
		{
		}

		public void Commit()
		{
		}

		public Task CommitAsync()
		{
			return Task.CompletedTask;
		}

		public void Rollback()
		{
		}

		public Task RollbackAsync()
		{
			return Task.CompletedTask;
		}

		public ValueTask DisposeAsync()
		{
			return ValueTask.CompletedTask;
		}
	}
}