module RepoScanner

open System.Timers
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

    let scanRepos ct =
        task {
            let filterConfiguration = getDefaultFilterConfiguration lastScan
            let! issues = getIssues filterConfiguration
            logger.LogInformation $"Found {Seq.length issues} interesting issues before local filter"
            let issuesToSend = issues
                               |> Seq.where (Filter.checkIgnoreFilters filterConfiguration.filters)
            logger.LogInformation $"Found {Seq.length issuesToSend} interesting issues"
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