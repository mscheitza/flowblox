# FlowBlox.Core.Runner

## Purpose

The `FlowBlox.Core.Runner` component provides a structured way to
execute FlowBlox projects programmatically.

It is responsible for: 
- Loading and executing a project 
- Applying user field and option overrides 
- Controlling abort behavior (error/warning) 
- Capturing logs 
- Collecting structured output datasets

This layer centralizes execution logic so that all hosts behave
consistently.

## Where It Is Used

The Runner is used by:

-   **ExecuteProjectFlowBlock** (via RunnerHost process)
-   **FlowBlox CLI**
-   **FlowBlox.Service** (Windows Service for configured project
    execution)

This ensures identical runtime behavior across interactive, background,
and service-based execution.

## Design Principles

-   Stateless execution model
-   Input via `RunnerRequest`
-   Output via `RunnerResponse`
-   No UI dependencies
-   Host-independent execution logic

The Runner represents execution infrastructure, not domain models.\
It is intentionally separated from general Models/Enums to keep domain
logic clean and focused.

------------------------------------------------------------------------

This component ensures consistent, reusable, and automation-friendly
project execution across the FlowBlox ecosystem.