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
				Console.WriteLine("GitHub Issue 一括登録ツール (JSON設定版)");
				Console.WriteLine("==========================================");

				// 設定の読み込み
				var config = AppConfig.LoadFromJson();

				// デバッグ情報の表示
				if (config.ShowDebugInfo)
				{
					Console.WriteLine($"実行ディレクトリ: {Directory.GetCurrentDirectory()}");
					Console.WriteLine($"実行ファイル場所: {System.Reflection.Assembly.GetExecutingAssembly().Location}");
					Console.WriteLine();
				}

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
					Console.WriteLine($"JSONファイルパス: {config.JsonFilePath}");

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

					// 読み込み結果の表示
					Console.WriteLine($"読み込まれたIssue数: {issues.Count}");
					Console.WriteLine($"対象リポジトリ: {config.RepoOwner}/{config.RepoName}");
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

					// Issue一括登録処理
					var (successCount, failCount) = await gitHubApiService.CreateIssuesBatchAsync(issues);

					// 結果表示
					Console.WriteLine("==============================");
					Console.WriteLine($"登録結果: 成功 {successCount}件, 失敗 {failCount}件");

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