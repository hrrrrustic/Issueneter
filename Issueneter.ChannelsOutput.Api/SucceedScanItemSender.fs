module Issueneter.ChannelsOutput.Api

open Issueneter.ChannelsOutput.Models

type SucceedScanItemSender =
    abstract member SendItem: destination: string * item: SucceedScanItem -> unit
 
