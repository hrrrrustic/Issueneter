module RepoScanner

open System.Timers
open IssueLabels
open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open System
open Octokit
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

    let checkIssueEvents (filters: seq<Filter>) (issue: Issue)  = task {
        let! issueEvents = getIssueEvents issue
        let labels = filters |> Seq.map Filter.getLabelsForGithub |> Seq.choose id |> Seq.map IssueLabel.toString
        issueEvents
        |> Seq.where (fun e -> e.Event.Value.Equals(EventInfoState.Labeled) && e.CreatedAt > lastScan)
        |> Seq.iter (fun x -> logger.LogInformation $"{x.Label.Name} - {x.CreatedAt}")
        return issueEvents
        |> Seq.where (fun e -> e.Event.Value.Equals(EventInfoState.Labeled) && e.CreatedAt > lastScan)
        |> Seq.exists (fun e -> labels |> Seq.contains e.Label.Name)
    }

    let scanRepos ct =
        task {
            let filterConfiguration = getDefaultFilterConfiguration lastScan
            let! issues = getIssues filterConfiguration
            logger.LogInformation $"Found {Seq.length issues} interesting issues before local filter"
            let issuesToSend = issues
                               |> Seq.where (Filter.checkIgnoreFilters filterConfiguration.filters)
            logger.LogInformation $"Found {Seq.length issuesToSend} interesting issues"
            for issue in issuesToSend do
                let! needToSend = checkIssueEvents filterConfiguration.filters issue
                if needToSend then
                    logger.LogInformation $"Sending issue {issue.Title}"
                    do! telegram.sendIssues issuesToSend
        }

    override _.ExecuteAsync ct =
        task {
            while not ct.IsCancellationRequested do
                try
                    logger.LogInformation $"Start scanning at {DateTimeOffset.UtcNow} (last scan - {lastScan})"
                    do! scanRepos ct
                    lastScan <- DateTimeOffset.UtcNow
                    logger.LogInformation $"End scanning at {lastScan}"
                    logger.LogInformation $"Remaining calls {client.GetLastApiInfo().RateLimit.Remaining} until {client.GetLastApiInfo().RateLimit.Reset}"
                    do! Task.Delay configuration.ScannerTimeOut
                with
                | :? RateLimitExceededException as ex ->
                    logger.LogInformation "Rate limit reached"
                    let diff = ex.Reset - DateTimeOffset.UtcNow
                    if diff > TimeSpan.Zero then
                        logger.LogInformation $"Go to sleep until rate reset: {ex.Reset}"
                        do! Task.Delay(diff)
                        logger.LogInformation "Wake up after sleep"
        }