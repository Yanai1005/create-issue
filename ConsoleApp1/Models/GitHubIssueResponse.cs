namespace ConsoleApp1.Models
{
	/// <summary>
	/// GitHub API Issue作成レスポンス用のモデル
	/// </summary>
	public class GitHubIssueResponse
	{
		/// <summary>
		/// Issue番号
		/// </summary>
		public int Number { get; set; }

		/// <summary>
		/// IssueのHTML URL
		/// </summary>
		public string HtmlUrl { get; set; } = string.Empty;

		/// <summary>
		/// Issueのタイトル
		/// </summary>
		public string Title { get; set; } = string.Empty;

		/// <summary>
		/// Issueの状態（open, closed）
		/// </summary>
		public string State { get; set; } = string.Empty;
	}
}