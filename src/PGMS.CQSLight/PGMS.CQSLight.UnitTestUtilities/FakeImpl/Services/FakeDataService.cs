using PGMS.Data.Services;

namespace PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services;

public class FakeDataService : IDataService
{
    private int nextId = 0;

    public long GenerateId()
    {
        nextId++;
        return nextId;
    }
}