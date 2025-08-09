# Engineering Insights Full Table

| # | Insight Name | Description | API Endpoint(s) | Sample cURL Query (replace placeholders) |
| --- | --- | --- | --- | --- |
| 1 | Velocity | Story points completed per sprint | Jira: `/rest/api/3/search` | `curl -u user:token -X POST "https://your-domain.atlassian.net/rest/api/3/search" -d '{"jql":"sprint = X AND issuetype = Story","fields":["customfield_10004"]}'` |
| 2 | Cycle Time | Time from work start to feature delivery | Jira: `/rest/api/3/issue/{issueId}`<br>GitHub: `/pulls` | Jira: `curl -u user:token "https://your-domain.atlassian.net/rest/api/3/issue/{issueId}`"<br>GitHub: `curl -H "Authorization: token TOKEN" https://api.github.com/repos/{owner}/{repo}/pulls` |
| 3 | Defect Ratio | Defects versus completed tasks | Jira: `/rest/api/3/search` | `curl -u user:token -X POST "https://your-domain.atlassian.net/rest/api/3/search" -d '{"jql":"type = Bug OR type = Task"}'` |
| 4 | Sprint Burndown | Remaining work over sprint timeline | Jira: `/rest/agile/1.0/board/{boardId}/sprint/{sprintId}/burndown` | `curl -u user:token "https://your-domain.atlassian.net/rest/agile/1.0/board/{boardId}/sprint/{sprintId}/burndown"` |
| 5 | Issue Throughput | Issues completed in a period | Jira: `/rest/api/3/search` | `curl -u user:token -X POST "https://your-domain.atlassian.net/rest/api/3/search" -d '{"jql":"status = Done AND resolved >= -7d"}'` |
| 6 | Lead Time | Time from issue creation to completion | Jira: `/rest/api/3/issue/{issueId}?expand=changelog` | `curl -u user:token "https://your-domain.atlassian.net/rest/api/3/issue/{issueId}?expand=changelog"` |
| 7 | Pull Request Merge Time | Time from PR creation to merge | GitHub: `/pulls?state=closed` | `curl -H "Authorization: token TOKEN" https://api.github.com/repos/{owner}/{repo}/pulls?state=closed` |
| 8 | Code Review Time | Time spent in PR review | GitHub: `/pulls/{pull_number}/reviews` | `curl -H "Authorization: token TOKEN" https://api.github.com/repos/{owner}/{repo}/pulls/{pull_number}/reviews` |
| 9 | PR Size (Lines Changed) | Lines added/removed per PR | GitHub: `/pulls/{pull_number}/files` | `curl -H "Authorization: token TOKEN" https://api.github.com/repos/{owner}/{repo}/pulls/{pull_number}/files` |
| 10 | Commit Frequency | Commits by repo/dev over time | GitHub: `/commits` | `curl -H "Authorization: token TOKEN" "https://api.github.com/repos/{owner}/{repo}/commits?author=username&since=YYYY-MM-DD&until=YYYY-MM-DD"` |
| 11 | Branch Activity | Active branches and merges | GitHub: `/branches` | `curl -H "Authorization: token TOKEN" https://api.github.com/repos/{owner}/{repo}/branches` |
| 12 | Issue Aging | Time issues remain in status | Jira: `/rest/api/3/issue/{issueId}?expand=changelog` | `curl -u user:token "https://your-domain.atlassian.net/rest/api/3/issue/{issueId}?expand=changelog"` |
| 13 | Reopened Issues | Count of reopened issues | Jira: `/rest/api/3/search` | `curl -u user:token -X POST "https://your-domain.atlassian.net/rest/api/3/search" -d '{"jql":"status WAS Done AND status = Reopened"}'` |
| 14 | Blocked Issues | Flagged/blocked issues | Jira: `/rest/api/3/search` | `curl -u user:token -X POST "https://your-domain.atlassian.net/rest/api/3/search" -d '{"jql":"Flagged = Impediment"}'` |
| 15 | Test Coverage Trends | Coverage over time | Your CI/coverage tool API | See your code coverage provider's badge or API |
| 16 | Team Workload Balance | Issues assigned per engineer | Jira: `/rest/api/3/search` | `curl -u user:token -X POST "https://your-domain.atlassian.net/rest/api/3/search" -d '{"jql":"assignee = username"}'` |
| 17 | Incident Frequency | Number of bugs/incidents | Jira: `/rest/api/3/search` | `curl -u user:token -X POST "https://your-domain.atlassian.net/rest/api/3/search" -d '{"jql":"issuetype = Bug"}'` |
| 18 | Deployment Frequency | Frequency of deployments/releases | GitHub Actions: `/actions/runs`<br>GitHub Releases: `/releases` | `curl -H "Authorization: token TOKEN" https://api.github.com/repos/{owner}/{repo}/actions/runs` |
| 19 | Time to Restore Service | Incident resolution duration | Jira: `/rest/api/3/search` | `curl -u user:token -X POST "https://your-domain.atlassian.net/rest/api/3/search" -d '{"jql":"issuetype = Incident"}'` |
| 20 | Customer Issue Resolution Times | Customer tickets -- time to resolve | Jira Service Desk API | `curl -u user:token "https://your-domain.atlassian.net/rest/servicedeskapi/request?status=resolved"` |
| 21 | Cross-Team Bottlenecks | Blocked issues due to dependencies | Jira: Issue links API | `curl -u user:token "https://your-domain.atlassian.net/rest/api/3/issue/{issueId}/remotelink"` |
| 22 | Meetings vs Coding Time | Balance of meetings and coding | Calendar API (Google/MS) + GitHub activity | Integrate calendar events with GitHub push events |
| 23 | Untriaged Issues | Created but unassigned issues | Jira: `/rest/api/3/search` | `curl -u user:token -X POST "https://your-domain.atlassian.net/rest/api/3/search" -d '{"jql":"assignee IS EMPTY"}'` |
| 24 | Code Churn Rate | Lines added/deleted, revisited | GitHub: `/commits` | `curl -H "Authorization: token TOKEN" https://api.github.com/repos/{owner}/{repo}/commits?author=username` |
| 25 | Code Ownership Metrics | Main contributors to files | GitHub: `/commits?path={file}` | `curl -H "Authorization: token TOKEN" https://api.github.com/repos/{owner}/{repo}/commits?path={file}` |
| 26 | PR Review Comments | PR review comments cadence | GitHub: `/pulls/{pull_number}/reviews` | `curl -H "Authorization: token TOKEN" https://api.github.com/repos/{owner}/{repo}/pulls/{pull_number}/reviews` |
| 27 | Automation Coverage | Status of CI/CD automation | GitHub Actions: `/actions/runs` | `curl -H "Authorization: token TOKEN" https://api.github.com/repos/{owner}/{repo}/actions/runs` |
| 28 | Build Failures Frequency | CI build failures frequency | GitHub Actions: `/actions/runs` | Filter runs for `"conclusion":"failure"` |
| 29 | Sprint Goal Completion Ratio | Ratio of planned vs completed goals | Jira Agile API: `/board/{boardId}/sprint/{sprintId}/report` | Compare committed vs completed story points via Jira endpoints |
| 30 | High Priority Issue Aging | Aging of critical/urgent issues | Jira: `/rest/api/3/search` | `curl -u user:token -X POST "https://your-domain.atlassian.net/rest/api/3/search" -d '{"jql":"priority = High"}'` |
| 31 | Blocked PRs | PRs stalled due to unresolved reviews | GitHub: `/pulls?state=open&review_requested=true` | `curl -H "Authorization: token TOKEN" https://api.github.com/repos/{owner}/{repo}/pulls?state=open&review_requested=true` |
| 32 | Feature Usage Feedback Loops | Link customer feedback to GitHub/Jira issues | GitHub & Jira: issue linking/metainfo | Leverage linked issue comments or metadata tags |
| 33 | Code Review Participation | Number of reviewers per PR | GitHub: `/pulls/{pull_number}/reviews` | `curl -H "Authorization: token TOKEN" https://api.github.com/repos/{owner}/{repo}/pulls/{pull_number}/reviews` |
| 34 | Developer Activity Heatmap | Commits/PRs by time | GitHub: `/commits`, `/pulls` | Aggregate dates from API responses |
| 35 | Epic Progress Tracking | Progress on epics and stories | Jira: `/rest/api/3/search` | `curl -u user:token -X POST "https://your-domain.atlassian.net/rest/api/3/search" -d '{"jql":"Epic Link = EPIC-123"}'` |
| 36 | Security Vulnerabilities | Open/closed security issues | GitHub Security: `/security/advisories`, `/vulnerability-alerts` | `curl -H "Authorization: token TOKEN" https://api.github.com/repos/{owner}/{repo}/security/advisories` |
| 37 | Refactoring Efforts | Labeled refactoring changes/issues | GitHub & Jira: label/issue filter | Query PRs or issues with `refactor` label |
| 38 | Sprint Predictability | Delivered vs committed in sprint | Jira Agile API: `/board/{boardId}/sprint/{sprintId}/report` | Compare committed and completed story points |
| 39 | Avg. Time in Code Review | PR time between review request and merge | GitHub: `/pulls/{pull_number}` + `/reviews` | Calculate delta between review requested and merged timestamps |
| 40 | Contributor Onboarding Time | Time to productivity for new contributor | Jira: `/users`<br>GitHub: `/commits` | Correlate user creation date with first commit event |
| 41 | Feature Toggle Usage | Usage of feature toggles via issues/PR meta | Jira custom fields<br>GitHub tags | Track conventions in fields or labels |
| 42 | Documentation Coverage | PRs/issues relating to docs | GitHub: `/issues`, `/pulls` | Filter by `docs` label or issue type |
| 43 | Code Merge Conflicts Frequency | Merge conflict indicator on PRs | GitHub: `/pulls/{pull_number}` | Check `"mergeable":false` field in PR details |
| 44 | Test Automation Failures | Failing automation runs | GitHub Actions: `/actions/runs` | Filter runs for `"conclusion":"failure"` and map to linked Jira issues |
| 45 | Release Notes Completeness | PRs/issues with release note attached | GitHub: `/pulls`, Jira | Filter by `release-note` label or custom field |
| 46 | Dependency Updates Frequency | How often dependencies are updated | GitHub: `/pulls`, `/issues` | Query for `dependency` label or keyword |
| 47 | Technical Debt Issues | Issues tagged as tech debt | Jira: `/rest/api/3/search` | `curl -u user:token -X POST "https://your-domain.atlassian.net/rest/api/3/search" -d '{"jql":"labels = tech-debt"}'` |
| 48 | Developer Satisfaction Proxy | Developer feedback via incidents/surveys | Jira custom fields, survey integrations | Use an internal survey tool or proxy incident rate |
| 49 | Cross-Repo Work Coordination | Linked PRs/Jira across repositories | GitHub: `/search/issues`<br>Jira: issue links | Query GitHub issues/PRs across repos and follow Jira remotelink API |
| 50 | Team Collaboration Efficiency | Resolution speed, comments frequency | Jira: `/rest/api/3/search`<br>GitHub: `/issues/comments` |