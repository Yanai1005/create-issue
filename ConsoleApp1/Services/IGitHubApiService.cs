using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApp1.Models;

namespace ConsoleApp1.Services
{
	/// <summary>
	/// GitHub API操作を行うサービスのインターフェース
	/// </summary>
	public interface IGitHubApiService
	{
		/// <summary>
		/// Issueを作成する
		/// </summary>
		/// <param name="issueRequest">Issue作成リクエスト</param>
		/// <returns>作成されたIssueの情報</returns>
		Task<GitHubIssueResponse?> CreateIssueAsync(GitHubIssueRequest issueRequest);

		/// <summary>
		/// 複数のIssueを一括作成する
		/// </summary>
		/// <param name="issues">作成するIssueのリスト</param>
		/// <returns>作成結果（成功数、失敗数）</returns>
		Task<(int successCount, int failCount)> CreateIssuesBatchAsync(List<IssueData> issues);

		/// <summary>
		/// リソースを解放する
		/// </summary>
		void Dispose();
	}
}