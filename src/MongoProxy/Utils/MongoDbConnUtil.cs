using System.Text;

namespace EdisonTalk.MongoProxy.Utils;
/// <summary>
/// For CAP项目MongoDB集成：
/*
        option.UseMongoDB(option =>
        {
            option.DatabaseConnection = DbConnUtil.GetMongoDbConnectionString(config);
            option.DatabaseName = config["MongoDatabaseConfigs:DatabaseName"] 
                ?? throw new ArgumentException("MongoDatabaseConfigs:DatabaseName must be set!");
option.PublishedCollection = "msg.published";
            option.ReceivedCollection = "msg.received";
        });
 */
/// </summary>
public static class MongoDbConnUtil
{
	// Const Settings for Mongo
	private const int DEFAULT_CONNECT_TIMEOUT_MS = 10000; // 10s
	private const int DEFAULT_SERVER_SELECTION_TIMEOUT_MS = 5000; // 5s
	private const string DEFAULT_AUTH_MECHANISM = "SCRAM-SHA-256"; // SCRAM-SHA-256
	private const string DEFAULT_READ_PREFERENCE = "primaryPreferred"; // Primary Preferred
	private const string DEFAULT_SSL_INVALID_HOSTNAME_ALLOWED = "true"; // Allow Invalid HostName for SSL

	/// <summary>
	/// 获取MongoDB数据库连接字符串
	/// 需要在配置文件中提前根据指定Key进行设置
	/// </summary>
	public static string GetMongoDbConnectionString(IConfiguration config)
	{
		var servers = config["MongoDatabaseConfigs:Servers"];
		var port = config["MongoDatabaseConfigs:Port"] ?? "27017";
		if (string.IsNullOrWhiteSpace(servers))
			throw new ArgumentNullException("Mongo Servers Configuration is Missing!");
		var mongoServers = servers.Split(',');

		// Basic Auth
		var userName = config["MongoDatabaseConfigs:UserName"];
		var password = config["MongoDatabaseConfigs:Password"];
		if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
			throw new ArgumentNullException("Mongo Account Configuration is Missing!");

		// Uri
		var replicaName = config["MongoDatabaseConfigs:ReplicaSetName"];
		var authDatabaseName = config["MongoDatabaseConfigs:AuthDatabaseName"] ?? "admin";
		var mongoUriBuilder = new StringBuilder();
		mongoUriBuilder.Append($"mongodb://{userName}:{password}@");
		for (int i = 0; i < mongoServers.Length; i++)
		{
			if (i < mongoServers.Length - 1)
			{
				mongoUriBuilder.Append($"{mongoServers[i]}:{port},");
			}
			else
			{
				mongoUriBuilder.Append($"{mongoServers[i]}:{port}/?");
			}
		}

		// Settings
		var applicationName = config["MongoDatabaseConfigs:ApplicationName"];
		mongoUriBuilder.Append($"replicaSet={replicaName}");
		mongoUriBuilder.Append($"&appName={applicationName}");
		mongoUriBuilder.Append($"&authSource={authDatabaseName}");
		mongoUriBuilder.Append($"&authMechanism={DEFAULT_AUTH_MECHANISM}");
		mongoUriBuilder.Append($"&connectTimeoutMS={DEFAULT_CONNECT_TIMEOUT_MS}");
		mongoUriBuilder.Append($"&serverSelectionTimeoutMS={DEFAULT_SERVER_SELECTION_TIMEOUT_MS}");
		mongoUriBuilder.Append($"&readPreference={DEFAULT_READ_PREFERENCE}");

		// TLS/SSL Auth
		var useTLS = Convert.ToBoolean(config["MongoDatabaseConfigs:UseTLS"] ?? "false");
		if (useTLS)
		{
			var allowInsecureTls = Convert.ToBoolean(config["MongoDatabaseConfigs:AllowInsecureTLS"] ?? "true");
			var sslCertificatePath = config["MongoDatabaseConfigs:SslCertificatePath"];

			mongoUriBuilder.Append($"&ssl={useTLS}");
			mongoUriBuilder.Append($"&net.ssl.CAFile={sslCertificatePath}");
			mongoUriBuilder.Append($"&net.ssl.allowInvalidCertificates={DEFAULT_SSL_INVALID_HOSTNAME_ALLOWED}");
		}

		return mongoUriBuilder.ToString();
	}
}
