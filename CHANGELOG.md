# Changelog

## 0.1.2

- Add badges, Development section to README
- Add GenerateDocumentationFile, RepositoryType, PackageReadmeFile to .csproj

## 0.1.0 (2026-03-15)

- Initial release
- `OutboxMessage` record for outbox message representation
- `IOutboxStore` abstraction for message persistence
- `IOutboxDispatcher` interface for message dispatch
- `OutboxRelayService` background hosted service for polling and dispatching
- `OutboxOptions` for configurable polling interval, batch size, and max retries
- `AddOutbox()` extension method for DI registration
