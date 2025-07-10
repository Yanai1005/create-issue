using System.Collections.Generic;

namespace ConsoleApp1.Models
{
	/// <summary>
	/// JSONファイルから読み込むIssueデータのモデル
	/// </summary>
	public class IssueData
	{
		/// <summary>
		/// Issueのタイトル
		/// </summary>
		public string Title { get; set; } = string.Empty;

		/// <summary>
		/// Issueの本文
		/// </summary>
		public string Body { get; set; } = string.Empty;

		/// <summary>
		/// ラベルのリスト
		/// </summary>
		public List<string> Labels { get; set; } = new List<string>();

		/// <summary>
		/// アサイニーのリスト
		/// </summary>
		public List<string> Assignees { get; set; } = new List<string>();

		/// <summary>
		/// マイルストーンID（オプショナル）
		/// </summary>
		public int? Milestone { get; set; }
	}
}