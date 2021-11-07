module RepoScanner

open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open System
open Octokit
open Issueneter.TelegramBot

type Scanner(telegram: IssueneterTelegramBot) =
    inherit BackgroundService()
    let mutable lastScan = DateTimeOffset.UtcNow
    let getapprovedFilter() = 
        let filter = 
            RepositoryIssueRequest(
                Filter = IssueFilter.All, 
                State = ItemStateFilter.Open, 
                SortProperty = IssueSort.Updated, 
                SortDirection = SortDirection.Descending, 
                Since = lastScan)
        filter.Labels.Add "api-approved"
        filter

    let client = GitHubClient(ProductHeaderValue("Issueneter"))

    let getIssues() = client.Issue.GetAllForRepository("kysect", "Issueneter", getapprovedFilter()) |> Async.AwaitTask
    let rec proceedIssues (issues : Issue list) =
        let getIssueMessage (issue: Issue) =
            issue.HtmlUrl.ToString()
        let proceedIssue (issue: Issue)=
            getIssueMessage issue 
            |> telegram.sendIssue
            |> ignore

        match issues with
        | head :: other ->
            proceedIssue head
            proceedIssues other
        | _ -> ()

    let job = async {
        while true do
            let! response = getIssues()
            let issues = List.ofSeq response
            lastScan <- DateTimeOffset.UtcNow
            proceedIssues issues
            do! Task.Delay(40000) |> Async.AwaitTask
    }

    override _.ExecuteAsync ctx = 
        job 
        |> Async.StartAsTask 
        |> ignore
        Task.CompletedTask

            