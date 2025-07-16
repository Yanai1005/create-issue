using ConsoleApp1.Services;

namespace ConsoleApp1
{
	/// <summary>
	/// メインプログラムクラス
	/// </summary>
	class Program
	{
		static async Task Main(string[] args)
		{
			try
			{
				Console.WriteLine("GitHub Issue 一括登録ツール ");

				// 設定の読み込み
				var config = Config.LoadFromJson("../../../appsettings.json");

				// 設定の妥当性チェック
				if (!config.IsValid())
				{
					return;
				}

				// サービスの初期化
				IFileService fileService = new FileService();
				IGitHubApiService gitHubApiService = new GitHubApiService(config);

				try
				{
					// ファイル存在チェック
					if (!fileService.FileExists(config.JsonFilePath))
					{
						Console.WriteLine($"JSONファイルが見つかりません: {config.JsonFilePath}");
						return;
					}

					// JSONファイルの読み込み
					var issues = fileService.LoadIssuesFromJson(config.JsonFilePath);

					if (issues == null || issues.Count == 0)
					{
						Console.WriteLine("有効なIssueデータが見つかりません。");
						return;
					}

					Console.WriteLine($"読み込まれたIssue数: {issues.Count}件");
					Console.WriteLine();

					// 開始時のRate Limit情報を取得
					Console.WriteLine("API Rate Limit情報を取得中...");
					var initialRateLimit = await gitHubApiService.GetRateLimitAsync();

					if (initialRateLimit != null)
					{
						Console.WriteLine($"開始時: {initialRateLimit}");
					}
					else
					{
						Console.WriteLine("Rate Limit情報の取得に失敗しました");
					}
					Console.WriteLine();

					// ユーザー確認
					Console.WriteLine("登録を開始しますか？ (y/n)");
					var input = Console.ReadLine();
					if (input?.ToLower() != "y")
					{
						Console.WriteLine("処理を中止しました。");
						return;
					}

					Console.WriteLine();
					Console.WriteLine("Issue登録を開始します...");

					// 処理開始時刻を記録
					var startTime = DateTime.Now;

					// Issue一括登録処理
					var (successCount, failCount) = await gitHubApiService.CreateIssuesBatchAsync(issues);

					// 処理終了時刻を記録
					var endTime = DateTime.Now;
					var processingTime = endTime - startTime;

					Console.WriteLine("処理が完了しました。");
					Console.WriteLine();

					// 終了時のRate Limit情報を取得
					Console.WriteLine("終了後のAPI Rate Limit情報を取得中...");
					var finalRateLimit = await gitHubApiService.GetRateLimitAsync();

					// 結果表示
					Console.WriteLine("実行結果");
					Console.WriteLine($"処理時間: {processingTime.TotalSeconds:F1}秒");
					Console.WriteLine($"登録結果: 成功 {successCount}件, 失敗 {failCount}件");
					Console.WriteLine();

					// Rate Limit情報の表示
					Console.WriteLine("API Rate Limit使用状況:");

					if (initialRateLimit != null)
					{
						Console.WriteLine($"開始時: {initialRateLimit}");
					}

					if (finalRateLimit != null)
					{
						Console.WriteLine($"終了時: {finalRateLimit}");
					}

					// API使用量の計算と表示
					if (initialRateLimit != null && finalRateLimit != null)
					{
						var apiUsage = Models.RateLimitInfo.CalculateUsage(initialRateLimit, finalRateLimit);

						Console.WriteLine();
						Console.WriteLine($"消費API呼び出し数: {apiUsage}回");

						// Rate Limit残量の警告表示
						if (finalRateLimit.Remaining < 100)
						{
							Console.WriteLine();
							Console.WriteLine($" 残りAPI呼び出し数が少なくなっています: {finalRateLimit.Remaining}回");
						}
					}

					Console.WriteLine();
					if (successCount > 0)
					{
						Console.WriteLine($"GitHub リポジトリ: https://github.com/{config.RepoOwner}/{config.RepoName}/issues");
					}

					Console.WriteLine("すべての処理が完了しました。");
				}
				finally
				{
					// リソースの解放
					gitHubApiService.Dispose();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"予期しないエラーが発生しました: {ex.Message}");
				Console.WriteLine($"スタックトレース: {ex.StackTrace}");
			}

			Console.WriteLine();
			Console.WriteLine("何かキーを押してください...");
			Console.ReadKey();
		}
	}
}