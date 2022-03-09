namespace Issueneter

open FSharp.Configuration
open Microsoft.Extensions.Configuration
open Telegram.Bot
open System.Threading.Tasks
open Telegram.Bot.Types
open Octokit


module TelegramBot =

    type IssueneterTelegramBot(config : IConfiguration) =
        let tgClient = TelegramBotClient("2113832464:AAFSnhElObzlFIHKiwYzLWPoayr89kxcCd0")
        let chatId = ChatId("-1001740532257")

        let getIssueLink (issue: Issue) =
            $"[изи]({issue.HtmlUrl})"

        let sendIssue issue = tgClient.SendTextMessageAsync(chatId, getIssueLink issue, Enums.ParseMode.Markdown) :> Task

        member _.sendIssues (issues : seq<Issue>) =
            task {
                for issue in issues do
                    do! sendIssue issue
            }