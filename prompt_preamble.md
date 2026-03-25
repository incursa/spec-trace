Target repository:
This current directory: C:\src\incursa\...

Spec Trace source of truth:
- Canonical standard: [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/)
- AI bootstrap: [`LLMS.txt`](./LLMS.txt)
- Supporting guidance: [`AGENTS.md`](./AGENTS.md)
- Supporting skills: [`skills/`](./skills/)

Task:
Use the local Spec Trace standard to help me turn imperfect, incomplete, human-supplied requirement input into proper repository-native specifications and requirements.

Context:
I am going to provide rough requirement input. It may be incomplete, ambiguous, inconsistent, poorly grouped, non-atomic, or missing important edge cases. Your job is to convert that raw input into clean Spec Trace artifacts and to actively identify what is still unclear.

This is not just a transcription task.
You should behave like a requirements analyst and editor working within the Spec Trace standard.

Primary goals:
1. Interpret rough input into proper Spec Trace specifications and requirements.
2. Keep requirements atomic, testable, and traceable according to the local Spec Trace standard.
3. Preserve the intended meaning of my input without inventing unjustified behavior.
4. Identify ambiguity, incompleteness, conflicts, hidden assumptions, and missing edge cases.
5. Ask follow-up questions when needed.
6. Also maintain a written gap/issue record in the repository so unresolved items are tracked even if I do not answer everything immediately.

Working rules:
- Treat [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/) as the canonical source of truth.
- Use [`LLMS.txt`](./LLMS.txt), [`AGENTS.md`](./AGENTS.md), and [`skills/`](./skills/) only as convenience guidance. If they differ from the canonical standard, the canonical standard wins.
- Do not invent a parallel requirements method.
- Do not leave raw rough notes as the final form if they can be normalized into proper Spec Trace artifacts.
- Do not silently fill in major product decisions unless they are strongly implied by context.
- Prefer explicit gaps over confident guessing.
- Keep diffs narrow and practical.
- Preserve existing valid requirement IDs where possible, but create new ones where required by the standard or by changed requirement boundaries.
- When you generate Markdown for this repository, prefer relative links for repo-local files, folders, and other concrete local artifacts. Keep backticks inside the link text when monospace styling should remain, and use absolute URLs only for external targets that cannot be expressed relatively.

What to do each time:
1. Read the current target repository and identify any existing Spec Trace artifacts or related requirement/design/work-item/verification content.
2. Read the local Spec Trace standard from the canonical path.
3. Interpret my rough input and classify it into:
   - clear requirement material that can be converted now
   - likely requirement material that needs confirmation
   - design/architecture considerations
   - work-item / implementation planning material
   - verification considerations
   - open questions / missing requirements / conflicts / assumptions
4. Create or update the appropriate Spec Trace artifacts in the target repo.
5. Create or update a repository file that tracks unresolved requirement gaps and follow-up questions.

Gap tracking requirements:
- Maintain a dedicated markdown file for unresolved requirement gaps.
- If the repo already has an appropriate canonical place for this, use it.
- Otherwise create a clearly named file such as:
  specs/requirements/[domain]/REQUIREMENT-GAPS.md
  or another location that fits the repo’s existing Spec Trace structure.
- Track items such as:
  - ambiguity
  - conflicting statements
  - missing edge cases
  - missing validation rules
  - missing failure behavior
  - missing external constraints
  - unclear terminology
  - unresolved scope boundaries
  - assumptions that need confirmation
- When possible, link gap items back to affected SPEC or REQ IDs.

Question-asking behavior:
- Ask follow-up questions when the missing information blocks good requirements.
- If the work can still proceed partially, do both:
  - draft what is clear
  - record the unclear parts in the gap file
- Do not stop just because some information is missing.

Authoring expectations:
- Group related requirements into appropriate specification files.
- Make requirement clauses atomic and normative according to the current Spec Trace standard.
- Keep Notes informative only.
- Use canonical trace fields.
- Keep requirement language precise and compact.
- Split combined thoughts into separate requirements where appropriate.
- If a statement is really design guidance rather than a requirement, put it in the correct artifact or gap file instead of forcing it into a requirement clause.
- If rough input contains tests, examples, or implementation ideas, use them appropriately without confusing them with normative requirements.

When updating an existing repo:
- Reuse and refine existing specification files where appropriate.
- Migrate or rewrite weak requirements into proper form.
- Do not preserve invalid requirement wording merely because it already exists.
- If a requirement boundary changes materially, handle IDs according to the current Spec Trace evolution rules.
- Preserve project-specific meaning while bringing the repo closer to the current standard.

Expected output after each pass:
1. Apply the artifact updates directly in the target repository.
2. Then provide a concise report with:
   - files changed
   - specifications created or updated
   - requirements added or revised
   - gaps/questions recorded
   - assumptions made
   - follow-up questions for me

Success criteria:
- Rough input is turned into real Spec Trace artifacts wherever possible.
- Unknowns are not lost; they are tracked explicitly in the repository.
- The repo becomes progressively more precise with each iteration.
- The result is useful for both humans and future tooling.

I will provide:
- the target repo path
- rough requirement input, either as dictated notes, bullets, or partial requirement text

Start by:
1. inspecting the target repo,
2. inspecting the local Spec Trace standard,
3. proposing where the requirement gaps file should live,
4. then converting my input into the first clean pass of artifacts.

---
