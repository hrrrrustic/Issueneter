namespace Issueneter

open FSharp.Configuration
open Microsoft.Extensions.Configuration
open Telegram.Bot
open Telegram.Bot.Types

module TelegramBot =

    type IssueneterTelegramBot(config : IConfiguration) =
        let tgClient = TelegramBotClient(config.GetSection("Token").Value)
        let chatId = ChatId("412750554")
        member _.sendIssue text = tgClient.SendTextMessageAsync(chatId, text)