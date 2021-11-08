namespace Issueneter

open FSharp.Configuration
open Microsoft.Extensions.Configuration
open Telegram.Bot
open Telegram.Bot.Types

module TelegramBot =

    type IssueneterTelegramBot(config : IConfiguration) =
        let tgClient = TelegramBotClient(config.GetSection("Token").Value)
        let chatId = ChatId("-1001740532257")
        member _.sendIssue text = tgClient.SendTextMessageAsync(chatId, text)