module Issueneter.ChannelsOutput.Models

type SucceedScanItem = {
    ItemLink: string
    CustomMessage: string option
    TriggeredBy: string array
}