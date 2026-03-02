# FlowBlox Agent Conventions

## Purpose
This file summarizes core project conventions for AI/coding agents working in the FlowBlox repository.

## High-Level Architecture
- `FlowBlox` is the WinForms host application.
- `FlowBlox.UICore` contains WPF-based UI infrastructure (property rendering, editors, factories, resolvers).
- `FlowBlox.Core` contains execution/runtime logic and the core domain model.
- FlowBlock execution logic belongs to `FlowBlox.Core` (runtime, testing, generation, execution pipeline).

## FlowBlock UI Generation Model
- FlowBlock property UIs are generated via attributes (annotation-driven UI).
- Main pipeline:
  - Schema/property resolution in PropertyView
  - Control resolution via resolver classes
  - Concrete controls built via factories
- Key classes in `FlowBlox.UICore`:
  - `Resolver/PropertyControlResolver`
  - `Resolver/TextBoxWithOptionalButtonsCreator`
  - `Factory/PropertyView/DataGridFactory`
  - `Factory/Base/PropertyFactoryBase` (`PropertyViewControlFactoryBase`)

## Required Annotation Namespaces
- FlowBlox UI annotations:
  - `FlowBlox.Core.Attributes`
  - Commonly used:
    - `FlowBlockUIAttribute`
    - `FlowBlockTextBoxAttribute`
    - `FlowBlockDataGridAttribute`
    - `FlowBlockListViewAttribute`
    - `FlowBlockUIGroupAttribute`
    - `FlowBloxSupportedTypesAttribute`
    - `FlowBlockUIFileSelectionAttribute`
- Display/localization annotation:
  - `System.ComponentModel.DataAnnotations`
  - Use `DisplayAttribute` with `ResourceType = typeof(FlowBloxTexts)`

## Localization Conventions (Mandatory)
- Always create localization keys for new user-visible types/properties/tooltips.
- Key naming format:
  - Type display name: `ClassName_DisplayName`
  - Property label: `ClassName_PropertyName`
  - Property tooltip: `ClassName_PropertyName_Tooltip`
- Add keys in both:
  - `FlowBlox.Core/FlowBloxTexts.resx`
  - `FlowBlox.Core/FlowBloxTexts.de.resx`
- Encoding requirement:
  - `FlowBlox.Core/FlowBloxTexts.de.resx` must be saved as `UTF-8` **without BOM**.
- Rule: provide tooltip text for explanatory/complex properties (recommended), especially for selectors/patterns (e.g., XPath, CSS selector, regex). Include short examples to improve UX where helpful.

## Property Declaration Conventions for FlowBlocks/Strategies
- Use `[Display(...)]` on user-facing properties.
- For editable string content with optional code editor/suggestions use:
  - `[FlowBlockTextBox(...)]`
- For association/linkable objects use:
  - `[FlowBlockUI(Factory = UIFactory.Association, ...)]`
- For list/grid rendering use:
  - `[FlowBlockUI(Factory = UIFactory.ListView|GridView, ...)]`
  - `[FlowBlockDataGrid(...)]` or `[FlowBlockListView(...)]` where needed.

## Runtime/Execution Conventions
- Keep execution concerns in `FlowBlox.Core`.
- Generation strategies and FlowBlock test/runtime integration must use existing runtime pipeline, not parallel ad-hoc execution paths.
- Prefer passing/using existing runtime context from executors instead of creating duplicate runtime instances.

## Practical Agent Rules
- Reuse existing infrastructure before introducing new patterns.
- Keep changes consistent with annotation-driven UI architecture.
- Do not introduce user-visible strings without localization keys.
- For new UI-facing properties: label keys are required. Tooltip keys are strongly recommended for explanatory/complex properties (e.g., XPath/CSS selectors) and should include concise examples when useful.
