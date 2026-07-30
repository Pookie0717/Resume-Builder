using Newtonsoft.Json;

namespace ResumeBuilder.Services
{
    public static class ApiKeyLoader
    {
        public static string GetOpenAIApiKey()
        {
            var fromEnv = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrWhiteSpace(fromEnv))
            {
                return fromEnv;
            }

            var localConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.local.json");
            if (File.Exists(localConfigPath))
            {
                var config = JsonConvert.DeserializeObject<LocalApiConfig>(File.ReadAllText(localConfigPath));
                if (!string.IsNullOrWhiteSpace(config?.OpenAIApiKey))
                {
                    return config.OpenAIApiKey;
                }
            }

            throw new InvalidOperationException(
                "OpenAI API key not found. Set the OPENAI_API_KEY environment variable, " +
                "or copy appsettings.local.json.example to appsettings.local.json and add your key.");
        }

        private sealed class LocalApiConfig
        {
            public string? OpenAIApiKey { get; set; }
        }
    }
}
