# Realm Growth Contracts

This no-engine package validates caller-supplied immutable realm-growth tiers and
resolves an exact level to the greatest tier whose caller-defined minimum level is
satisfied. It requires stable ordinal IDs, strictly increasing nonnegative minimum
levels, nondecreasing positive footprint and node budgets, and nonnegative explicit
expansion-anchor capacity.

The package supplies no tiers, level thresholds, coordinates, currency, named
content, persistence, networking, gameplay behavior, or Unity interaction. Core or
another authoritative caller owns all of those decisions and supplies the catalogue
for each validation, exact tier-ID lookup, or level resolution.
