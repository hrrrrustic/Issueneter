namespace Issueneter

open Telegram.Bot
open Telegram.Bot.Types


module TelegramBot =

    type IssueneterTelegramBot() =
        let tgClient = TelegramBotClient("2086755576:AAHEVSgycR0YQyCIdJ2-WDU-7cu96QoDm2Q")
        let chatId = ChatId("336389404")
        member _.sendIssue text = tgClient.SendTextMessageAsync(chatId, text)