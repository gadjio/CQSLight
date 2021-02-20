using System;

namespace PGMS.IntegratedTests
{
	public static class QADataProvider
	{
		public static Guid LocationId => new Guid("C57F77D1-DDC5-45DC-86E2-E1E982EC4DC5");

		public static Guid Location2Id => new Guid("C57F77D1-0000-0000-0000-000000000001");
		public static Guid Location3Id => new Guid("C57F77D1-0000-0000-0000-000000000002");

		public static Guid AngelaMartinId => new Guid("00000000-0000-0000-0000-000000000001");
		public static Guid CreedBrattonId => new Guid("00000000-0000-0000-0000-000000000002");
		public static Guid DwightSchruteId => new Guid("00000000-0000-0000-0000-000000000003");
	}
}