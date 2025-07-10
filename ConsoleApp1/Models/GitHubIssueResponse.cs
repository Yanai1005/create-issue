using System.Text.Json.Serialization;

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
        [JsonPropertyName("number")]
        public int Number { get; set; }

        /// <summary>
        /// IssueのHTML URL
        /// </summary>
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;

        /// <summary>
        /// Issueのタイトル
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Issueの状態（open, closed）
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;
    }
}