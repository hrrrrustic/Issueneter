module Issueneter.WebAPI

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open TelegramBot
open RepoScanner


let configureApp (app : IApplicationBuilder) =
    ()

let configureServices (services : IServiceCollection) =
    services.AddLogging() |> ignore
    services.AddSingleton<IssueneterTelegramBot>() |> ignore
    services.AddHostedService<Scanner>() |> ignore
    services.AddSingleton<ScannerConfiguration>({ScannerTimeOut = TimeSpan.FromSeconds(float 60)}) |> ignore

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    0