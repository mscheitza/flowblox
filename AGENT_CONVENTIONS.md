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
    - `FlowBloxUIAttribute`
    - `FlowBloxTextBoxAttribute`
    - `FlowBloxDataGridAttribute`
    - `FlowBloxListViewAttribute`
    - `FlowBloxUIGroupAttribute`
    - `FlowBloxSupportedTypesAttribute`
    - `FlowBloxUIFileSelectionAttribute`
- Display/localization annotation:
  - `System.ComponentModel.DataAnnotations`
  - Use `DisplayAttribute` with `ResourceType = typeof(FlowBloxTexts)`

## Localization Conventions (Mandatory)
- Always create localization keys for new user-visible types/properties/tooltips.
- German localization files must contain real German umlauts (`ä`, `ö`, `ü`, `Ä`, `Ö`, `Ü`, `ß`). Do not use XML character codes for normal text and avoid mojibake such as `Ã¤`.
- Key naming format:
  - Type display name: `ClassName_DisplayName`
  - Property label: `ClassName_PropertyName`
  - Property tooltip: `ClassName_PropertyName_Tooltip`
- Add keys in both:
  - `FlowBlox.Core/FlowBloxTexts.resx`
  - `FlowBlox.Core/FlowBloxTexts.de.resx`
- After changing `.resx` keys, regenerate the strongly typed designer for the neutral/main `.resx` file so `DisplayAttribute(ResourceType = ...)` can find the generated static string properties. Passing a localized file is okay; the script maps it to the neutral file:
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Update-StronglyTypedResourceDesigner.ps1 -ResxPath FlowBlox.Core/FlowBloxTexts.resx`
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Update-StronglyTypedResourceDesigner.ps1 -ResxPath FlowBlox.Core/FlowBloxTexts.de.resx`
- The designer regeneration script uses `StronglyTypedResourceBuilder` with an explicit `.resx` base path so image/SVG `ResXFileRef` entries resolve correctly. It processes only existing strongly typed resource designers and skips WinForms/WPF control designers.
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

## FlowBlock Card Summaries (`GetDisplayableProperties`)
- Every concrete FlowBlock with meaningful user configuration should override `GetDisplayableProperties()`.
- Start with `base.GetDisplayableProperties()` and append only the properties that best identify the configured operation at a glance.
- Prefer primary configuration such as service URLs and paths, source/target providers, associated managed-state FlowBlocks, selectors, patterns, and expressions.
- Omit secondary formatting and tuning details such as separators, indentation, retry values, and similar switches. Also omit credentials, secrets, large payload/content properties, and verbose mapping/result collections unless they are the defining configuration of the FlowBlock.
- Keep the properties in the same logical order in which the user configures the FlowBlock.
- FlowBlocks without meaningful user-facing configuration may rely on the base implementation.

## Unit-Test Scope
- Do not add trivial baseline tests for changes below roughly two story points of complexity.
- A test must protect meaningful FlowBlox-owned behavior such as branching, validation and error handling, state transitions, migrations, serialization compatibility, runtime interaction, or non-trivial orchestration.
- Do not create tests that merely prove framework/BCL or third-party behavior, straightforward property-to-option mapping, or known cryptographic library output. For example, hashing `"abc"` and comparing it with the standard SHA-256 digest does not meaningfully test FlowBlox.
- Thin wrappers and direct mappings normally require a successful build, not a dedicated unit test.
- Add tests when project-specific logic or a concrete regression risk makes them valuable, even if the implementation itself is small.

## Runtime/Execution Conventions
- Keep execution concerns in `FlowBlox.Core`.
- Generation strategies and FlowBlock test/runtime integration must use existing runtime pipeline, not parallel ad-hoc execution paths.
- Prefer passing/using existing runtime context from executors instead of creating duplicate runtime instances.

## FlowBlock Notifications
- For concise, flow-block-local error/warning messages shown directly in the FlowBlock UI, use the Notifications model.
- Define a block-specific enum with `FlowBloxNotificationAttribute` on each entry.
- Register it via `NotificationTypes` override in the FlowBlock.
- Trigger it with `CreateNotification(runtime, ...)`.
- Keep notification display texts short and action-oriented.

## Practical Agent Rules
- Reuse existing infrastructure before introducing new patterns.
- Keep changes consistent with annotation-driven UI architecture.
- Do not introduce user-visible strings without localization keys.
- For new UI-facing properties: label keys are required. Tooltip keys are strongly recommended for explanatory/complex properties (e.g., XPath/CSS selectors) and should include concise examples when useful.
- Build and test with `--no-restore` by default, especially after NuGet restore hangs or fails because NuGet cannot be reached.
- If a local build fails because build outputs are locked, the application is already running, or a similar local environment issue is present, do not work around it with temporary output directories. Report the situation to the developer and let them close the app or run the verification locally.
- Run restore only when NuGet package references were newly added or changed. If restore is required and cannot reach NuGet, do not keep retrying; report the restore problem and let the developer handle that verification step.
- Do not use ad-hoc reflection/introspection projects to discover NuGet API shapes; this has proven unreliable in this repository. Prefer the local package XML documentation, installed package docs, existing code usage, and compiler feedback.

## Protecting concurrent developer changes

- Existing developer changes take precedence and must not be overwritten with an earlier state previously read or generated by Codex.
- When editing a file for the first time, record the current file hash in the working context and read the required target section.
- After making a file change, update the stored hash. Before a later edit to the same file, especially after a new user message or a longer work step, compute the hash again.
- If the hash has changed since the last Codex edit, assume an interim developer change, reread the current target section, and treat that state as authoritative. Do not write back an older state previously produced by Codex.
- Preserve interim developer changes. Modify them only when required for the current task or when there is an obvious error; explicitly mention such corrections in the result.
- Full files usually do not need to be reread; a fingerprint and a small context section are normally enough.

## Icon Workflow
- For new/specific icons (for example for new FlowBlocks/ManagedObjects), do not copy random SVGs from `FlowBloxIcons.resx` first.
- First search for the best matching icon in Pictogrammers MDI.
- Download the selected SVG to `FlowBlox.Core/Resources`.
- Import it into `FlowBlox.Core/FlowBloxIcons.resx`, then use the generated key in code.
- If the icon is not imported yet, use `FlowBloxComponent` icon as temporary compile-safe fallback and switch to the final icon after import.

## FlowBlock Execute Contract
- Keep notifications scoped to the concrete operation/context of the flow block.
- Do not rely on `BaseFlowBlock` generic unexpected-error notification for expected domain failures.
- In `Execute(...)`, always leave via exactly one flow contract action:
  - non-result blocks: `ExecuteNextFlowBlocks(runtime)`
  - result blocks: `GenerateResult(...)`
- On handled failures, keep the same contract action (continue with next block or emit empty/default result), paired with a scoped notification.
- For iterator-like blocks, consider a dedicated warning notification when no items are found (empty but valid result set).

## ManagedObject UI Operations
- For ManagedObject associations, default to allowing all UI operations unless the domain context requires restrictions.
- In practice this means: if no explicit `UIOperations` are set, treat it as fully enabled (`Link`, `Unlink`, `Create`, `Edit`, `Delete`).
- Only constrain operations when there is a clear functional or security reason in the specific business context.
