FlowBlox

FlowBlox is a visual flow-based automation platform.
You compose flows from reusable FlowBlocks, connect them on a project grid, and execute data-driven runtime pipelines.


Quick Start

1. Download the latest release from:
   https://flowbloxweb.net/
   Use "Download for Windows x64".

2. Install the MSI package.
   The installer includes required ONNX runtime packages.

3. Open FlowBlox and explore sample projects:
   FlowBlox App -> Extras -> View projects (Project Space)

4. Create your first flow:
   - Drag a Start FlowBlock from the component library to the grid.
   - Add additional FlowBlocks and connect them.
   - Configure properties and required fields.
   - Run and inspect results.

5. Optional: auto-align your flow layout:
   FlowBlox App -> Edit -> Automatically align flow

6. Optional: use the AI Assistant to create or update flows by text.
   The assistant is shown in the right panel.
   You can configure model/provider settings in:
   FlowBlox App -> Extras -> Options -> AI


Platform Support

- Currently supported platform: Windows x64.
- A platform port is in progress via migration to WPF and planned usage of Avalonia XPF.
- Target platforms for future UI support include macOS and Unix/Linux.
- FlowBlox.Core runtime is potentially usable cross-platform and can be built separately for non-Windows environments.

Build scripts are available in:
- scripts/


AI / ONNX Setup

For local AI flows, select the desired execution provider in:
FlowBlox App -> Extras -> Options -> AI -> AI Execution Provider

Supported provider options depend on your environment and can include:
- CPU (default)
- CUDA
- DirectML
- OpenVINO

This applies to both:
- ONNX Runtime-based features
- ONNX GenAI-based features


AI Assistant

FlowBlox includes an integrated AI Assistant (right-side panel) that can create and refactor flows from plain text.

Current provider integrations include:
- OpenAI
- Anthropic
- Google Gemini

Additional providers are in progress.

Example prompt:
"Create a flow that executes an SQL query, iterates all rows, reads JSON property XZ, keeps only rows where the value is XY, and exports the result to CSV."

This is useful for complex cases that would otherwise require custom coding.


Practical Use Case Example

A common real-world task:
- Run an SQL command that returns many rows (for example 1000+)
- Inspect a JSON value per row
- Filter rows by condition
- Store filtered output in a target format

In many environments this requires ad-hoc scripts.
With FlowBlox you can solve it visually (or via AI Assistant) using standard building blocks such as SQL readers, JSON selectors, validators, and file writers.


Core Concepts

1) What Is a FlowBlock?

A FlowBlock is the basic executable unit in FlowBlox.
Each block encapsulates one operation (for example: read table data, format text, call web endpoints, write output).

Every FlowBlock supports:
- Activation conditions
- Required fields
- Structured inputs/outputs depending on block type


2) Flow Execution Logic

Flow execution is dataset-driven.

If a source FlowBlock produces result datasets, each direct successor is invoked once per dataset.
If no result datasets are produced, successors are invoked once.

Execution order of next FlowBlocks is:
- creation order, or
- explicit execution index (starting at 0)

You can set an index via context menu on a FlowBlock:
- Right-click FlowBlock -> Set index


3) Datasets and Fields

Result FlowBlocks produce 1..N datasets.
A dataset is a collection of field values defined by that block.

Example:
A TableReader with mappings for FirstName, LastName, Salutation creates one dataset per input row.
Successor invocations receive these datasets one by one.

Field values can be consumed:
- via direct field references
- via fully qualified field names in text properties

Field value propagation remains unambiguous along the invocation path, including deeper successors.


4) Result Block Variants

Common output-capable patterns:
- Result FlowBlock: N results
- SingleResult FlowBlock: 1 result
- Pipe FlowBlock: 1 input -> 1 result


5) Multi-Input Combination and Input Behavior

When multiple input FlowBlocks connect to one successor, FlowBlox builds combined datasets according to configured input behavior.
Default behavior is CROSS (combinational).

The successor is invoked N times, where N is the number of resulting combined datasets.

Input behavior can be configured per incoming source in:
- Edit FlowBlock -> Input

This enables strategies such as:
- first dataset from source A
- row-wise from sources B/C
- last dataset from source D


6) Iteration Context

For multi-input scenarios, iteration context defines when dataset combination and target invocation occur.

Default behavior:
- Automatically resolved to the first common upstream FlowBlock across incoming paths.

You can override it manually in:
- Input -> IterationContext

Practical idea:
The target executes after the iteration context completes, then combined datasets are processed.


Testing

FlowBlox supports test definitions directly on FlowBlocks.

Location:
- Edit FlowBlock -> Tests

You can:
- create or link test cases
- define expectations on target FlowBlocks
- provide predecessor field values manually
- combine manual setup with automatic value selection

This model supports scalable test-driven flow design.


Generators

Generators use test definitions to derive or repair FlowBlock configuration.

Available strategies include:
- AIPropertyValueGenerationStrategy
  Generates property values from input examples + property context + test expectations.
  Example: generate a regex for extracting company names from DOM content.

- SequenceDetectionGenerationStrategy
  Non-AI strategy for complex hierarchical inputs (for example DOM/tree structures),
  generating structural patterns that match target values.

- OnnxPropertyValueGenerationStrategy
  In progress.

Runtime safety mode:
If a generator-relevant test is marked "required for runtime execution":
- Runtime checks test status before execution.
- If test is failing, generator runs.
- If test passes afterward, runtime continues.
- If test still fails, runtime aborts.

This ensures only valid/generated configurations reach production runtime.


Flow Design Patterns (Readability)

Default pattern:
- Sequential downstream chain where each step depends on previous step output.

Stack pattern (organization/readability):
- Multiple independent next FlowBlocks connected to one source block.
- Useful for prerequisite subtasks (configuration extraction, preparatory actions) before core flow logic.
- Helps avoid overly long horizontal chains and improves visual comprehension.

Use sequential chaining when operations are strongly dependent.
Use stacked branches when tasks are independent and can be organized as parallel prerequisites.


Project Philosophy and Community Notes

FlowBlox is designed as a community-friendly project:
- Modular
- Open and extensible
- Provider-independent
- Locally runnable with easy setup (download -> install -> start)

If you see missing functionality for your use case, contributions are welcome:
- Add or improve FlowBlocks in core components
- Provide features through extension management
- Fix bugs and open pull requests

This is an invitation, not an expectation.
If you find value in FlowBlox and want to contribute missing puzzle pieces for your own scenarios, your contribution is appreciated.

Technical openness:
- FlowBlock APIs are adaptable.
- Extension API and Project Space API can evolve with contributions.
- FlowBlox.Core runtime is platform-independent in concept and can be hosted in cloud services (for example Azure) if needed.
- For custom hosting/execution, use and integrate FlowBlox.Core runner components (for example FlowBloxProjectRunner).


License

This project is licensed under the MIT License.
See LICENSE.txt for details.
