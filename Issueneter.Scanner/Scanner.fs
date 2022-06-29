namespace Issueneter.Scanner

open Issueneter.Contracts.Events
open System
open System.Threading.Tasks

type RepositoryScanner() =
    let proceedTask scanTask = task {
        let! result = Task.FromResult 1
        
        let scanResult = {
                ScanTime = DateTime.UtcNow
            }
        ()
    }

