<h1 align="center" style="border-bottom: none">
    Arcon PAM Provider
</h1>

<p align="center">
  <!-- Badges -->
<img src="https://img.shields.io/badge/integration_status-production-3D1973?style=flat-square" alt="Integration Status: production" />
<a href="https://github.com/Keyfactor/arcon-pam/releases"><img src="https://img.shields.io/github/v/release/Keyfactor/arcon-pam?style=flat-square" alt="Release" /></a>
<img src="https://img.shields.io/github/issues/Keyfactor/arcon-pam?style=flat-square" alt="Issues" />
<img src="https://img.shields.io/github/downloads/Keyfactor/arcon-pam/total?style=flat-square&label=downloads&color=28B905" alt="GitHub Downloads (all assets, all releases)" />
</p>

<p align="center">
  <!-- TOC -->
  <a href="#support">
    <b>Support</b>
  </a>
  ·
  <a href="#getting-started">
    <b>Installation</b>
  </a>
  ·
  <a href="#license">
    <b>License</b>
  </a>
  ·
  <a href="https://github.com/orgs/Keyfactor/repositories?q=pam">
    <b>Related Integrations</b>
  </a>
</p>

## Overview
The ARCON PAM integration for Keyfactor enables secure retrieval of secrets (variables) from [ARCON](https://arconnet.com/) instances, using API calls to fetch credentials for assets stored in the ARCON Digital Vault. Authentication to the ARCON API is performed using a dedicated API username and password. To retrieve a password for an asset, you must supply the asset's `Username` and `Alias` as configured in ARCON Digital Vault.

Each password retrieval requests a 1-hour checkout window from ARCON Digital Vault. This window is fixed by the integration and is not configurable.

This PAM integration can run on either Keyfactor Command or Universal Orchestrator.

## Support
The Arcon PAM Provider is supported by Keyfactor for Keyfactor customers. If you have a support issue, please open a support ticket via the Keyfactor Support Portal at https://support.keyfactor.com.

> To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.

## Getting Started

The Arcon PAM Provider is used by Command to resolve PAM-eligible credentials for Universal Orchestrator extensions and for accessing Certificate Authorities. When configured, Command will use the Arcon PAM Provider to retrieve credentials needed to communicate with the target system. There are two ways to install the Arcon PAM Provider, and you may elect to use one or both methods:

1. **Locally on the Keyfactor Command server**: PAM credential resolution via the Arcon PAM Provider will occur on the Keyfactor Command server each time an elegible credential is needed.
2. **Remotely On Universal Orchestrators**: When Jobs are dispatched to Universal Orchestrators, the associated Certificate Store extension assembly will use the Arcon PAM Provider to resolve eligible PAM credentials.

Before proceeding with installation, you should consider which pattern is best for your requirements and use case.

### Installation

> [!IMPORTANT]
> For the most up-to-date and complete documentation on how to install a PAM provider extension, please visit our [product documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/ReferenceGuide/Preparing%20Third%20Party%20PAM%20Providers%20to%20Work%20with.htm?Highlight=pam%20provider#InstallingCustomPAMProviderExtensions)


To install Arcon PAM Provider, it is recommended you install [kfutil](https://github.com/Keyfactor/kfutil). `kfutil` is a command-line tool that simplifies the process of creating PAM Types in Keyfactor Command.



#### Requirements
   ### ARCON
   - **Tested with**: ARCON Partner CI (V10.11.003_HF2)
   - **Authentication**: An interactive API username / password that is registered in ARCON Digital Vault
   - **Supported products**: ARCON (self-hosted or cloud)
   
   ### Keyfactor Platform
   - **Minimum .NET Runtime**: .NET 8
   - **Keyfactor Command**: Version 12.0 or later
   - **Universal Orchestrator**: Version 12.3 or later

#### Create PAM type in Keyfactor Command


##### Using `kfutil`
Create the required PAM Types in the connected Command platform.

```shell
# Arcon
kfutil pam-types create -r arcon-pam -n Arcon
```

##### Using the API
For full API docs please visit our [product documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/WebAPI/KeyfactorAPI/PAMProvidersPOSTTypes.htm?Highlight=pam%20type)

Below is the payload to `POST` to the Keyfactor Command API
```json
{
  "Name": "Arcon",
  "Parameters": [
    {
      "Name": "ApiHost",
      "DisplayName": "API Host",
      "DataType": 1,
      "InstanceLevel": false,
      "Description": "The host URL for the Arcon API, including the protocol (e.g., https://api.arcon.com)."
    },
    {
      "Name": "ApiUsername",
      "DisplayName": "API Username",
      "DataType": 1,
      "InstanceLevel": false,
      "Description": "The username for the Arcon API."
    },
    {
      "Name": "ApiPassword",
      "DisplayName": "API Password",
      "DataType": 2,
      "InstanceLevel": false,
      "Description": "The password for the Arcon API."
    },
    {
      "Name": "AssetUsername",
      "DisplayName": "Asset Username",
      "DataType": 1,
      "InstanceLevel": true,
      "Description": "The username for the asset in the Arcon system. The integration will retrieve the password associated with the username and alias."
    },
    {
      "Name": "AssetAlias",
      "DisplayName": "Asset Alias",
      "DataType": 1,
      "InstanceLevel": true,
      "Description": "The alias for the asset in the Arcon system. The integration will retrieve the password associated with the alias and username."
    }
  ]
}
```

#### Install PAM provider on Keyfactor Command Host (Local)


1. On the server that hosts Keyfactor Command, download and unzip the latest release of the Arcon PAM Provider from the [Releases](../../releases) page.

2. Copy the assemblies to the appropriate directories on the Keyfactor Command server:

    <details><summary>Keyfactor Command 11+</summary>

    1. Copy the unzipped assemblies to each of the following directories:

        * `C:\Program Files\Keyfactor\Keyfactor Platform\WebAgentServices\Extensions\arcon-pam`
        * `C:\Program Files\Keyfactor\Keyfactor Platform\WebConsole\Extensions\arcon-pam`
        * `C:\Program Files\Keyfactor\Keyfactor Platform\KeyfactorAPI\Extensions\arcon-pam`

    </details>

    <details><summary>Keyfactor Command 10</summary>

    1. Copy the assemblies to each of the following directories:

        * `C:\Program Files\Keyfactor\Keyfactor Platform\WebAgentServices\bin\arcon-pam`
        * `C:\Program Files\Keyfactor\Keyfactor Platform\KeyfactorAPI\bin\arcon-pam`
        * `C:\Program Files\Keyfactor\Keyfactor Platform\WebConsole\bin\arcon-pam`
        * `C:\Program Files\Keyfactor\Keyfactor Platform\Service\arcon-pam`

    2. Open a text editor on the Keyfactor Command server as an administrator and open the `web.config` file located in the `WebAgentServices` directory.

    3. In the `web.config` file, locate the `<container> </container>` section and add the following registration:

        ```xml
        <container>
            ...
            <!--The following are PAM Provider registrations. Uncomment them to use them in the Keyfactor Product:-->

            <!--Add the following line exactly to register the PAM Provider-->
            <register type="IPAMProvider" mapTo="Keyfactor.Extensions.Pam.Arcon, Keyfactor.Command.PAMProviders" name="Arcon" />
        </container>
        ```

    4. Repeat steps 2 and 3 for each of the directories listed in step 1. The configuration files are located in the following paths by default:

        * `C:\Program Files\Keyfactor\Keyfactor Platform\WebAgentServices\web.config`
        * `C:\Program Files\Keyfactor\Keyfactor Platform\KeyfactorAPI\web.config`
        * `C:\Program Files\Keyfactor\Keyfactor Platform\WebConsole\web.config`
        * `C:\Program Files\Keyfactor\Keyfactor Platform\Service\CMSTimerService.exe.config`

    </details>

3. Restart the Keyfactor Command services (`iisreset`).

#### Install PAM provider on a Universal Orchestrator Host (Remote)


1. Install the Arcon PAM Provider assemblies.

    * **Using kfutil**: On the server that hosts the Universal Orchestrator, run the following command:

        ```shell
        # Windows Server
        kfutil orchestrator extension -e arcon-pam@latest --out "C:\Program Files\Keyfactor\Keyfactor Orchestrator\extensions"

        # Linux
        kfutil orchestrator extension -e arcon-pam@latest --out "/opt/keyfactor/orchestrator/extensions"
        ```

    * **Manually**: Download the latest release of the Arcon PAM Provider from the [Releases](../../releases) page. Extract the contents of the archive to:

        * **Windows Server**: `C:\Program Files\Keyfactor\Keyfactor Orchestrator\extensions\arcon-pam`
        * **Linux**: `/opt/keyfactor/orchestrator/extensions/arcon-pam`

2. Included in the release is a `manifest.json` file that contains the following object:
    ```json
    {
      "Keyfactor:PAMProviders:Arcon:InitializationInfo": {
        "ApiHost": "<ApiHost>",
        "ApiUsername": "<ApiUsername>",
        "ApiPassword": "<ApiPassword>"
      }
    }
    ```

    Populate the fields in this object with credentials and configuration data collected in the [requirements](docs/arcon.md#requirements) section.

3. Restart the Universal Orchestrator service.


### Usage


#### From Keyfactor Command Host (Local)

##### Define a PAM provider in Command
1. In the Keyfactor Command Portal, hover over the ⚙️  (settings) icon in the top right corner of the screen and select **Priviledged Access Management**.

2. Select the **Add** button to create a new PAM provider. Click the dropdown for **Provider Type** and select **Arcon**.

> [!IMPORTANT]
> If you're running Keyfactor Command 11+, make sure `Remote Provider` is unchecked.

3. Populate the fields with the necessary information collected in the [requirements](docs/arcon.md#requirements) section:

| Initialization parameter | Display Name | Description |
| --- | --- | --- |
| ApiHost | API Host | The host URL for the Arcon API, including the protocol (e.g., https://api.arcon.com). |
| ApiUsername | API Username | The username for the Arcon API. |
| ApiPassword | API Password | The password for the Arcon API. |


4. Click **Save**. The PAM provider is now available for use in Keyfactor Command.

##### Using the PAM provider

Now, when defining Certificate Stores (**Locations**->**Certificate Stores**), **Arcon** will be available as a PAM provider option. When defining new Certificate Stores, the secret parameter form will display tabs for **Load From Keyfactor Secrets** or **Load From PAM Provider**. 

Select the **Load From PAM Provider** tab, choose the **Arcon** provider from the list of **Providers**, and populate the fields with the necessary information from the table below:

| Instance parameter | Display Name | Description |
| --- | --- | --- |
| AssetUsername | Asset Username | The username for the asset in the Arcon system. The integration will retrieve the password associated with the username and alias. |
| AssetAlias | Asset Alias | The alias for the asset in the Arcon system. The integration will retrieve the password associated with the alias and username. |


#### From a Universal Orchestrator Host (Remote)


<details><summary>Keyfactor Command 11+</summary>

##### Define a remote PAM provider in Command

In Command 11 and greater, before using the Arcon PAM type, you must define a Remote PAM Provider in the Command portal.

1. In the Keyfactor Command Portal, hover over the ⚙️  (settings) icon in the top right corner of the screen and select **Priviledged Access Management**.

2. Select the **Add** button to create a new PAM provider.

3. Make sure that `Remote Provider` is checked.

4. Click the dropdown for **Provider Type** and select **Arcon**. 

5. Give the provider a unique name.

6. Click "Save".

##### Using the PAM provider

When defining Certificate Stores (**Locations**->**Certificate Stores**), **Arcon** can be used as a PAM provider. When defining a new Certificate Store, the secret parameter form will display tabs for **Load From Keyfactor Secrets** or **Load From PAM Provider**.

Select the **Load From PAM Provider** tab, choose the **Arcon** provider from the list of **Providers**, and populate the fields with the necessary information from the table below:

| Instance parameter | Display Name | Description |
| --- | --- | --- |
| AssetUsername | Asset Username | The username for the asset in the Arcon system. The integration will retrieve the password associated with the username and alias. |
| AssetAlias | Asset Alias | The alias for the asset in the Arcon system. The integration will retrieve the password associated with the alias and username. |


</details>

<details><summary>Keyfactor Command 10</summary>

When defining Certificate Stores (**Locations**->**Certificate Stores**), **Arcon** can be used as a PAM provider.

When entering Secret fields, select the **Load From Keyfactor Secrets** tab, and populate the **Secret Value** field with the following JSON object:

```json
{"AssetUsername":"The username for the asset in the Arcon system. The integration will retrieve the password associated with the username and alias.","AssetAlias":"The alias for the asset in the Arcon system. The integration will retrieve the password associated with the alias and username."}

```

> We recommend creating this JSON object in a text editor, and copying it into the Secret Value field.

</details>


> [!NOTE]
> Additional information on Arcon can be found in the [supplemental documentation](docs/arcon.md).

## License

Apache License 2.0, see [LICENSE](LICENSE)

## Related Integrations

See all [Keyfactor PAM Provider extensions](https://github.com/orgs/Keyfactor/repositories?q=pam).
