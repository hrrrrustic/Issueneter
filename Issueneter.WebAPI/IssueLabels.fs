module IssueLabels
    type IssueLabel =
    | ApiApproved
    | ApiReadyForReview
    | UpForGrabs
    | Easy
    | InPr

    module IssueLabel =
        let toString label = match label with
            | ApiApproved -> "api-approved"
            | ApiReadyForReview -> "api-ready-for-review"
            | UpForGrabs -> "up-for-grabs"
            | Easy -> "easy"
            | InPr -> "in-pr"