# Realm Layout Contracts

Faction-neutral immutable graph facts for a caller-authored realm layout. Nodes use
exact stable gameplay-role IDs and landmarks use exact stable presentation-role IDs;
neither vocabulary is owned or inferred by this package.

`RealmLayoutGraphValidator.Validate` checks structural IDs, undirected edges,
connectivity, a safe Entry-to-Objective route, finite bounded positions, floor widths,
node/corridor clearance, landmarks, and expansion sockets. It does not select a
layout, create a scene, materialize gameplay, infer content, or own saves, rewards,
RNG, networking, or UI.
