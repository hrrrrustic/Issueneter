module Issueneter.Requests.CreateScan

open Issueneter.Requests.BaseTypes

type LabelingType =
    | Or
    | And

type Label = {
    Title: string
}

type AuthorTrigger = {
    AuthorName: string
}

type LabelingTrigger = {
    Labels: Label array
    Type: LabelingType
}

type ScanTrigger = {
    Author: AuthorTrigger array option
    Label: LabelingTrigger array option
}

type ScanType =
    | Issue
    | PullRequest

type Scan = {
    Type: ScanType
    Triggers: ScanTrigger array
}

type CreateScanFullConfig = {
    Repository : RepositoryUrl
    Chat: ChatId
    Scans: Scan array
}

type CreationError =
    | InvalidRepository of reason: string
    | InvalidTrigger of reason: string
    | NotSupported of message: string * estimation: string