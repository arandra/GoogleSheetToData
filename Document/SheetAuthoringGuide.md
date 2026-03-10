# Sheet Authoring Guide

GoogleSheetToData interprets each Google Sheet tab as either **Table** or **Const** mode. Follow the structure below so the Unity generator and provided samples behave consistently.

## Shared Rules
- Row 1 defines **field types** using the supported identifiers listed below.
- Row 2 defines **field names**. Use PascalCase and avoid spaces/special characters.
- The sheet name becomes the generated class name (`FieldTransform`, `InitConst`, etc.).
- The sheet ID comes from the spreadsheet URL segment `/d/<ID>/`. Sample data references `1_2Y3BtltwsyXTovWuWV6J32x_Ebe2Sy8vybGNhzkIsM`.

### Supported Field Types
- `string`
- `int`
- `float`
- `double`
- `bool`
- `List<T>` where `T` is one of the scalar types above or a `Pair<TKey, TValue>`
- `Pair<TKey, TValue>` (e.g., `Pair<string, int>`, `Pair<int, float>`)
- Append `[]` to make an array of any supported type (`int[]`, `Pair<string, float>[]`, etc.)

Type keywords are matched case-insensitively, but aliases such as `integer`, `number`, or `boolean` are not recognized—use the exact names above.

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
| 3+ | Data rows (each row becomes an entry in the generated list) |

Example:
| string | int | float |
| --- | --- | --- |
| Name | Level | Rate |
| Knight | 10 | 0.12 |
| Archer | 7 | 0.08 |

- Output: base class `FieldTransform` plus ScriptableObject `FieldTransforms` containing `List<FieldTransform> Values`.
- Use `SerializableTypes.Pair<TKey, TValue>` for pair columns. Enter values as `(key, value)`; arrays of pairs use comma-separated tuples.
- Split list columns are merged per row before serialization, so `Reward#0`, `Reward#1`, `Reward#2` becomes one `Rewards` list on the generated row type.

## Const Mode
- Each row follows `Type / Name / Value`.
- Values serialize verbatim; wrap pair entries in parentheses and separate list items with commas.
- Output: ScriptableObject with a single `Value` field instead of a list.
- Split list columns follow the same `Name#0`, `Name#1`, ... rule as table sheets and are merged into one list value before the ScriptableObject is written.

Example:
| string | GameTitle | GSheet Heroes |
| --- | --- | --- |
| int | DefaultLives | 3 |
| Pair<string,int>[] | StarterItem | `(Sword,1),(Potion,5)` |

## Output Paths & Namespaces
- Script/asset output paths must live under `Assets/`. Relative inputs are recommended.
- Leaving the namespace empty generates global-scope classes.

## Samples
- **FieldTransform**: Table workflow example. View the live sheet at [Google Sheets link](https://docs.google.com/spreadsheets/d/1_2Y3BtltwsyXTovWuWV6J32x_Ebe2Sy8vybGNhzkIsM) (`FieldTransform` tab).
- **InitConst**: Const workflow example. Use the same sheet link (`InitConst` tab).

Open the shared Google Sheet, follow the tab-specific layout, finish OAuth setup, and run the generator in your target environment.
