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
		private readonly Config _config;
		private readonly JsonSerializerOptions _jsonOptions;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="config">アプリケーション設定</param>
		public GitHubApiService(Config config)
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
		/// 現在のRate Limit情報を取得する
		/// </summary>
		/// <returns>Rate Limit情報</returns>
		public async Task<RateLimitInfo?> GetRateLimitAsync()
		{
			try
			{
				var apiUrl = $"{_config.GitHubApiBaseUrl}/rate_limit";
				var response = await _httpClient.GetAsync(apiUrl);

				if (response.IsSuccessStatusCode)
				{
					// レスポンスヘッダーからRate Limit情報を取得
					var rateLimitInfo = ExtractRateLimitFromHeaders(response.Headers);

					if (rateLimitInfo != null)
					{
						return rateLimitInfo;
					}

					// ヘッダーから取得できない場合は、レスポンスボディから取得
					var responseJson = await response.Content.ReadAsStringAsync();
					var jsonDocument = JsonDocument.Parse(responseJson);

					if (jsonDocument.RootElement.TryGetProperty("rate", out var rateElement))
					{
						return new RateLimitInfo
						{
							Limit = rateElement.GetProperty("limit").GetInt32(),
							Remaining = rateElement.GetProperty("remaining").GetInt32(),
							ResetTimestamp = rateElement.GetProperty("reset").GetInt64()
						};
					}
				}
				else
				{
					Console.WriteLine($"Rate Limit取得エラー: {response.StatusCode}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Rate Limit取得エラー: {ex.Message}");
			}

			return null;
		}

		/// <summary>
		/// HTTPレスポンスヘッダーからRate Limit情報を抽出する
		/// </summary>
		/// <param name="headers">HTTPレスポンスヘッダー</param>
		/// <returns>Rate Limit情報</returns>
		private static RateLimitInfo? ExtractRateLimitFromHeaders(System.Net.Http.Headers.HttpResponseHeaders headers)
		{
			try
			{
				var hasLimit = headers.TryGetValues("X-RateLimit-Limit", out var limitValues);
				var hasRemaining = headers.TryGetValues("X-RateLimit-Remaining", out var remainingValues);
				var hasReset = headers.TryGetValues("X-RateLimit-Reset", out var resetValues);

				if (hasLimit && hasRemaining && hasReset)
				{
					var limit = int.Parse(limitValues.First());
					var remaining = int.Parse(remainingValues.First());
					var reset = long.Parse(resetValues.First());

					return new RateLimitInfo
					{
						Limit = limit,
						Remaining = remaining,
						ResetTimestamp = reset
					};
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Rate Limitヘッダー解析エラー: {ex.Message}");
			}

			return null;
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

					var deserializeOptions = new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true,
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase
					};

					var issueResponse = JsonSerializer.Deserialize<GitHubIssueResponse>(responseJson, deserializeOptions);

					// Rate Limit情報をデバッグ表示（オプション）
					if (_config.ShowDebugInfo)
					{
						var rateLimitInfo = ExtractRateLimitFromHeaders(response.Headers);
						if (rateLimitInfo != null)
						{
							Console.WriteLine($"  Rate Limit: {rateLimitInfo.ToShortString()}");
						}
					}

					return issueResponse;
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
		/// Issueにコメントを追加する
		/// </summary>
		/// <param name="issueNumber">Issue番号</param>
		/// <param name="commentBody">コメント内容</param>
		/// <returns>追加に成功したかどうか</returns>
		public async Task<bool> AddCommentAsync(int issueNumber, string commentBody)
		{
			try
			{
				var commentRequest = new { body = commentBody };
				var json = JsonSerializer.Serialize(commentRequest, _jsonOptions);
				var content = new StringContent(json, Encoding.UTF8, "application/json");
				var apiUrl = $"{_config.GitHubApiBaseUrl}/repos/{_config.RepoOwner}/{_config.RepoName}/issues/{issueNumber}/comments";

				var response = await _httpClient.PostAsync(apiUrl, content);

				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"コメント追加エラー: {response.StatusCode}");
					Console.WriteLine($"詳細: {errorContent}");
					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"コメント追加エラー: {ex.Message}");
				return false;
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

					// URLの表示（フォールバック付き）
					if (!string.IsNullOrEmpty(result.HtmlUrl))
					{
						Console.WriteLine($"  URL: {result.HtmlUrl}");
					}
					else
					{
						// URLが取得できない場合は手動で構築
						var manualUrl = $"https://github.com/{_config.RepoOwner}/{_config.RepoName}/issues/{result.Number}";
						Console.WriteLine($"  URL: {manualUrl}");
					}

					// コメントがある場合は追加
					if (issueData.Comments != null && issueData.Comments.Count > 0)
					{
						Console.WriteLine($"  コメントを追加中... ({issueData.Comments.Count}件)");

						for (int commentIndex = 0; commentIndex < issueData.Comments.Count; commentIndex++)
						{
							var comment = issueData.Comments[commentIndex];
							var commentResult = await AddCommentAsync(result.Number, comment.Body);

							if (commentResult)
							{
								Console.WriteLine($"    ✓ コメント {commentIndex + 1} を追加しました");
							}
							else
							{
								Console.WriteLine($"    ✗ コメント {commentIndex + 1} の追加に失敗しました");
							}
							await Task.Delay(500);
						}
					}

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