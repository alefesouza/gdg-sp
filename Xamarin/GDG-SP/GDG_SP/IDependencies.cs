namespace GDG_SP
{
	public interface IDependencies
	{
		void SendOneSignalTag(string key, string value);
        string GetAppVersion();
        string GetOSVersion();
	}
}
