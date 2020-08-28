namespace Sample.Core.Infra.Configs
{
	public class AppSettings
	{
		public Environnement Environment { get; set; }
	}

	public enum Environnement
	{
		DEV,
		QA,
		ClientUAT,
		PROD,
	}
}