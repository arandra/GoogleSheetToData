# Sheet Authoring Guide

GoogleSheetToData interprets each Google Sheet tab as **Table**, **Const**, or **Enum** mode. Follow the structure below so the generator behaves consistently.

## Shared Rules
- Row 1 defines field metadata for the selected mode.
- Row 2 defines field names or enum names depending on the mode.
- The sheet name becomes the generated class name for Table and Const sheets.
- Enum sheets generate one `.cs` file per enum name defined in the sheet.
- The sheet ID comes from the spreadsheet URL segment `/d/<ID>/`. Sample data references `1_2Y3BtltwsyXTovWuWV6J32x_Ebe2Sy8vybGNhzkIsM`.

### Supported Field Types
- `string`
- `int`
- `float`
- `double`
- `bool`
- `enum(Name)` where `Name` is defined by an Enum sheet, for example `enum(MonsterType)`
- `Pair<TKey, TValue>` such as `Pair<string, int>` or `Pair<int, float>`
- Append `[]` to make an array of any supported type, such as `int[]`, `Pair<string, float>[]`, or `enum(MonsterType)[]`

Type keywords are matched case-insensitively, but aliases such as `integer`, `number`, or `boolean` are not recognized. Use the exact names above.

### Split List Columns
Use split list columns when a list value is too large to fit comfortably in one cell.

- Append `#<index>` to the field name to spread one logical list across multiple columns, for example `Param#0`, `Param#1`, `Param#2`.
- The shared base name becomes the generated property name, so keep the plural naming you would use for a normal list column.
- Indexes must start at `0` and remain contiguous. Missing or out-of-order indexes are treated as errors and stop generation.
- Every split column in the same group must use the same field type. Mixed types are treated as errors and stop generation.
- A split column typed as `int` generates `List<int>`. A split column typed as `int[]` generates `List<List<int>>`.
- Blank cells are allowed only as a trailing run. Once a blank cell appears in a split list group, every later cell in that same group must also be blank.
- If a blank cell is followed by a non-blank cell, generation stops with an error.
- These rules apply to both **Table** and **Const** sheets.

## Table Mode
| Row | Description |
| --- | --- |
| 1 | Field types |
| 2 | Field names |
| 3+ | Data rows |

Example:

| string | int | float |
| --- | --- | --- |
| Name | Level | Rate |
| Knight | 10 | 0.12 |
| Archer | 7 | 0.08 |

- Output: base class `FieldTransform` plus ScriptableObject `FieldTransforms` containing `List<FieldTransform> Values`.
- Use `SerializableTypes.Pair<TKey, TValue>` for pair columns. Enter values as `(key, value)`.
- Array values use comma-separated entries.
- Split list columns are merged per row before serialization, so `Reward#0`, `Reward#1`, `Reward#2` becomes one `Rewards` list on the generated row type.
- Enum fields are written as `enum(Name)` in row 1 and use enum member names in data rows.

Enum field example:

| enum(MonsterType) | int |
| --- | --- |
| Target | Count |
| Goblin | 3 |
| Slime | 5 |

## Const Mode
- Each row follows `Type / Name / Value`.
- Values serialize verbatim.
- Pair values use parentheses, for example `(Sword,1)`.
- Array values use comma-separated entries.
- Output: ScriptableObject with a single `Value` field instead of a list.
- Split list columns follow the same `Name#0`, `Name#1`, ... rule as table sheets and are merged into one list value before the ScriptableObject is written.

Example:

| string | GameTitle | GSheet Heroes |
| --- | --- | --- |
| int | DefaultLives | 3 |
| Pair<string,int>[] | StarterItem | `(Sword,1),(Potion,5)` |

Const enum example:

| enum(MonsterType) | DefaultTarget | Goblin |
| --- | --- | --- |

Notes:
- Enum fields in Const mode also use `enum(Name)` in the type column.
- Parsing does not validate whether the enum type exists or whether a member name is valid.

## Enum Mode
- Use Enum mode for sheets that define reusable C# enums.
- Enum sheets do not generate JSON output.
- A single sheet can contain multiple enum definitions.

### Layout
| Row | Description |
| --- | --- |
| 1 | Column role: `enum` or `value` |
| 2 | Enum name |
| 3+ | Enum members and optional numeric values |

Rules:
- Columns whose row-1 header is not `enum` or `value` are ignored.
- A `value` column is paired with the `enum` column whose row-2 enum name matches exactly.
- If an enum has no matching `value` column, members are emitted without explicit assignments.
- If a member name cell is empty, that row is skipped for that enum.
- If a member value cell is empty, the member is emitted without an explicit assignment.

Single enum example:

| enum | value |
| --- | --- |
| UnitType | UnitType |
| Monster | 0 |
| Player | 1 |

Generated code:

```csharp
public enum UnitType
{
    Monster = 0,
    Player = 1,
}
```

Enum without values:

| enum |
| --- |
| MonsterType |
| Goblin |
| Slime |

Generated code:

```csharp
public enum MonsterType
{
    Goblin,
    Slime,
}
```

Multiple enums in one sheet:

| enum | value | enum | value |
| --- | --- | --- | --- |
| UnitType | UnitType | MonsterType | MonsterType |
| Monster | 0 | Goblin | 100 |
| Player | 1 | Slime | 200 |

This layout generates `UnitType.cs` and `MonsterType.cs`.

## Output Paths & Namespaces
- Script and asset output paths must live under `Assets/`.
- Leaving the namespace empty generates global-scope classes and enums.

## Samples
- **FieldTransform**: Table workflow example. View the live sheet at [Google Sheets link](https://docs.google.com/spreadsheets/d/1_2Y3BtltwsyXTovWuWV6J32x_Ebe2Sy8vybGNhzkIsM) (`FieldTransform` tab).
- **InitConst**: Const workflow example. Use the same sheet link (`InitConst` tab).

Open the shared Google Sheet, follow the tab-specific layout, finish OAuth setup, and run the generator in your target environment.
