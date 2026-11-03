# Repository continuity instructions

## Start a session

- Read root PROJECT_CONTEXT.md when starting a development session, resuming after compaction, or when project history matters. Check current Git status/diff before assuming its snapshot is current.
- Use the context file to locate relevant source and detailed evidence; read only additional documentation needed for the task. Treat current source and verified results as stronger evidence than stale roadmap prose.
- Active engine work is Lea. Keep the mature Lea.Engine / Lea.Win path distinct from earlier prototypes and unrelated games, FSIN and Bubble Wallpaper projects.

## Preserve existing work

- Preserve existing source code, functionality, uncommitted work, unrelated changes and retained failure evidence. Do not overwrite or revert another task's work. Make scoped changes authorized by the current user request; ask before destructive actions or changes outside that scope.
- Preserve important architecture/design decisions and unresolved problems. Distinguish implemented, partially verified, planned and blocked work; do not report plans or compilation alone as completed features.
- Keep validation tied to the tested revision/candidate. Do not transfer older passing results to changed binaries or weaken isolation/deadlines to clear a gate.
- When updating any existing AGENTS.md, retain its instructions and make the smallest compatible addition; do not replace unrelated guidance. User instructions take precedence.

## Finish a significant task

- Update PROJECT_CONTEXT.md as part of normal work after significant features, fixes, architectural changes or milestones, and when pausing unfinished substantial work.
- Record update date/revision, what changed and why, what is completed, what remains unfinished, validation outcomes/limits, important decisions and the recommended next action. Refresh the current-task section before handing off.
- Keep the summary concise and current: replace superseded statements, remove duplication, and link detailed feature docs/logs instead of copying them. Preserve unresolved issues until evidence resolves them.
- Keep LeaForge/PROGRESS.md and relevant feature documentation consistent when an engine milestone changes; claim completion only after appropriate verification.

## Long conversations and fresh starts

- Before a lengthy conversation risks losing important context, save a checkpoint in PROJECT_CONTEXT.md and recommend context compaction if the interface supports it, or a fresh conversation in the same repository. Do not claim an exact remaining context limit or unsupported UI command.
- A handoff must identify the current objective, relevant files/work in progress, verification boundary and next step so another conversation can resume from the files alone.
- Maintain these files during ordinary development tasks only. Do not create background processes, automatic monitoring or scheduled memory updates.
