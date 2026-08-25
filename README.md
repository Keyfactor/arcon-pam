# cpr-pam-template

## Template for new PAM Provider integrations

### Use this repository to create new integrations for new PAM Provider integration types. 


1. [Use this repository](#using-the-repository)
1. [Update the integration-manifest.json](#updating-the-integration-manifest.json)
1. [Add Keyfactor Bootstrap Workflow (keyfactor-bootstrap-workflow.yml)](#add-bootstrap)
1. [Create required branches](#create-required-branches)
1. [Replace template files/folders](#replace-template-files-and-folders)
1. [Create initial prerelease](#create-initial-prerelease)
---

#### Using the repository
1. Select the ```Use this template``` button at the top of this page
1. Update the repository name following [these guidelines](https://keyfactorinc.sharepoint.com/sites/IntegrationWiki/SitePages/GitHub-Processes.aspx#repository-naming-conventions) 
    1. All repositories must be in lower-case
	1. General pattern: company-product-type
	1. e.g. hashicorp-vault-pam
1. Click the ```Create repository``` button

---

#### Updating the integration-manifest.json

*The following properties must be updated in the integration-manifest.json*

Clone the repository locally, use vsdev.io, or the GitHub online editor to update the file.

* "name": "Friendly name for the integration"
	* This will be used in the readme file generation and catalog entries
* "description": "Brief description of the integration."
	* This will be used in the readme file generation
	* If the repository description is empty this value will be used for the repository description upon creating a release branch
* "release_dir": "PATH\\\TO\\\BINARY\\\RELEASE\\\OUTPUT\\\FOLDER"
	* Path separators can be "\\\\" or "/"
	* Be sure to specify the release folder name. This can be found by running a Release build and noting the output folder
	* Example: "AzureAppGatewayOrchestrator\\bin\\Release"
* "regDLL": "Assembly Name",
* "qualifiedName": "Fully Qualified Assembly Name",
* "dbName": "PAM vault dbName",
* "name": "Pam Provider Name"

## PAM readme support files
* Contents of the readme-src\* files must be updated for this integration. If these are not needed, they can be safely deleted.
* The images\* files can be updated to integration-specific screenshots or safely deleted

---

If the repository is ready to be published in the public catalog, the following property must be updated:
* "update_catalog": true

When the repository is ready to be made public, the catalog must include a property to display the link to the github repo.
* "link_github": true

## For a more detailed description, see the [Integration Wiki](https://keyfactorinc.sharepoint.com/sites/IntegrationWiki/SitePages/PAM-README.md-Build.aspx)
* Including an [annotated pdf](https://keyfactorinc.sharepoint.com/:b:/s/IntegrationWiki/EQNS6-4_QpNGoUiGqM80BAIBE4hMa2-a9FD7w7Tryv4PEA?e=ltSQut) descibing each of the files and fields required
