module Filtering
    open System
    open IssueLabels
    open Octokit

    type Filter =
        | IssueLabelFilter of IssueLabel
        | IssueIgnoreLabelFilter of IssueLabel

    module Filter =
        let getLabelsForGithub (filter : Filter) : (IssueLabel Option) =
            match filter with
            | IssueLabelFilter gf -> Some gf
            | IssueIgnoreLabelFilter _ -> None

        let getLabelsForLocalIgnore (filter : Filter) : (IssueLabel Option) =
            match filter with
            | IssueLabelFilter _ -> None
            | IssueIgnoreLabelFilter i -> Some i

        let checkIgnoreFilters (filters : seq<Filter>) (issue : Issue) : bool =
            let ignoreLabels = filters |> Seq.map getLabelsForLocalIgnore |> Seq.choose id |> Seq.map IssueLabel.toString
            issue.Labels |> Seq.exists (fun l -> ignoreLabels |> Seq.contains l.Name) |> not


    type FilterConfiguration = {
        since : DateTimeOffset
        filters : seq<Filter>
    }

    let getDefaultFilters =
        [|
            IssueLabelFilter ApiReadyForReview
            IssueLabelFilter UpForGrabs
            IssueLabelFilter Easy
            IssueLabelFilter ApiApproved
        |]

    let getDefaultFilterConfiguration (since: DateTimeOffset) =
            {
                since = since
                filters = getDefaultFilters
            }