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
open Github
open Microsoft.Extensions.Logging

type ScannerConfiguration = {
    ScannerTimeOut: TimeSpan
}

type Scanner(telegram: IssueneterTelegramBot, configuration: ScannerConfiguration, logger: ILogger<Scanner>) =
    inherit BackgroundService()
    let mutable lastScan = DateTimeOffset.UtcNow
    let credentionals = Credentials("_", "_")
    let client = 
        let value = GitHubClient(ProductHeaderValue("Issueneter"))
        value.Credentials <- credentionals
        value

    let needToSendIssue (issue : Issue) = task {
        let! events = getIssueEvents client issue
        return events
            |> Seq.sortByDescending ^ fun (x: TimelineEventInfo) -> x.CreatedAt
            |> Seq.exists ^ fun x -> x.Event.Value = EventInfoState.Labeled && x.CreatedAt > lastScan
    }

    let proceedIssues (issues : Issue list) = unitTask {
        let sendInterestingIssues issues = task {
            for issue in issues do
                do! telegram.sendIssue issue
        }

        let! interestingIssues = getUpdatedByLabelingIssues issues needToSendIssue
        logger.LogInformation($"Remaining calls after filtering {issues.Length} issues: {client.GetLastApiInfo().RateLimit.Remaining}")
        do! sendInterestingIssues interestingIssues
    }

    let job (ctx : CancellationToken) = task {
        while not ctx.IsCancellationRequested do
            try
                let! response = getFilters lastScan
                                |> getAllIssues client
                logger.LogInformation($"Remaining calls after getting issues: {client.GetLastApiInfo().RateLimit.Remaining}")
                let log = response |> Seq.fold (fun x y -> x + y.Count.ToString() + " ") "";
                logger.LogInformation $"Found {log}"
                let scanTime = DateTimeOffset.UtcNow
                do! response
                    |> Array.map ^ fun x -> proceedIssues <| List.ofSeq x
                    |> Task.WhenAll

                lastScan <- scanTime
                do! Task.Delay(configuration.ScannerTimeOut)
            with
            | :? RateLimitExceededException as ex -> 
                logger.LogError $"Rate limit reached"
                let diff = ex.Reset - DateTimeOffset.UtcNow
                if diff > TimeSpan.Zero then
                    logger.LogInformation $"Go to sleep until rate reset: {ex.Reset}"
                    do! Task.Delay(diff)
                    logger.LogInformation "Wake up after sleep"
                
    }

    override _.ExecuteAsync ctx =
        job ctx 
        |> ignore
        Task.CompletedTask