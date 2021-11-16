module Filtering
    open Octokit
    open IssueLabels
    open System

    let defaultFilterWithLabel (label: EasyIssueLabel) =
        let filter = RepositoryIssueRequest(
                        Filter = IssueFilter.All, 
                        State = ItemStateFilter.Open, 
                        SortProperty = IssueSort.Updated, 
                        SortDirection = SortDirection.Descending)
        filter.Labels.Add <| label.ToString()
        filter

    let private filters = 
        [|
            defaultFilterWithLabel ApiReadyForReview
            defaultFilterWithLabel UpForGrabs
            defaultFilterWithLabel Easy
            defaultFilterWithLabel ApiApproved

        |]
    let getFilters (since : DateTimeOffset) =
        filters |>
        Array.map ^ fun x -> x.Since <- since; x