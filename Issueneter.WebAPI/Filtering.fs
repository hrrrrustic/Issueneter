module Filtering
    open Octokit
    open IssueLabels
    open System
    open FSharp.Control.Tasks
    open System.Threading.Tasks


    let inline toString label =
        match label with
        | ApiReadyForReview -> "api-ready-for-review"
        | ApiApproved -> "api-approved"
        | UpForGrabs -> "up-for-grabs"
        | Easy -> "easy"

    let defaultFilterWithLabel (label: EasyIssueLabel) =
        let filter = RepositoryIssueRequest(
                        Filter = IssueFilter.All, 
                        State = ItemStateFilter.Open, 
                        SortProperty = IssueSort.Updated, 
                        SortDirection = SortDirection.Descending)
        filter.Labels.Add <| toString label
        {BaseFilter = filter; Label = label}

    let private filters = 
        [|
            defaultFilterWithLabel ApiReadyForReview
            defaultFilterWithLabel UpForGrabs
            defaultFilterWithLabel Easy
            defaultFilterWithLabel ApiApproved
        |]
    let getFilters (since : DateTimeOffset) =
        filters |>
        Array.map ^ fun x -> x.BaseFilter.Since <- since; x
    
    let rec getUpdatedByLabelingIssues (issues : Issue list) (filter: FoundIssue -> Task<bool>) = task {
        match issues with
        | [] -> return []
        | issue::other -> 
            let! valid = filter issue
            let! others = getUpdatedByLabelingIssues other filter

            return match valid with
                    | true -> issue::others
                    | false -> others
    }