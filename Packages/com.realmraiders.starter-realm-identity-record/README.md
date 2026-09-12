# Starter Realm Identity Record

Portable no-engine facts for one already-chosen starter realm layout. The
canonical UTF-8 format has exactly four LF-separated fields and no final newline:

```text
version=1
realmId=realmraiders.realm.sylvan
seed=-17
layoutId=realmraiders.sylvan-layout.ancient-crossroads
```

Fields, order, decimal number spelling and lowercase ASCII namespaced IDs are
strict. Values are raw tokens; quotes, escapes, whitespace, BOMs and trailing
data are not canonical. Parsing is fail-closed and returns immutable ordered
evidence.

This package does not choose a layout, generate a seed, read or write storage,
migrate records, or own Core, account, multiplayer, Unity or gameplay state.
