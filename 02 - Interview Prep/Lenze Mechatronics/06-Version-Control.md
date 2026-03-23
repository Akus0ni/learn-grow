# Version Control Systems — Interview Q&A

> Sr. Software Engineer (6+ Years) — Lenze Mechatronics

---

## Git

### Q1: Explain Git's internal object model. What are blobs, trees, commits, and tags?

**Answer:** Git is fundamentally a content-addressable filesystem built on four object types, all stored as SHA-1-hashed, zlib-compressed files inside `.git/objects/`:

- **Blob (Binary Large Object):** Stores the raw contents of a single file — no filename, no metadata. Two files with identical content share the same blob. This is how Git deduplicates storage.
- **Tree:** Represents a directory. A tree object maps filenames (and file modes) to blob SHA-1 hashes, and can reference other tree objects for subdirectories. It is the snapshot of a directory at a point in time.
- **Commit:** Points to exactly one tree (the root tree of the project at that moment), contains metadata (author, committer, timestamp, message), and references zero or more parent commits. The chain of parent references forms the DAG (Directed Acyclic Graph) that is the repository history.
- **Tag (annotated):** Points to a commit (or any other object) and carries a tagger name, date, and message. Lightweight tags are just refs, but annotated tags are full objects stored in the database.

You can inspect these objects directly:

```bash
# View the type of any object
git cat-file -t <sha1>

# Pretty-print the content
git cat-file -p <sha1>

# Walk a commit: see its tree, parent(s), author
git cat-file -p HEAD

# Walk the root tree of that commit
git cat-file -p HEAD^{tree}

# List all objects in the repo
git rev-list --objects --all
```

Understanding this model clarifies why operations like branching are cheap (a branch is just a 41-byte file containing a SHA-1 hash) and why Git can detect renames by comparing blob hashes across trees.

---

### Q2: How do Git branches work internally? Why is branching considered "cheap" in Git?

**Answer:** A Git branch is nothing more than a mutable pointer — a small file under `.git/refs/heads/` that contains the 40-character SHA-1 hash of the commit it points to. When you create a branch, Git writes one file; it does not copy any data.

`HEAD` is a special symbolic reference stored in `.git/HEAD` that points to the currently checked-out branch (e.g., `ref: refs/heads/main`). When you make a new commit, Git:

1. Creates a new commit object whose parent is the commit `HEAD` currently points to.
2. Updates the branch ref file to point to the new commit's SHA-1.

This is why branching and switching are O(1) operations, unlike SVN where branching copies the entire directory tree on the server.

```bash
# See what HEAD points to
cat .git/HEAD
# output: ref: refs/heads/main

# See the commit a branch points to
cat .git/refs/heads/main
# output: a1b2c3d4e5...

# Create a branch (just writes a new ref file)
git branch feature-x

# Verify
cat .git/refs/heads/feature-x
```

Detached HEAD occurs when `HEAD` points directly to a commit SHA rather than a branch name — common when checking out a tag or a specific commit.

---

### Q3: Merge vs Rebase — what are the differences and when should you use each?

**Answer:**

| Aspect | Merge | Rebase |
|--------|-------|--------|
| History | Preserves the full branching topology; creates a merge commit with two parents | Rewrites history to produce a linear sequence of commits |
| Safety | Non-destructive — never alters existing commits | Destructive — rewrites commit SHAs, which can break shared branches |
| Conflict resolution | Resolve once in the merge commit | Resolve per replayed commit (can be more tedious but more granular) |
| Traceability | Shows exactly when and where integration happened | Cleaner log but loses the "when was this integrated" context |

**When to use merge:**
- Integrating a long-lived feature branch into `main` or `develop` — you want the merge commit as a historical marker.
- When other people have already based work on the branch (never rebase shared/published history).
- When you want to preserve the exact sequence of how work happened.

**When to use rebase:**
- Keeping a local feature branch up to date with `main` before opening a PR — gives reviewers a clean, linear diff.
- Cleaning up messy local commit history before sharing (interactive rebase).
- Small, short-lived branches where linear history aids readability.

```bash
# Merge: integrate feature into main
git checkout main
git merge feature-branch

# Rebase: replay feature commits on top of latest main
git checkout feature-branch
git rebase main

# If conflicts arise during rebase
git rebase --continue   # after resolving
git rebase --abort      # to cancel entirely
```

**Golden rule:** Never rebase commits that have been pushed to a shared remote branch that others have pulled.

---

### Q4: Compare Git Flow, GitHub Flow, and Trunk-Based Development. When is each appropriate?

**Answer:**

**Git Flow (Driessen model):**
- Branches: `main`, `develop`, `feature/*`, `release/*`, `hotfix/*`.
- Features branch off `develop`, are merged back into `develop`. Release branches are cut from `develop`, stabilized, then merged into both `main` (tagged) and `develop`. Hotfixes branch off `main` and merge back into both `main` and `develop`.
- Best for: projects with scheduled release cycles, multiple versions in production, or strict QA gates (common in embedded/industrial software like at Lenze).
- Downside: heavyweight; many long-lived branches can lead to painful merges.

**GitHub Flow:**
- Simple: `main` is always deployable. Create a feature branch, push, open a Pull Request, get code review, merge to `main`, deploy.
- Best for: SaaS/web applications with continuous deployment, small to medium teams.
- Downside: no concept of release branches; assumes you can deploy from `main` at any time.

**Trunk-Based Development:**
- All developers commit to `main` (the trunk) frequently — at least daily. Short-lived feature branches (< 2 days) are allowed. Feature flags gate incomplete work.
- Best for: high-performing teams practicing CI/CD, where fast feedback loops are critical.
- Downside: requires excellent CI, feature-flag discipline, and high test coverage.

For an embedded/mechatronics environment with hardware release cycles, Git Flow or a modified trunk-based approach with release branches is often the pragmatic choice.

---

### Q5: Explain `git cherry-pick`, `git stash`, and `git bisect` with practical use cases.

**Answer:**

**`git cherry-pick`** — Applies the diff introduced by a specific commit onto the current branch as a new commit.

```bash
# Apply a single commit from another branch
git cherry-pick abc1234

# Cherry-pick without committing (stage only)
git cherry-pick --no-commit abc1234

# Cherry-pick a range of commits
git cherry-pick A..B
```

*Use case:* A critical bug fix was committed on `develop` but needs to go into a `release/1.2` branch immediately, without merging all of `develop`.

**`git stash`** — Temporarily shelves uncommitted changes (both staged and unstaged) so you can switch context.

```bash
# Stash current work
git stash push -m "WIP: refactoring motor controller"

# List stashes
git stash list

# Apply most recent stash and keep it
git stash apply

# Apply and remove from stash stack
git stash pop

# Stash only specific files
git stash push -m "partial stash" -- src/controller.cs

# Apply a specific stash
git stash apply stash@{2}
```

*Use case:* You are mid-feature and a production hotfix comes in. Stash your work, switch to `hotfix`, fix, commit, switch back, and pop your stash.

**`git bisect`** — Binary search through commit history to find the exact commit that introduced a bug.

```bash
# Start bisecting
git bisect start

# Mark the current (broken) commit as bad
git bisect bad

# Mark a known-good commit
git bisect good v1.0.0

# Git checks out the midpoint; you test and mark
git bisect good   # or git bisect bad

# Repeat until Git identifies the offending commit

# Automate with a test script
git bisect start HEAD v1.0.0
git bisect run ./run_tests.sh

# When done
git bisect reset
```

*Use case:* A regression appeared somewhere in the last 200 commits. Bisect finds it in ~8 steps (log2(200)).

---

### Q6: How do you resolve merge conflicts in Git? Walk through the process.

**Answer:**

When Git cannot automatically reconcile changes from two branches, it marks the conflicting regions in the affected files and halts the merge. The workflow is:

1. **Identify conflicts:**

```bash
git merge feature-branch
# CONFLICT (content): Merge conflict in src/MotorDriver.cs
# Automatic merge failed; fix conflicts and then commit the result.

git status
# Shows files under "Unmerged paths"
```

2. **Understand the conflict markers:** Git inserts markers into the file:

```
<<<<<<< HEAD
    var speed = CalculateSpeed(rpm, gearRatio);
=======
    var speed = ComputeSpeed(rpm, gearRatio, efficiency);
>>>>>>> feature-branch
```

Everything between `<<<<<<< HEAD` and `=======` is the current branch's version. Between `=======` and `>>>>>>>` is the incoming branch's version.

3. **Resolve:** Edit the file to the desired final state, removing all conflict markers.

4. **Stage and commit:**

```bash
git add src/MotorDriver.cs
git commit
# Git auto-populates a merge commit message
```

**Advanced techniques:**

```bash
# Use a merge tool (e.g., VS Code, Beyond Compare, KDiff3)
git mergetool

# See the common ancestor version (3-way diff)
git checkout --conflict=diff3 src/MotorDriver.cs

# Abort the merge entirely
git merge --abort

# Accept one side wholesale
git checkout --theirs src/MotorDriver.cs   # take incoming
git checkout --ours src/MotorDriver.cs     # keep current
```

**Best practices:** Merge frequently from the target branch to keep divergence small. Use clear, small commits so that conflicts are easier to reason about. Configure `merge.conflictstyle = diff3` globally to always see the base version.

---

### Q7: What are Git hooks and how can they be used in a development workflow?

**Answer:** Git hooks are scripts that Git executes automatically before or after specific events (commit, push, merge, etc.). They live in `.git/hooks/` (local) or can be shared via a hooks directory committed to the repo.

**Common hooks:**

| Hook | Trigger | Use Case |
|------|---------|----------|
| `pre-commit` | Before a commit is created | Run linters, formatters, static analysis; reject if checks fail |
| `commit-msg` | After commit message is written | Enforce message format (e.g., Jira ticket prefix) |
| `pre-push` | Before `git push` | Run unit tests; prevent pushing to `main` directly |
| `post-merge` | After a merge completes | Auto-install dependencies, notify team |
| `pre-receive` (server) | Before accepting a push | Enforce branch protection, run CI checks |
| `post-receive` (server) | After accepting a push | Trigger CI/CD pipelines, send notifications |

**Example — enforcing commit message format:**

```bash
#!/bin/bash
# .git/hooks/commit-msg
MSG=$(cat "$1")
if ! echo "$MSG" | grep -qE "^(feat|fix|refactor|docs|test|chore)\(.+\): .{10,}"; then
    echo "ERROR: Commit message must follow Conventional Commits format."
    echo "Example: feat(motor-ctrl): add PID tuning parameter"
    exit 1
fi
```

**Sharing hooks across the team:**

```bash
# Store hooks in the repo
mkdir -p .githooks
# Add hook scripts to .githooks/

# Configure Git to use the shared hooks directory
git config core.hooksPath .githooks
```

Tools like **Husky** (Node.js) or **pre-commit** (Python) simplify hook management across teams. In enterprise settings, server-side hooks on platforms like GitLab or Bitbucket enforce policies that cannot be bypassed locally.

---

### Q8: Explain interactive rebase and squashing commits. When and how would you use them?

**Answer:** Interactive rebase (`git rebase -i`) lets you rewrite, reorder, combine, split, or drop commits before sharing them. It is the primary tool for cleaning up local branch history.

```bash
# Interactively rebase the last 5 commits
git rebase -i HEAD~5
```

Git opens an editor with a list of commits and commands:

```
pick a1b2c3d Add motor controller skeleton
pick e4f5a6b Fix typo in controller
pick 7c8d9e0 WIP: experimenting with PID values
pick 1f2a3b4 Finalize PID tuning
pick 5d6e7f8 Update unit tests for motor controller
```

**Available commands:**
- `pick` — keep the commit as is
- `reword` — keep the commit but edit its message
- `squash` — meld into the previous commit, combine messages
- `fixup` — like squash but discard this commit's message
- `edit` — pause rebase to amend the commit (useful for splitting)
- `drop` — remove the commit entirely

**Squashing example** — combine the typo fix and WIP into the main commits:

```
pick a1b2c3d Add motor controller skeleton
fixup e4f5a6b Fix typo in controller
fixup 7c8d9e0 WIP: experimenting with PID values
pick 1f2a3b4 Finalize PID tuning
pick 5d6e7f8 Update unit tests for motor controller
```

Result: 3 clean, meaningful commits instead of 5.

**When to use:**
- Before opening a Pull Request — present a coherent, reviewable commit history.
- Combining fix-up commits, removing debug commits, rewriting messages.
- Never on commits already pushed to a shared branch (unless the team explicitly agrees, e.g., force-push to your own PR branch).

```bash
# Quick squash: merge last 3 commits into one
git reset --soft HEAD~3
git commit -m "feat(motor-ctrl): implement PID-based speed controller"
```

---

### Q9: Explain the differences between `git reset`, `git revert`, and `git checkout`.

**Answer:** These three commands serve fundamentally different purposes despite all being able to "undo" changes:

**`git reset`** — Moves the branch pointer (HEAD) backward, optionally modifying the staging area and working directory. It rewrites history.

```bash
# Soft: move HEAD back, keep changes staged
git reset --soft HEAD~1

# Mixed (default): move HEAD back, unstage changes, keep in working dir
git reset HEAD~1

# Hard: move HEAD back, discard all changes (dangerous)
git reset --hard HEAD~1

# Unstage a file (does not modify the file itself)
git reset HEAD -- src/file.cs
```

**`git revert`** — Creates a new commit that undoes the changes of a specified commit. History is preserved, making this safe for shared branches.

```bash
# Revert a single commit
git revert abc1234

# Revert without auto-committing
git revert --no-commit abc1234

# Revert a merge commit (specify which parent to keep)
git revert -m 1 <merge-commit-sha>
```

**`git checkout`** — Historically overloaded: switches branches OR restores files. Modern Git splits this into `git switch` and `git restore`.

```bash
# Switch branch (modern)
git switch feature-branch

# Restore a file to its last committed state (modern)
git restore src/file.cs

# Restore a file from a specific commit
git restore --source=abc1234 src/file.cs

# Legacy syntax (still works)
git checkout feature-branch
git checkout -- src/file.cs
```

**Summary decision matrix:**

| Scenario | Command |
|----------|---------|
| Undo last commit locally, keep changes | `git reset --soft HEAD~1` |
| Undo a commit on a shared branch | `git revert <sha>` |
| Discard uncommitted changes to a file | `git restore <file>` |
| Switch branches | `git switch <branch>` |
| Completely erase last N commits and changes | `git reset --hard HEAD~N` (dangerous) |

---

### Q10: What are `.gitignore` best practices?

**Answer:**

**Core principles:**
1. **Commit `.gitignore` early** — it should be one of the first files in any repository.
2. **Never track generated artifacts** — build outputs (`bin/`, `obj/`, `Debug/`, `Release/`), compiled binaries, package caches (`node_modules/`, `packages/`).
3. **Never track secrets** — `.env`, API keys, certificates, credentials files. If accidentally committed, rotate the secret immediately (Git history retains it even after removal).
4. **Never track user-specific configuration** — IDE settings (`.vs/`, `.idea/`, `.vscode/` unless team-shared), OS files (`.DS_Store`, `Thumbs.db`).

**Layered ignore strategy:**

```bash
# 1. Global gitignore (per-developer, for OS/IDE files)
git config --global core.excludesFile ~/.gitignore_global

# ~/.gitignore_global
.DS_Store
Thumbs.db
*.swp
.idea/
.vs/

# 2. Repository .gitignore (committed, shared)
# For a C#/.NET project:
bin/
obj/
*.user
*.suo
*.cache
packages/
TestResults/
*.nupkg

# 3. Directory-level .gitignore (for specific subdirectories)
# e.g., docs/.gitignore to ignore generated HTML

# 4. .git/info/exclude (local, not committed — per-developer overrides)
```

**Useful patterns:**

```bash
# Ignore everything in a directory except one file
logs/*
!logs/.gitkeep

# Ignore all .log files anywhere in the tree
**/*.log

# Negate a pattern (track a specific file that would be ignored)
*.dll
!lib/required.dll
```

**If a file is already tracked and you add it to `.gitignore`:**

```bash
# Remove from tracking but keep on disk
git rm --cached path/to/file
git commit -m "chore: stop tracking generated file"
```

Use GitHub's [gitignore templates](https://github.com/github/gitignore) as a starting point for your language/framework.

---

## SVN (Subversion)

### Q11: What are the key differences between SVN and Git?

**Answer:**

| Aspect | Git | SVN |
|--------|-----|-----|
| **Architecture** | Distributed — every clone is a full repository with complete history | Centralized — single authoritative server; clients hold working copies |
| **Branching** | Branches are lightweight pointers (41 bytes) | Branches are full directory copies on the server (`svn copy`) |
| **Offline work** | Full capability — commit, branch, diff, log, merge all work offline | Requires server connection for most operations (commit, log, blame) |
| **Atomic unit** | Commit hash (SHA-1 of content) — content-addressable | Revision number (sequential integer, global to the repo) |
| **Speed** | Most operations are local, extremely fast | Network-dependent for most operations |
| **History** | Every developer has the full history locally | History lives only on the server |
| **Storage** | Efficient packfiles; deduplication via content hashing | Stores deltas on the server; working copies can be large |
| **Merge tracking** | Built into the DAG; merge commits have multiple parents | Added in SVN 1.5+ via `svn:mergeinfo` property; historically error-prone |
| **Learning curve** | Steeper due to distributed model, staging area, rebase | Simpler mental model (one central truth) |
| **Partial checkout** | Not natively supported (sparse checkout is limited) | Supports checking out individual subdirectories easily |

**When SVN still makes sense:**
- Large binary assets that Git handles poorly (though Git LFS mitigates this).
- Very large monorepos where partial checkout is critical.
- Teams deeply invested in centralized workflows and tooling (e.g., some legacy embedded tool chains integrate with SVN).
- Strict access control at the directory level (SVN has native path-based authorization).

```bash
# SVN equivalents of common Git commands
svn checkout https://svn.example.com/repo/trunk   # git clone
svn update                                          # git pull
svn commit -m "message"                             # git commit + git push
svn log                                             # git log
svn diff                                            # git diff
svn status                                          # git status
```

---

### Q12: How does SVN handle branching and tagging? Why is it called "directory-based"?

**Answer:** In SVN, branches and tags are not first-class concepts — they are conventional directory copies within the repository. The standard repository layout is:

```
/trunk          # main development line (equivalent to Git's main)
/branches       # feature and release branches
  /feature-x
  /release-1.2
/tags           # release snapshots
  /v1.0.0
  /v1.1.0
```

**Creating a branch or tag uses `svn copy`:**

```bash
# Create a branch
svn copy https://svn.example.com/repo/trunk \
         https://svn.example.com/repo/branches/feature-x \
         -m "Create branch for feature X"

# Create a tag
svn copy https://svn.example.com/repo/trunk \
         https://svn.example.com/repo/tags/v1.2.0 \
         -m "Tag release v1.2.0"
```

**Why "directory-based":**
- `svn copy` performs a "cheap copy" — it does not duplicate data on the server. Internally, SVN stores a pointer to the source revision, similar to a hard link. Only subsequent changes consume additional storage.
- However, to the user, branches and tags look and behave like directories. There is nothing preventing someone from committing to a "tag" directory — enforcing tag immutability is a policy matter (often handled by pre-commit hooks on the server).
- Switching between branches means changing the working copy's URL:

```bash
svn switch https://svn.example.com/repo/branches/feature-x
```

**Merge workflow:**

```bash
# Merge changes from trunk into your branch
svn merge https://svn.example.com/repo/trunk

# Reintegrate branch back into trunk
svn switch https://svn.example.com/repo/trunk
svn merge --reintegrate https://svn.example.com/repo/branches/feature-x
svn commit -m "Merge feature-x into trunk"
```

The directory-based model is simpler to understand but lacks the DAG-based merge tracking that makes Git merges more reliable.

---

## ClearCase

### Q13: Provide a brief overview of IBM Rational ClearCase — UCM, VOBs, and views.

**Answer:** ClearCase is an enterprise-grade, centralized version control system by IBM (originally Rational Software). It is found in large organizations with long-established toolchains, particularly in automotive, aerospace, defense, and industrial automation.

**Key concepts:**

**VOB (Versioned Object Base):**
- The repository — stores all version-controlled files, directories, and their metadata.
- A VOB is mounted on the file system and appears as a directory tree.
- Multiple VOBs can compose a project (component-based architecture).

**Views:**
- A view is a developer's workspace that presents a specific configuration of the VOB's contents.
- **Snapshot view:** A local copy of selected files (similar to an SVN working copy). Works offline but must be explicitly updated.
- **Dynamic view:** A virtual, transparent filesystem overlay (using MVFS — Multi-Version File System) that resolves file versions in real time based on a *config spec*. No local copy — reads come from the network. Extremely powerful but requires network connectivity and a ClearCase server.
- Config specs define rules like "give me the latest on branch X" or "give me everything as of this label."

**UCM (Unified Change Management):**
- A higher-level process layer on top of base ClearCase.
- Organizes work into **activities** (logical change sets, similar to a commit), **streams** (similar to branches — integration stream, development streams), **baselines** (labeled snapshots of a stream, similar to tags), and **components** (modular VOB subsets).
- Enforces a workflow: developers work on a development stream, deliver changes to the integration stream, and the integrator creates new baselines after testing.

**ClearCase vs Git:**

| Aspect | ClearCase | Git |
|--------|-----------|-----|
| Model | Centralized (with dynamic views) | Distributed |
| Branching | Per-file branching (branch tree per element) | Per-repository branching |
| Cost | Expensive commercial license | Free and open source |
| Complexity | Very high — requires dedicated admin | Moderate — self-service |
| Strengths | Fine-grained access control, auditing, regulatory compliance | Speed, flexibility, ecosystem |

ClearCase is relevant in interviews for companies with legacy systems or those transitioning from ClearCase to Git, which many enterprises are doing. Understanding it shows breadth of experience across enterprise VCS tools.

---

## General Version Control

### Q14: What branching strategies work best for large enterprise teams?

**Answer:** The right strategy depends on team size, release cadence, and deployment model. Here are the main options ranked by complexity:

**1. Trunk-Based Development (with short-lived branches):**
- Everyone integrates to `main` at least daily. Feature branches live < 2 days.
- Requires: excellent CI/CD, feature flags, high test coverage.
- Used by: Google, Meta, Microsoft (for many products).
- Best for: teams practicing continuous deployment.

**2. GitHub Flow:**
- `main` + short-lived feature branches + Pull Requests.
- Best for: teams that deploy frequently but want a code review gate.

**3. Git Flow:**
- `main` + `develop` + `feature/*` + `release/*` + `hotfix/*`.
- Best for: scheduled releases, multiple supported versions, QA-heavy workflows.
- Common in: embedded systems, industrial automation, regulated environments.

**4. Release Branch Strategy (simplified Git Flow):**
- `main` for development. Cut `release/X.Y` branches when approaching a release. Hotfixes go to the release branch and are cherry-picked or merged back to `main`.
- Good compromise between Git Flow's rigor and trunk-based simplicity.

**Enterprise considerations:**
- **Long-lived release branches** are common when you must support multiple firmware/software versions in the field (e.g., Lenze drives running different software generations).
- **Branch protection rules:** Require PR reviews, passing CI, no force-pushes to `main`.
- **Environment branches** (e.g., `staging`, `production`) are sometimes used but can cause drift — prefer pipeline-based promotion instead.
- **Monorepo branching:** In monorepos, trunk-based development is strongly preferred because long-lived branches across many components lead to merge nightmares.

```bash
# Branch protection via GitHub CLI
gh api repos/owner/repo/branches/main/protection --method PUT \
  --field required_pull_request_reviews='{"required_approving_review_count":2}' \
  --field enforce_admins=true
```

---

### Q15: How should tagging and labelling be used for releases?

**Answer:** Tags (Git) and labels (ClearCase/SVN) provide immutable markers that identify release points in the history. They are essential for traceability, reproducibility, and rollback.

**Git tagging best practices:**

```bash
# Annotated tag (preferred for releases — stores tagger, date, message)
git tag -a v2.4.1 -m "Release 2.4.1: Fix motor overcurrent protection"

# Push a specific tag
git push origin v2.4.1

# Push all tags
git push origin --tags

# List tags matching a pattern
git tag -l "v2.4.*"

# Tag a past commit
git tag -a v2.4.0 abc1234 -m "Retroactively tag release 2.4.0"

# Delete a tag (local and remote)
git tag -d v2.4.1
git push origin --delete v2.4.1
```

**Semantic Versioning (SemVer):** Use `vMAJOR.MINOR.PATCH`:
- MAJOR: breaking changes (API incompatibility)
- MINOR: new features, backward-compatible
- PATCH: bug fixes, backward-compatible

For embedded/industrial projects, you might extend this: `v2.4.1-rc1` (release candidate), `v2.4.1+build.3847` (build metadata).

**Release workflow:**

```bash
# 1. Create release branch
git checkout -b release/2.4.1 develop

# 2. Stabilize (bug fixes only)
# ...

# 3. Merge into main and tag
git checkout main
git merge --no-ff release/2.4.1
git tag -a v2.4.1 -m "Release 2.4.1"

# 4. Merge back into develop
git checkout develop
git merge release/2.4.1

# 5. Create a GitHub release (with release notes)
gh release create v2.4.1 --title "v2.4.1" --notes "Fixed motor overcurrent protection"
```

**CI/CD integration:** Configure pipelines to trigger on tag pushes — build the release artifact, run full test suite, deploy to staging, and await approval for production.

---

### Q16: How do code review workflows integrate with version control?

**Answer:** Code review is the quality gate between writing code and merging it. Modern VCS platforms provide first-class support:

**Pull Request / Merge Request workflow:**

1. Developer creates a feature branch and pushes commits.
2. Opens a PR/MR targeting the integration branch (`main`, `develop`).
3. Automated checks run (CI pipeline, linters, static analysis, tests).
4. One or more reviewers examine the diff, leave comments, request changes.
5. Developer addresses feedback with additional commits (or amends).
6. Reviewer approves; PR is merged (merge commit, squash, or rebase — configurable).

**Best practices for reviewable PRs:**
- Keep PRs small (< 400 lines of diff). Large PRs get superficial reviews.
- Write a clear PR description: what changed, why, how to test.
- Use atomic, well-messaged commits — tell a story.
- Include screenshots or test evidence for UI or hardware-related changes.

**Branch protection enforcing reviews:**

```bash
# Require 2 approvals, dismiss stale approvals on new pushes
# GitHub branch protection (via UI or API)
gh api repos/owner/repo/branches/main/protection --method PUT --input - <<'EOF'
{
  "required_pull_request_reviews": {
    "required_approving_review_count": 2,
    "dismiss_stale_reviews": true
  },
  "required_status_checks": {
    "strict": true,
    "contexts": ["ci/build", "ci/test"]
  },
  "enforce_admins": true
}
EOF
```

**Code review in non-Git systems:**
- **SVN:** Tools like Crucible, ReviewBoard, or Phabricator provide review workflows on top of SVN commits.
- **ClearCase:** UCM's deliver/rebase model provides an integration step, but dedicated code review is typically handled by external tools (Collaborator, Gerrit-like processes).

**Code Owners:** Define ownership rules so the right reviewers are automatically assigned:

```bash
# .github/CODEOWNERS
*.cs          @backend-team
src/drivers/  @firmware-team @safety-lead
*.proto       @api-team
```

---

### Q17: How do you handle large binary files in version control?

**Answer:** Standard Git is designed for text files and performs poorly with large binaries because:
- Every clone downloads the full history of every binary — a 100 MB firmware image changed 50 times means ~5 GB.
- Binary diffs are opaque — Git stores full copies, not meaningful deltas.
- Repository size grows quickly, slowing clone, fetch, and checkout.

**Solutions:**

**1. Git LFS (Large File Storage) — the standard solution:**

```bash
# Install and initialize
git lfs install

# Track binary file types
git lfs track "*.bin"
git lfs track "*.hex"
git lfs track "*.elf"
git lfs track "*.png"

# This creates/updates .gitattributes (commit this file)
git add .gitattributes
git commit -m "chore: configure Git LFS for binary artifacts"

# From here, normal Git commands work transparently
git add firmware.bin
git commit -m "Add firmware v2.4.1 binary"
git push
```

LFS stores a lightweight pointer in the Git repo and uploads the actual file to a separate LFS server. Clones only download the binary versions they need.

**2. Git submodules or external artifact storage:**
- Store binaries in a dedicated artifact repository (Artifactory, Nexus, Azure Artifacts).
- Reference them by version in your build configuration.
- Best for compiled dependencies and third-party libraries.

**3. `.gitattributes` for binary handling:**

```bash
# Mark files as binary to prevent diff/merge issues
*.bin binary
*.hex binary
*.elf binary

# Use LFS
*.bin filter=lfs diff=lfs merge=lfs -text
```

**4. SVN's advantage:** SVN handles large binaries better than vanilla Git because it supports partial checkout — developers only download the directories they need, and only the latest revision is checked out by default.

**5. ClearCase's advantage:** Dynamic views never copy files locally until accessed, so large binaries do not impact clone/checkout performance.

**Best practice:** Keep generated binaries out of version control entirely. Use CI/CD to build and publish artifacts. Store only source code, build scripts, and configuration. If binaries must be versioned (e.g., pre-compiled vendor libraries, FPGA bitstreams), use Git LFS.

---

### Q18: Compare monorepo and polyrepo approaches. What are the trade-offs?

**Answer:**

**Monorepo:** A single repository contains all projects, libraries, and services for an organization or product line.

**Polyrepo:** Each project, library, or service lives in its own repository.

| Aspect | Monorepo | Polyrepo |
|--------|----------|----------|
| **Code sharing** | Trivial — import directly, refactor across boundaries atomically | Requires publishing packages, versioning, and consuming them |
| **Atomic changes** | One commit can update an API and all its consumers | Cross-repo changes require coordinated PRs and releases |
| **Dependency management** | Single version of each dependency (diamond dependency problem is eliminated) | Each repo manages its own dependencies (version drift) |
| **CI/CD** | Must be smart — only build/test what changed (need tools like Bazel, Nx, Turborepo) | Simple — each repo has its own pipeline |
| **Repository size** | Can become enormous; requires tooling for performance (sparse checkout, VFS) | Each repo stays small and fast |
| **Access control** | Harder — everyone can see everything (GitHub CODEOWNERS helps but does not restrict read access) | Natural isolation — grant access per repo |
| **Onboarding** | One clone, one build system, consistent tooling | Must discover and clone multiple repos |
| **Ownership** | Can become ambiguous; requires CODEOWNERS | Clear ownership per repo |

**When to choose monorepo:**
- Tightly coupled components that frequently change together (e.g., a motor controller firmware, its communication protocol library, and its configuration tool).
- Small-to-medium organization wanting maximum code reuse and atomic refactoring.
- Teams willing to invest in build tooling (Bazel, Nx, Rush).

**When to choose polyrepo:**
- Independent teams/products with different release cycles.
- Open-source libraries meant for external consumption.
- Strict access control requirements (e.g., different security clearance levels).
- When repository size would be unmanageable in a monorepo.

**Hybrid approach:** A common pragmatic choice — group tightly coupled components into a monorepo, keep independent products in separate repos. For example, all firmware for a product family in one repo, cloud services in another, and shared protocol libraries published as versioned packages.

```bash
# Monorepo tooling examples

# Bazel: build only affected targets
bazel test //... --test_tag_filters=-manual

# Git sparse checkout: check out only what you need in a monorepo
git sparse-checkout init --cone
git sparse-checkout set src/motor-controller src/common-lib

# CODEOWNERS in monorepo
# /src/motor-controller/  @firmware-team
# /src/cloud-api/          @backend-team
# /src/common-lib/         @platform-team
```

For an embedded/mechatronics context, a monorepo often works well because firmware, drivers, and hardware abstraction layers are tightly coupled and benefit from atomic cross-cutting changes.

---

*End of Version Control Systems Q&A*
