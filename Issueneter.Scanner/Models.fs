module Issueneter.Scanner.Models

open System

type ScanTask = {
    ScanTarget: string
    Timeout: TimeSpan
    Triggers: string array
}