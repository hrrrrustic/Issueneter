namespace Issueneter

open FSharp.Configuration
open Microsoft.Extensions.Configuration
open Telegram.Bot
open System.Threading.Tasks
open Telegram.Bot.Types
open Octokit

module TelegramBot =

    type IssueneterTelegramBot(config : IConfiguration) =
        let tgClient = TelegramBotClient(config.GetSection("Token").Value)
        let chatId = ChatId("-1001740532257")

        let getIssueLink (issue: Issue) =
            $"[изи]({issue.HtmlUrl})"


        member _.sendIssue issue = tgClient.SendTextMessageAsync(chatId, getIssueLink issue) :> Task