using System;

namespace ConsoleApp1.Models
{
	/// <summary>
	/// GitHub API Rate Limit情報を格納するモデル
	/// </summary>
	public class RateLimitInfo
	{
		/// <summary>
		/// 1時間あたりのAPI制限数
		/// </summary>
		public int Limit { get; set; }

		/// <summary>
		/// 残りAPI呼び出し数
		/// </summary>
		public int Remaining { get; set; }

		/// <summary>
		/// Rate Limitがリセットされる時刻（Unix timestamp）
		/// </summary>
		public long ResetTimestamp { get; set; }

		/// <summary>
		/// 使用済みAPI呼び出し数
		/// </summary>
		public int Used => Limit - Remaining;

		/// <summary>
		/// 使用率（パーセント）
		/// </summary>
		public double UsagePercentage => Limit > 0 ? (double)Used / Limit * 100 : 0;

		/// <summary>
		/// Rate Limit情報の文字列表現
		/// </summary>
		/// <returns>フォーマットされた文字列</returns>
		public override string ToString()
		{
			return $"使用済み: {Used}/{Limit} ({UsagePercentage:F1}%), 残り: {Remaining}";
		}

		/// <summary>
		/// 簡潔な文字列表現
		/// </summary>
		/// <returns>簡潔なフォーマットの文字列</returns>
		public string ToShortString()
		{
			return $"{Used}/{Limit} (残り: {Remaining})";
		}

		/// <summary>
		/// 2つのRateLimit情報の差分を計算
		/// </summary>
		/// <param name="before">変更前の情報</param>
		/// <param name="after">変更後の情報</param>
		/// <returns>使用されたAPI呼び出し数</returns>
		public static int CalculateUsage(RateLimitInfo before, RateLimitInfo after)
		{
			if (before == null || after == null)
				return 0;

			return before.Remaining - after.Remaining;
		}
	}
}