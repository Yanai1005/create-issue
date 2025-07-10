using System.Text.Json;

namespace ConsoleApp1
{
	/// <summary>
	/// アプリケーション設定を管理するクラス
	/// </summary>
	public class AppConfig
	{
		/// <summary>
		/// GitHubアクセストークン
		/// </summary>
		public string GitHubToken { get; set; } = string.Empty;

		/// <summary>
		/// リポジトリオーナー名
		/// </summary>
		public string RepoOwner { get; set; } = string.Empty;

		/// <summary>
		/// リポジトリ名
		/// </summary>
		public string RepoName { get; set; } = string.Empty;

		/// <summary>
		/// JSONファイルのパス
		/// </summary>
		public string JsonFilePath { get; set; } = "../../../issues.json";

		/// <summary>
		/// GitHub API ベースURL
		/// </summary>
		public string GitHubApiBaseUrl { get; set; } = "https://api.github.com";

		/// <summary>
		/// API呼び出し間隔（ミリ秒）
		/// </summary>
		public int ApiCallDelayMs { get; set; } = 1000;

		/// <summary>
		/// User-Agent文字列
		/// </summary>
		public string UserAgent { get; set; } = "ConsoleApp1/1.0";

		/// <summary>
		/// デバッグ情報を表示するかどうか
		/// </summary>
		public bool ShowDebugInfo { get; set; } = false;

		/// <summary>
		/// 設定ファイルから設定を読み込む
		/// </summary>
		/// <param name="configFilePath">設定ファイルのパス</param>
		/// <returns>読み込まれた設定</returns>
		public static AppConfig LoadFromJson(string configFilePath = "appsettings.json")
		{
			try
			{
				var fullPath = GetConfigFilePath(configFilePath);

				if (!File.Exists(fullPath))
				{
					Console.WriteLine($"設定ファイルが見つかりません: {fullPath}");
					Console.WriteLine("デフォルト設定を使用します。");
					return CreateDefaultConfig();
				}

				var json = File.ReadAllText(fullPath);
				var jsonDocument = JsonDocument.Parse(json);
				var root = jsonDocument.RootElement;

				var config = new AppConfig();

				// GitHubSettings
				if (root.TryGetProperty("GitHubSettings", out var gitHubSettings))
				{
					config.GitHubToken = GetStringValue(gitHubSettings, "Token", config.GitHubToken);
					config.RepoOwner = GetStringValue(gitHubSettings, "RepoOwner", config.RepoOwner);
					config.RepoName = GetStringValue(gitHubSettings, "RepoName", config.RepoName);
					config.GitHubApiBaseUrl = GetStringValue(gitHubSettings, "ApiBaseUrl", config.GitHubApiBaseUrl);
					config.UserAgent = GetStringValue(gitHubSettings, "UserAgent", config.UserAgent);
				}

				// FileSettings
				if (root.TryGetProperty("FileSettings", out var fileSettings))
				{
					config.JsonFilePath = GetStringValue(fileSettings, "IssuesJsonPath", config.JsonFilePath);
				}

				// ApplicationSettings
				if (root.TryGetProperty("ApplicationSettings", out var appSettings))
				{
					config.ApiCallDelayMs = GetIntValue(appSettings, "ApiCallDelayMs", config.ApiCallDelayMs);
					config.ShowDebugInfo = GetBoolValue(appSettings, "ShowDebugInfo", config.ShowDebugInfo);
				}

				// JSONファイルパスを絶対パスに変換
				config.JsonFilePath = GetAbsoluteFilePath(config.JsonFilePath);

				Console.WriteLine($"設定ファイルを読み込みました: {fullPath}");
				return config;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"設定ファイル読み込みエラー: {ex.Message}");
				Console.WriteLine("デフォルト設定を使用します。");
				return CreateDefaultConfig();
			}
		}

		/// <summary>
		/// デフォルト設定を作成する
		/// </summary>
		/// <returns>デフォルト設定</returns>
		private static AppConfig CreateDefaultConfig()
		{
			return new AppConfig
			{
				JsonFilePath = GetAbsoluteFilePath("issues.json")
			};
		}

		/// <summary>
		/// 設定ファイルのパスを取得する
		/// </summary>
		/// <param name="configFileName">設定ファイル名</param>
		/// <returns>設定ファイルの絶対パス</returns>
		private static string GetConfigFilePath(string configFileName)
		{
			// 実行ファイルと同じディレクトリを最初に確認
			var exeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			var configPath = Path.Combine(exeDirectory ?? "", configFileName);

			if (File.Exists(configPath))
			{
				return configPath;
			}

			// 現在のディレクトリを確認
			var currentDirectory = Directory.GetCurrentDirectory();
			var currentPath = Path.Combine(currentDirectory, configFileName);

			if (File.Exists(currentPath))
			{
				return currentPath;
			}

			// プロジェクトルートディレクトリを探す
			var parentDirectory = Directory.GetParent(currentDirectory);
			while (parentDirectory != null)
			{
				var parentPath = Path.Combine(parentDirectory.FullName, configFileName);
				if (File.Exists(parentPath))
				{
					return parentPath;
				}
				parentDirectory = parentDirectory.Parent;
			}

			return configPath; // 見つからない場合は実行ディレクトリのパスを返す
		}

		/// <summary>
		/// ファイルの絶対パスを取得する
		/// </summary>
		/// <param name="filePath">ファイルパス</param>
		/// <returns>絶対ファイルパス</returns>
		private static string GetAbsoluteFilePath(string filePath)
		{
			if (Path.IsPathRooted(filePath))
			{
				return filePath;
			}

			// 実行ファイルと同じディレクトリを最初に確認
			var exeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			var exePath = Path.Combine(exeDirectory ?? "", filePath);

			if (File.Exists(exePath))
			{
				return exePath;
			}

			// 現在のディレクトリを確認
			var currentDirectory = Directory.GetCurrentDirectory();
			var currentPath = Path.Combine(currentDirectory, filePath);

			if (File.Exists(currentPath))
			{
				return currentPath;
			}

			// プロジェクトルートディレクトリを探す
			var parentDirectory = Directory.GetParent(currentDirectory);
			while (parentDirectory != null)
			{
				var parentPath = Path.Combine(parentDirectory.FullName, filePath);
				if (File.Exists(parentPath))
				{
					return parentPath;
				}
				parentDirectory = parentDirectory.Parent;
			}

			return exePath; // 見つからない場合は実行ディレクトリのパスを返す
		}

		/// <summary>
		/// JSON要素から文字列値を取得する
		/// </summary>
		private static string GetStringValue(JsonElement element, string propertyName, string defaultValue)
		{
			if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
			{
				return property.GetString() ?? defaultValue;
			}
			return defaultValue;
		}

		/// <summary>
		/// JSON要素から整数値を取得する
		/// </summary>
		private static int GetIntValue(JsonElement element, string propertyName, int defaultValue)
		{
			if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number)
			{
				return property.GetInt32();
			}
			return defaultValue;
		}

		/// <summary>
		/// JSON要素からブール値を取得する
		/// </summary>
		private static bool GetBoolValue(JsonElement element, string propertyName, bool defaultValue)
		{
			if (element.TryGetProperty(propertyName, out var property))
			{
				if (property.ValueKind == JsonValueKind.True) return true;
				if (property.ValueKind == JsonValueKind.False) return false;
			}
			return defaultValue;
		}

		/// <summary>
		/// 設定の妥当性を検証する
		/// </summary>
		/// <returns>妥当な設定かどうか</returns>
		public bool IsValid()
		{
			var isValid = !string.IsNullOrWhiteSpace(GitHubToken) &&
						  !string.IsNullOrWhiteSpace(RepoOwner) &&
						  !string.IsNullOrWhiteSpace(RepoName) &&
						  !string.IsNullOrWhiteSpace(JsonFilePath) &&
						  GitHubToken != "your_github_token_here" &&
						  RepoOwner != "your_username" &&
						  RepoName != "your_repository_name";

			if (!isValid)
			{
				Console.WriteLine("設定が正しくありません。appsettings.jsonの以下の項目を確認してください:");

				if (string.IsNullOrWhiteSpace(GitHubToken) || GitHubToken == "your_github_token_here")
					Console.WriteLine("- GitHubSettings.Token: Personal Access Tokenを設定");

				if (string.IsNullOrWhiteSpace(RepoOwner) || RepoOwner == "your_username")
					Console.WriteLine("- GitHubSettings.RepoOwner: リポジトリオーナー名を設定");

				if (string.IsNullOrWhiteSpace(RepoName) || RepoName == "your_repository_name")
					Console.WriteLine("- GitHubSettings.RepoName: リポジトリ名を設定");
			}

			return isValid;
		}

		/// <summary>
		/// GitHubリポジトリのIssues API URLを取得する
		/// </summary>
		/// <returns>Issues API URL</returns>
		public string GetIssuesApiUrl()
		{
			return $"{GitHubApiBaseUrl}/repos/{RepoOwner}/{RepoName}/issues";
		}
	}
}