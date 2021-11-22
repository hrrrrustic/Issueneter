module RepoScanner

open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open System
open System.Threading
open Octokit
open FSharp.Control.Tasks
open FSharp.Control
open Issueneter.TelegramBot
open Filtering

type Scanner(telegram: IssueneterTelegramBot) =
    inherit BackgroundService()
    let mutable lastScan = DateTimeOffset.UtcNow
    let client = GitHubClient(ProductHeaderValue("Issueneter"))

    let getIssues (filter: RepositoryIssueRequest) = client.Issue.GetAllForRepository("kysect", "Issueneter", filter)

    let getAllIssues filters =
        filters
        |> Seq.map ^ fun x -> getIssues x
        |> Task.WhenAll

    let getIssueEvents (issue : Issue) = 
            client.Issue.Timeline.GetAllForIssue("kysect", "Issueneter", issue.Number)

    let needToSendIssue (issue : Issue) = task {
        let! events = getIssueEvents issue
        return events
            |> Seq.sortByDescending ^ fun (x: TimelineEventInfo) -> x.CreatedAt
            |> Seq.exists ^ fun x -> x.Event.Value = EventInfoState.Labeled && x.CreatedAt > lastScan
    }

    let proceedIssues (issues : Issue list) = unitTask {
        let sendInterestingIssues issues = task {
            for issue in issues do
                do! telegram.sendIssue issue
        }

        let! interestingIssues = Filtering.getUpdatedByLabelingIssues issues needToSendIssue
        do! sendInterestingIssues interestingIssues
    }

    let job (ctx : CancellationToken) = task {
        while not ctx.IsCancellationRequested do
            let! response = getFilters lastScan
                            |> getAllIssues

            let scanTime = DateTimeOffset.UtcNow
            do! response
                |> Array.map ^ fun x -> proceedIssues <| List.ofSeq x
                |> Task.WhenAll

            lastScan <- scanTime
            do! Task.Delay(TimeSpan.FromSeconds ^ float 20)
    }

    override _.ExecuteAsync ctx = 
        job ctx 
        |> ignore
        Task.CompletedTask        