## Overview

The ARCON PAM integration for Keyfactor enables secure retrieval of secrets (variables) from [ARCON](https://arconnet.com/) instances, using API calls to fetch credentials for assets stored in the ARCON Digital Vault. Authentication to the ARCON API is performed using a dedicated API username and password. To retrieve a password for an asset, you must supply the asset's `Username` and `Alias` as configured in ARCON Digital Vault.

Each password retrieval requests a 1-hour checkout window from ARCON Digital Vault. This window is fixed by the integration and is not configurable.

This PAM integration can run on either Keyfactor Command or Universal Orchestrator.

## Requirements

### ARCON
- **Tested with**: ARCON Partner CI (V10.11.003_HF2)
- **Authentication**: An interactive API username / password that is registered in ARCON Digital Vault
- **Supported products**: ARCON (self-hosted or cloud)

### Keyfactor Platform
- **Minimum .NET Runtime**: .NET 8
- **Keyfactor Command**: Version 12.0 or later
- **Universal Orchestrator**: Version 12.3 or later

