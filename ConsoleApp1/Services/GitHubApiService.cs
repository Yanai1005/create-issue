using System.Text;
using System.Text.Json;
using ConsoleApp1.Models;

namespace ConsoleApp1.Services
{
	/// <summary>
	/// GitHub API操作を行うサービスの実装
	/// </summary>
	public class GitHubApiService : IGitHubApiService, IDisposable
	{
		private readonly HttpClient _httpClient;
		private readonly AppConfig _config;
		private readonly JsonSerializerOptions _jsonOptions;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="config">アプリケーション設定</param>
		public GitHubApiService(AppConfig config)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
			_httpClient = new HttpClient();

			_jsonOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true
			};

			SetupHttpClient();
		}

		/// <summary>
		/// HTTPクライアントの初期設定
		/// </summary>
		private void SetupHttpClient()
		{
			// User-Agentの設定（GitHubのAPI要件）
			_httpClient.DefaultRequestHeaders.Add("User-Agent", _config.UserAgent);

			// 認証ヘッダーの設定
			_httpClient.DefaultRequestHeaders.Add("Authorization", $"token {_config.GitHubToken}");

			// Acceptヘッダーの設定
			_httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
		}

		/// <summary>
		/// Issueを作成する
		/// </summary>
		/// <param name="issueRequest">Issue作成リクエスト</param>
		/// <returns>作成されたIssueの情報</returns>
		public async Task<GitHubIssueResponse?> CreateIssueAsync(GitHubIssueRequest issueRequest)
		{
			try
			{
				var json = JsonSerializer.Serialize(issueRequest, _jsonOptions);
				var content = new StringContent(json, Encoding.UTF8, "application/json");
				var apiUrl = _config.GetIssuesApiUrl();

				var response = await _httpClient.PostAsync(apiUrl, content);

				if (response.IsSuccessStatusCode)
				{
					var responseJson = await response.Content.ReadAsStringAsync();
					return JsonSerializer.Deserialize<GitHubIssueResponse>(responseJson, _jsonOptions);
				}
				else
				{
					var errorContent = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"API エラー: {response.StatusCode}");
					Console.WriteLine($"詳細: {errorContent}");
					return null;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Issue作成エラー: {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// 複数のIssueを一括作成する
		/// </summary>
		/// <param name="issues">作成するIssueのリスト</param>
		/// <returns>作成結果（成功数、失敗数）</returns>
		public async Task<(int successCount, int failCount)> CreateIssuesBatchAsync(List<IssueData> issues)
		{
			var successCount = 0;
			var failCount = 0;

			for (int i = 0; i < issues.Count; i++)
			{
				var issueData = issues[i];

				Console.WriteLine($"Issue {i + 1}/{issues.Count} を登録中...");
				Console.WriteLine($"タイトル: {issueData.Title}");

				var githubIssueRequest = ConvertToGitHubRequest(issueData);
				var result = await CreateIssueAsync(githubIssueRequest);

				if (result != null)
				{
					Console.WriteLine($"✓ Issue #{result.Number} が作成されました");
					Console.WriteLine($"  URL: {result.HtmlUrl}");
					successCount++;
				}
				else
				{
					Console.WriteLine($"✗ Issue作成に失敗しました");
					failCount++;
				}

				// レート制限対策
				await Task.Delay(_config.ApiCallDelayMs);
				Console.WriteLine();
			}

			return (successCount, failCount);
		}

		/// <summary>
		/// IssueDataをGitHubIssueRequestに変換する
		/// </summary>
		/// <param name="issueData">変換元のIssueData</param>
		/// <returns>GitHubIssueRequest</returns>
		private static GitHubIssueRequest ConvertToGitHubRequest(IssueData issueData)
		{
			return new GitHubIssueRequest
			{
				Title = issueData.Title,
				Body = issueData.Body,
				Labels = issueData.Labels ?? new List<string>(),
				Assignees = issueData.Assignees ?? new List<string>(),
				Milestone = issueData.Milestone
			};
		}

		/// <summary>
		/// リソースを解放する
		/// </summary>
		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}