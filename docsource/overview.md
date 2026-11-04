## Overview
The ARCON PAM integration for Keyfactor enables secure retrieval of secrets (variables) from [ARCON](https://arconnet.com/) instances, using API calls to fetch credentials for assets stored in the ARCON Digital Vault. Authentication to the ARCON API is performed using a dedicated API username and password. To retrieve a password for an asset, you must supply the asset's `Username` and `Alias` as configured in ARCON Digital Vault.

Each password retrieval requests a 1-hour checkout window from ARCON Digital Vault. This window is fixed by the integration and is not configurable.

This PAM integration can run on either Keyfactor Command or Universal Orchestrator.