
namespace RibbitReels.Data.Configs;

    public class AzureBlobConfiguration
    {
        public string ConnectionString { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public string AccountKey { get; set; } = string.Empty;

        public string ContainerName { get; set; } = "videos";
    }
