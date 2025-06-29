# Changelog

All notable changes to Bundlingway will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0.3] - 2025-06-23

### New Features
- Users now receive a notification when the application begins unzipping downloaded files.
- The application now requests administrator privileges upon launch.

### Improvements
- File downloads now target zip files instead of executable files.
- Download progress notifications have been enhanced for clarity and reliability.
- Progress bar updates are now more robust and thread-safe, preventing UI freezes and errors.

### Refactor
- Progress bar update logic has been simplified for better maintainability.

## [0.1.0.2] - 2025-06-23

### Added
- Support for operations requiring elevated (administrator) privileges, including a new elevation service and handler for secure execution of system-level tasks.
- Enhanced HTTP client functionality to allow custom headers for network requests.
- Centralized user notifications through a new notification service, improving feedback and error messaging.
- Status check method for Bundlingway updates.

### Changed
- Migrated from static UI notification methods to a service-based approach for user announcements and progress reporting.
- Updated application startup to handle elevated operations and protocol invocations more robustly.
- Consolidated Bundlingway status checks for efficiency.

### Removed
- Unused files and legacy service locator code.
- Excessive and redundant comments for improved code readability.

### Fixed
- Improved error handling and user feedback during elevated and protocol-based operations.
- Updated version numbers and application manifest to run without requiring administrator privileges by default.

## [0.1.0.1] - 2025-06-22

### New Features

- Introduced a modular architecture separating core logic and UI, supporting multiple UI frameworks (WinForms, Blazor, Electron, CLI).
- Added comprehensive headless (command-line) operation mode and protocol handling.
- Implemented robust package management, configuration, update, and post-processing services with dependency injection.
- Provided new interfaces and services for notifications, progress reporting, file system, HTTP client, environment, single-instance, and inter-process communication.
- Added user notification and progress reporting implementations for UI and console environments.

### Improvements

- Migrated to dependency injection for improved testability and maintainability.
- Refactored UI to a Model-View-Presenter (MVP) pattern with async operations and service integration.
- Enhanced error handling, logging, null safety, and asynchronous programming throughout.
- Updated package onboarding, installation, removal, and update workflows for reliability and extensibility.
- Added detailed environment validation and health checks.
- Improved protocol registration robustness with verification and detailed logging.

### Bug Fixes

- Fixed null reference issues and added defensive checks in process, registry, and file operations.
- Corrected property initializations and type annotations to prevent runtime errors.

### Documentation

- Added detailed architecture modernization and migration strategy documents.
- Provided a comprehensive migration checklist to track progress.

### Chores

- Updated project dependencies and assembly versioning.
- Removed obsolete static helpers and legacy service locator pattern.
- Cleaned up UI static classes and replaced with service-based implementations.
- Removed unused files: DummyProgressReporter.cs, HeadlessScript.cs, Instances.cs, UI.cs, FS.cs.
- Cleaned up excessive and redundant comments throughout the codebase.

## [Unreleased]

### Added
### Changed
### Deprecated
### Removed
### Fixed
### Security
