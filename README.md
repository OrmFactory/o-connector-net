# O-Connector

**O-Connector** is a lightweight ADO.NET-compatible client library for connecting to databases through the high-performance binary protocol of [O-Bridge](https://github.com/OrmFactory/o-bridge).

It is designed specifically for use with **[OrmFactory](https://ormfactory.com)** - a modern developer tool for schema-first modeling and ORM code generation.
O-Connector enables OrmFactory to connect to Oracle in a fast and resource-efficient way, without relying on heavyweight native drivers.
## Features

- ADO.NET `DbConnection`, `DbCommand`, `DbDataReader` implementations
- Binary protocol optimized for latency and memory usage
- Async-first API for efficient I/O
- Full support for Oracle-specific types (e.g., `INTERVAL`, `NUMBER`, `DATE`)

## When to use

Use O-Connector if you:

- Need fast, low-allocation access to Oracle or similar databases
- Want to avoid installing native drivers (like Oracle Instant Client)
- Are working in sandboxed, cross-platform, or containerized environments
- Are using [O-Bridge](https://github.com/OrmFactory/o-bridge) as a backend protocol layer


This project is not affiliated with, endorsed by, or sponsored by Oracle Corporation.  
"Oracle" is a registered trademark of Oracle Corporation and/or its affiliates.

This library provides an alternative communication layer for Oracle-compatible clients.
