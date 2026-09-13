# Infernal Expansion Tactic Evaluation

Immutable, no-engine evaluation of factual run observations against the exact
cached MGC33 Infernal first-expansion tactic recipe. The evaluator authenticates
the requested layout/socket and recipe reference, preserves MGC33 validation
issues for malformed evidence, and returns `Invalid`, `Inactive`, `Waiting`,
`AlreadyIssued`, or `Eligible`.

Only an eligible result exposes the same cached recipe plus a read-only snapshot
of its ordered actor response references. Core remains authoritative for clocks,
revision storage, events, entities, world-distance measurement, AI, movement,
abilities, traps, combat, possession, UI, lifecycle, and tactic issuance. This
package never waits, calculates distance, mutates state, or invokes callbacks.
