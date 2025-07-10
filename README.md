
# 概要
GitHubのREST APIを使用してJSONファイルからIssueを一括登録するC#コンソールアプリケーションです
## 前提条件
.NET 8.0 Runtime
Visual Studio 2022 
GitHub Personal Access Token (PAT)
対象リポジトリへの書き込み権限

## セットアップ
1. リポジトリのクローン
```bash
git clone https://github.com/Yanai1005/create-issue
cd createissue
```
3. プロジェクトのビルド
```bash
cd ConsoleApp1
dotnet build
```
3.ConsoleApp1/appsettings.json を作成：
```bash
{
  "GitHubSettings": {
    "Token": "ghp_your_github_token_here",
    "RepoOwner": "your-username",
    "RepoName": "your-repository-name",
    "ApiBaseUrl": "https://api.github.com",
    "UserAgent": "ConsoleApp1/1.0"
  },
  "FileSettings": {
    "IssuesJsonPath": "issues.json"
  },
  "ApplicationSettings": {
    "ApiCallDelayMs": 1000,
    "ShowDebugInfo": false
  }
}
```
4. GitHub Personal Access Token (PAT) の取得
- GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
- "Generate new token" をクリック
- 必要なスコープを選択：
パブリックリポジトリ: public_repo
プライベートリポジトリ: repo
- 生成されたトークンを appsettings.json に設定


##  使用方法
1.ConsoleApp1/issues.json にIssueデータを定義　　　
2. アプリケーションの実行　　　
```bash
cd ConsoleApp1
dotnet run
```
または Visual Studio で F5 キーを押して実行
##　実行例
![image](https://github.com/user-attachments/assets/ce7e460c-ee3c-44db-975a-f8e8ab802e83)
