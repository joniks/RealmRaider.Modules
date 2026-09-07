# Realm Raider Modules

This repository is the isolated module catalogue for Realm Raiders. It is intentionally not a second Unity game project.

## Ownership

- **Module developer** creates or changes only self-contained modules here.
- **Reviewer / QA** reviews the frozen module diff and owns any requested verification.
- **Architect / Project Manager** commits accepted module work.
- **Project owner** pushes commits and authorizes integration into the main game.

## Module shape

Each Unity-ready module belongs below `Packages/` and uses a stable namespace:

```text
Packages/
  com.realmraiders.<feature>/
    package.json
    Runtime/
    Editor/
    Tests/
    Documentation~/
```

Modules must not contain `ProjectSettings`, scenes, `Library`, generated build output, or edits to the main game's shared gameplay/UI files. A module is linked into the main Unity project only by an explicit, reviewed integration task.

## Integration rule

The main RealmRaider repository pins an exact module-repository commit through a Git submodule. Core developer does not update that pointer during an active feature. Architect updates it only after QA acceptance and an explicit integration decision.
