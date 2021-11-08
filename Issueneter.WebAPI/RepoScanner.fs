module RepoScanner

open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open System
open System.Threading
open Octokit
open FSharp.Control.Tasks
open Issueneter.TelegramBot
open CustomOperators

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

    let getIssues() = client.Issue.GetAllForRepository("kysect", "Issueneter", getApprovedFilter())
    let getIssueEvents (issue : Issue) = 
            client.Issue.Timeline.GetAllForIssue("kysect", "Issueneter", issue.Number)
    let rec proceedIssues (issues : Issue list) = task {
        let getIssueLink (issue: Issue) =
            issue.HtmlUrl.ToString()
        
        let needToSendIssue (issue : Issue) = task {
                let! events = getIssueEvents issue
                return events
                    |> Seq.sortByDescending ^ fun (x: TimelineEventInfo) -> x.CreatedAt
                    |> Seq.exists ^ fun x -> x.Event.Value = EventInfoState.Labeled && x.CreatedAt > lastScan
            }
        let proceedIssue (issue: Issue) = task {
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

    let job (ctx : CancellationToken) = task {
        while not ctx.IsCancellationRequested do
            let! response = getIssues()
            let scanTime = DateTimeOffset.UtcNow
            let issues = List.ofSeq response
            do! proceedIssues issues
            lastScan <- scanTime
            do! Task.Delay(20000)
    }

    override _.ExecuteAsync ctx = 
        job 
        |> ignore
        Task.CompletedTask

            