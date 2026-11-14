# Local correctness and security audit — 2026-11-15

Baseline: `d6053e71971b4471b38ddc4aba1e563c7035e1ea` in `sannlynnhtun-coding/b7-dotnet-training-sep-2026`.

Scope: controllers, request models, database mappings and configuration across the training applications, with independent reviews of GameShop, Supermarket POS, and the other API samples. Validation includes all 38 application/database/regression projects, isolated SQL databases, loopback HTTP/HTTPS, and mocked outbound HTTP. Expense Tracker (PR #7) is excluded from edits and source review as instructed. During this audit, another actor merged PR #7 into master as `d1da79d`; that upstream merge was preserved, not performed or reverted by this task.

## Confirmed findings and fixes

| Area | Finding | Fix and regression coverage |
|---|---|---|
| GameShop stock | Partial patches could set available stock above effective total or reserved copies. | Validate effective stock against active sessions; rejected updates leave SQL data unchanged. |
| GameShop deletion | Unloaded navigation collections missed children; hard deletes ignored soft-deleted FK references. | Query children in SQL and include every reference for hard deletion. |
| GameShop consoles | Count-based numbering collided after deletion or concurrent creates; duplicate renames hit unique constraints. | Serialize mutations with the existing scheduling lock, allocate an unused bounded label, and reject duplicate renames. |
| GameShop billing | Deposits could be negative or exceed the effective edited bill; long intervals and large totals overflowed SQL precision. | Validate effective amounts, rounded duration/money bounds, fractional-cent boundaries, and minimum-date arithmetic. |
| Supermarket POS | Delete endpoints physically removed records despite soft-delete fields, losing data or failing on sales-history FKs. | Soft-delete categories, customers, employees and products; verify historical sales/items remain readable. |
| Supermarket POS | Phone values were discarded; missing/deleted parents and duplicate unique values caused server errors. | Persist phone, validate active parents, translate SQL duplicate-key failures to generic 409 responses, and test concurrent duplicates. |
| Request boundaries | Several DTOs accepted blank or oversized required text and unsupported numeric values. | Add schema-aligned validation for POS, GameShop, Personality, ReaderClub, Movie and Zodiac/Horoscope inputs. |
| MagicNumberTrick | A malformed hardcoded connection string overrode injected options. | Respect injected options, support external configuration and a valid integrated-authentication local default; verify seeded SQL queries. |
| PersonalityTest | Option lookup used a question ID; parent references and historical option edits/deletes were unsafe. | Look up by option primary key, validate parents, reject referenced deletions and changes to answered options' question/scoring identity; test edit/answer races. |
| Battleship | Concurrent turns could duplicate shots and lose counters; reset/placement/read operations could race. | Serialize each game's operations and make new-game/fleet creation atomic; test identical/different shots, placement, restart, give-up and rollback. |
| ReaderClub | Reviewer deletion could remove shared book-review content or fail on another reviewer's reference. | Preserve shared and anonymous references when deleting a reviewer. |
| Clinic | Token requests with missing parents and deletion of referenced patients caused raw FK failures. | Return controlled 404/409 responses while retaining SQL FK protection. |
| JSON importer | A failed later insert left the earlier rows committed. | Bind the entire batch to one transaction; injected failure after a valid insert leaves no partial batch. |
| Recipe/pantry matching | Blank legacy names matched every recipe; whitespace patches erased values; case matching depended on culture. | Ignore blank names, preserve data on whitespace-only patches, use ordinal case-insensitive matching, and return no matches for an empty/blank-only pantry without upstream calls. Include a Turkish-culture regression. |

## Standards review

No repository `rules.md` or documented coding standards were found. Fixes reuse the existing EF, ADO.NET, MVC and transaction patterns. No style-only redesign was performed. Runtime packages were not added for the fixes; regression projects reference the existing application projects. The POS scaffolding dependency is private to keep design-time packages from leaking to consumers.

Independent reviews checked the final GameShop and Battleship transaction/validation changes and the importer/pantry changes. A fractional-money boundary identified during cross-review was added to the fixes and regression suite.

## Specification review

No separate formal product specification was supplied. Findings were evaluated against the database schema, existing API behavior, explicit validation messages, and the user's local-training constraints. Tests preserve those behaviors while preventing confirmed failures. No sales-total formula, tax policy, rating scale, authentication system, or new calendar interpretation was invented.

## Validation

- Scoped 38-project rebuild with `--no-incremental -warnaserror`: zero warnings/errors. Full integration inventory is now 41 projects after the external PR #7 merge; it builds with zero errors and one pre-existing scaffold warning in excluded Expense Tracker.
- Vulnerable-package audit including transitive dependencies across the full integration inventory.
- Tracked-file credential pattern scan and GitGuardian PR checks.
- `tests/SecurityRegression`: SQL injection, atomic importer failure, pantry edge cases and culture, Personality answer/submission races, real loopback HTTP/HTTPS with mocked Spoonacular responses, CRUD validation and retained data.
- `tests/GameShop.Regression`: real SQL stock/deposit/precision/deletion/numbering/race cases and DTO validation.
- `tests/Supermarket.Regression`: real loopback HTTP and SQL history, parent, phone, uniqueness and request-boundary cases.
- `tests/OtherApis.Regression`: Magic configuration/seeding, Personality lookup/parent/history races, ReaderClub shared references, Clinic FK responses, Movie validation, and Battleship concurrency/atomicity.
- Existing Clinic concurrency, ReaderClub SQL, credential configuration, nullable student handling, Battleship configuration, and Zodiac configuration/schema regressions.

SQL regression fixtures create uniquely named temporary databases and delete them afterward. Local listeners use loopback and are stopped after testing. Existing application databases and their data are not modified.

## Limits and remaining decisions

- Real upstream API calls and deployed HTTPS remain skipped as explicitly requested. External responses are mocked.
- The scaffold warning in externally merged Expense Tracker remains untouched because PR #7 was excluded from this task.
- These APIs remain unauthenticated local training samples and must not be publicly exposed.
- Existing deployed stock discrepancies and malformed legacy application data were not migrated or repaired.
- Supermarket sales still accept independently supplied totals; calculation, tax and rounding policy require a product specification. Zodiac calendar-range semantics and Movie rating scales likewise retain their existing behavior.
- This is a deeper targeted audit with concurrency and failure-path tests, not proof that every possible defect is absent. Production load, distributed failure/recovery, and deployment configuration are outside this run.
