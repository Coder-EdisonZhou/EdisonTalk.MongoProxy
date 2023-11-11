using System.Security.Cryptography.X509Certificates;

namespace EdisonTalk.MongoProxy.Configurations;

//// ++++++++++++++++++++++
//// MongoDB
//// ++++++++++++++++++++++
/** Config Example
"MongoDatabaseConfigs": {
  "Servers": "xxx01.edisontalk.net,xxx02.edisontalk.net,xxx03.edisontalk.net",
  "Port": 27017,
  "ReplicaSetName": "edt-replica",
  "DatabaseName": "EDT_Practices",
  "AuthDatabaseName": "admin",
  "ApplicationName": "Todo",
  "UserName": "service_testdev",
  "Password": "xxxxxxxxxxxxxxxxxxxxxxxx",
  "UseTLS": true,
  "AllowInsecureTLS": true,
  "SslCertificatePath": "/etc/pki/tls/certs/EDT_CA.cer",
  "UseEncryption": true
}
**/
public class MongoDatabaseConfigs
{
    private const string DEFAULT_AUTH_DB = "admin"; // Default AuthDB: admin

    public string Servers { get; set; }
    public int Port { get; set; } = 27017; // Default Port: 27017
    public string ReplicaSetName { get; set; }
    public string DatabaseName { get; set; }
    public string DefaultCollectionName { get; set; } = string.Empty;
    public string ApplicationName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string AuthDatabaseName { get; set; } = DEFAULT_AUTH_DB; // Default AuthDB: admin
    public string CustomProperties { get; set; } = string.Empty;
    public bool UseTLS { get; set; } = false;
    public bool AllowInsecureTLS { get; set; } = true;
    public string SslCertificatePath { get; set; } = string.Empty;
    public bool StoreCertificateInKeyStore { get; set; } = false;


    public MongoClientSettings GetMongoClientSettings(IConfiguration configuration = null)
    {
        if (string.IsNullOrWhiteSpace(Servers))
            throw new ArgumentNullException("Mongo Servers Configuration is Missing!");

        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
            throw new ArgumentNullException("Mongo Account Configuration is Missing!");

        // Base Configuration
        MongoClientSettings settings = new MongoClientSettings
        {
            ApplicationName = ApplicationName,
            ReplicaSetName = ReplicaSetName
        };


        // Credential
        settings.Credential = MongoCredential.CreateCredential(AuthDatabaseName, UserName, Password);

        // Servers
        var mongoServers = Servers.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
        if (mongoServers.Count == 1) // Standalone
        {
            settings.Server = new MongoServerAddress(mongoServers.First(), Port);
            settings.DirectConnection = true;
        }

        if (mongoServers.Count > 1) // Cluster
        {
            var mongoServerAddresses = new List<MongoServerAddress>();
            foreach (var mongoServer in mongoServers)
            {
                var mongoServerAddress = new MongoServerAddress(mongoServer, Port);
                mongoServerAddresses.Add(mongoServerAddress);
            }
            settings.Servers = mongoServerAddresses;
            settings.DirectConnection = false;
        }

        // SSL
        if (UseTLS)
        {
            settings.UseTls = true;
            settings.AllowInsecureTls = AllowInsecureTLS;
            if (string.IsNullOrWhiteSpace(SslCertificatePath))
                throw new ArgumentNullException("SslCertificatePath is Missing!");

            if (StoreCertificateInKeyStore)
            {
                var localTrustStore = new X509Store(StoreName.Root);
                var certificateCollection = new X509Certificate2Collection();
                certificateCollection.Import(SslCertificatePath);
                try
                {
                    localTrustStore.Open(OpenFlags.ReadWrite);
                    localTrustStore.AddRange(certificateCollection);
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    localTrustStore.Close();
                }
            }

            var certs = new List<X509Certificate> { new X509Certificate2(SslCertificatePath) };
            settings.SslSettings = new SslSettings();
            settings.SslSettings.ClientCertificates = certs;
            settings.SslSettings.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls13;
        }

        return settings;
    }
}
