module Issueneter.WebAPI

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open TelegramBot
open RepoScanner
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http

let hande next (ctx : HttpContext) = task {
    let bot = ctx.GetService<IssueneterTelegramBot>()
    return! json "ok" next ctx
}

let webApp =
    choose [
        route "/ping"   >=> GET >=> hande
        route "/"       >=> htmlFile "/pages/index.html" ]

let configureApp (app : IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app.UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    // Add Giraffe dependencies
    services.AddGiraffe() |> ignore
    services.AddSingleton<IssueneterTelegramBot>() |> ignore
    services.AddHostedService<Scanner>() |> ignore

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