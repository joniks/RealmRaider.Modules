# Realm Expansion Planning

Faction-neutral, no-engine expansion planning over one caller-supplied validated
`RealmLayoutGraph` and one caller-resolved valid `RealmGrowthTier`.

The evaluator returns the first authored expansion sockets in their original
order, up to the tier's exact expansion-anchor capacity. It owns no level or tier
selection, thresholds, persistence, randomization, geometry, materialization,
content placement, economy, networking, account, or runtime gameplay behavior.

The Editor compatibility assembly is conditionally enabled only when the
Infernal layout package is present; it does not add a faction dependency to the
runtime package.
