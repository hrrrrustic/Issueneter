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
        
        let processIssueEvents (issueEvents : seq<string>) =
            issueEvents |> Seq.map (fun e -> $"\[{e}]") |> String.concat " "
        
        let getIssueLink (issue: Issue) =
            $"[изи]({issue.HtmlUrl})"
                    
        let getMessage (issue : Issue) (issueEvents : seq<string>) =
            $"{processIssueEvents issueEvents} - {getIssueLink issue}"

        member _.sendIssue (issue : Issue) (issueEvents : seq<string>) =
            task {
                do! tgClient.SendTextMessageAsync(chatId, (getMessage issue issueEvents), Enums.ParseMode.Markdown) :> Task
            }