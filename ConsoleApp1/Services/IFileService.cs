using System.Collections.Generic;
using ConsoleApp1.Models;

namespace ConsoleApp1.Services
{
	/// <summary>
	/// ファイル操作を行うサービスのインターフェース
	/// </summary>
	public interface IFileService
	{
		/// <summary>
		/// JSONファイルからIssueデータを読み込む
		/// </summary>
		/// <param name="filePath">JSONファイルのパス</param>
		/// <returns>Issueデータのリスト</returns>
		List<IssueData>? LoadIssuesFromJson(string filePath);

		/// <summary>
		/// ファイルが存在するかチェックする
		/// </summary>
		/// <param name="filePath">ファイルパス</param>
		/// <returns>ファイルが存在するかどうか</returns>
		bool FileExists(string filePath);
	}
}