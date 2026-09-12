# Seeded Layout Selection

Faction-neutral, platform-stable selection of one exact layout ID from a
caller-authored canonical ordered catalogue. The caller supplies both the
canonical realm ID and signed 32-bit seed. Selection preserves catalogue order.

The explicit avoidance API maps directly over the catalogue with the previous
entry removed; it never loops or rerolls. A one-entry catalogue returns that sole
entry as a documented fallback.

This package never generates a seed and owns no account, player, persistence,
sync, RNG service, balance, Unity asset or multiplayer authority.
