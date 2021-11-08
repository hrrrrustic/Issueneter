module RepoScanner

open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open System
open Octokit
open Issueneter.TelegramBot

type Scanner(telegram: IssueneterTelegramBot) =
    inherit BackgroundService()
    let mutable lastScan = DateTimeOffset.UtcNow
    let getApprovedFilter() = 
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

    let getIssues() = client.Issue.GetAllForRepository("kysect", "Issueneter", getApprovedFilter()) |> Async.AwaitTask
    let getIssueComments (issue : Issue) =
            client.Issue.Comment.GetAllForIssue("kysect", "Issueneter", issue.Id, IssueCommentRequest(
                    Since = lastScan
                )) |> Async.AwaitTask
    let rec proceedIssues (issues : Issue list) = async {
        let getIssueLink (issue: Issue) =
            issue.HtmlUrl.ToString()
        
        let isLabelChangeComment (comment : IssueComment) =
            true
        let needToSendIssue (issue : Issue) = async {
                let! comments = getIssueComments issue
                return comments |> List.ofSeq |> Seq.exists isLabelChangeComment
            }
        let proceedIssue (issue: Issue) = async {
            match! needToSendIssue issue with
            | true -> getIssueLink issue |> telegram.sendIssue |> ignore
            | _ -> ()
        }

        match issues with
        | head :: other ->
            do! proceedIssue head
            do! proceedIssues other
        | _ -> ()
    }

    let job = async {
        while true do
            let! response = getIssues()
            let issues = List.ofSeq response
            lastScan <- DateTimeOffset.UtcNow
            do! proceedIssues issues
            do! Task.Delay(20000) |> Async.AwaitTask
    }

    override _.ExecuteAsync ctx = 
        job 
        |> Async.StartAsTask 
        |> ignore
        Task.CompletedTask

            