using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ConsoleApp1.Models;

namespace ConsoleApp1.Services
{
	/// <summary>
	/// ファイル操作を行うサービスの実装
	/// </summary>
	public class FileService : IFileService
	{
		/// <summary>
		/// JSONファイルからIssueデータを読み込む
		/// </summary>
		/// <param name="filePath">JSONファイルのパス</param>
		/// <returns>Issueデータのリスト</returns>
		public List<IssueData>? LoadIssuesFromJson(string filePath)
		{
			try
			{
				if (!File.Exists(filePath))
				{
					Console.WriteLine($"ファイルが見つかりません: {filePath}");
					return null;
				}

				var json = File.ReadAllText(filePath);
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					AllowTrailingCommas = true,
					ReadCommentHandling = JsonCommentHandling.Skip
				};

				var issues = JsonSerializer.Deserialize<List<IssueData>>(json, options);

				Console.WriteLine($"JSONファイルを読み込みました: {filePath}");
				return issues;
			}
			catch (JsonException jsonEx)
			{
				Console.WriteLine($"JSON形式エラー: {jsonEx.Message}");
				return null;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"JSONファイルの読み込みエラー: {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// ファイルが存在するかチェックする
		/// </summary>
		/// <param name="filePath">ファイルパス</param>
		/// <returns>ファイルが存在するかどうか</returns>
		public bool FileExists(string filePath)
		{
			return File.Exists(filePath);
		}
	}
}