# Installation

Download the latest release from the Release directory, and place it in the Roms/Apps directory of your TF1/TF2. 

To load the "App store", navigate to Apps and launch it.

> [!CAUTION]
>
> You do not need to read past this point unless you are a developer looking to add your own app to the store.

# RG35XX Store Submission Guide

Welcome to the RG35XX Store submission guide! This document will help you add your application to the RG35XX Store so that users can easily discover and download it.

## Table of Contents

- [How to Submit Your Application](#how-to-submit-your-application)
- [Understanding `index.json`](#understanding-indexjson)
  - [Field Explanations](#field-explanations)
  - [Download URL Construction](#download-url-construction)
- [Best Practices](#best-practices)
- [Need Help?](#need-help)

## How to Submit Your Application

To add your application to the RG35XX Store, you'll need to edit the `index.json` file and submit a Pull Request (PR). Here's how:

1. **Fork the Repository**

   - Visit the [RG35XX-Store repository](https://github.com/MrJackSpade/RG35XX-Store) on GitHub.
   - Click on the **Fork** button in the top-right corner to create your own copy of the repository.

2. **Clone Your Fork**

   - Clone your forked repository to your local machine:
     ```bash
     git clone https://github.com/<your-username>/RG35XX-Store.git
     ```
     Replace `<your-username>` with your GitHub username.

3. **Create a New Branch**

   - Navigate into the repository:
     ```bash
     cd RG35XX-Store
     ```
   - Create a new branch for your changes:
     ```bash
     git checkout -b add-my-app
     ```
     Replace `add-my-app` with a descriptive branch name.

4. **Edit `index.json`**

   - Open the `index.json` file in a text editor.
   - Add a new entry for your application following the existing JSON structure.
   - Ensure your JSON is valid by using a [JSON validator](https://jsonlint.com/).

   **Example Entry:**
   ```json
   {
     "root": "Release",
     "files": ["MyApp"],
     "name": "My Awesome App",
     "description": "An app that does amazing things",
     "repo": "MyAwesomeApp",
     "branch": "main",
     "author": "YourGitHubUsername"
   }
   ```

5. **Commit Your Changes**

   - Stage your changes:
     ```bash
     git add index.json
     ```
   - Commit with a descriptive message:
     ```bash
     git commit -m "Add My Awesome App to the store"
     ```

6. **Push Your Branch**

   - Push your changes to GitHub:
     ```bash
     git push origin add-my-app
     ```

7. **Open a Pull Request**

   - Go to your fork on GitHub.
   - Click on **Compare & pull request**.
   - Provide a clear title and description for your PR.
   - Submit the pull request.

8. **Respond to Feedback**

   - Maintain open communication with the repository maintainers.
   - Make any requested changes to your PR.

## Understanding `index.json`

The `index.json` file contains an array of application entries that the RG35XX Store app reads to display available applications. Each entry includes metadata and information necessary for downloading and installing the app.

### Field Explanations

Below is a detailed explanation of each field required in your application entry:

- **`root`** (String)

  - **Description**: The directory path in your repository where the application files are located.
  - **Usage**: Used to construct download URLs.
  - **Example**: `"Release"`, `"dist"`, `"build/output"`.

- **`files`** (Array of Strings)

  - **Description**: A list of filenames to be downloaded and installed.
  - **Usage**: Specifies which files in your repository will be downloaded.
  - **Example**: `["MyApp"]`, `["MyApp", "config.json"]`.

- **`name`** (String)

  - **Description**: The display name of your application.
  - **Usage**: Shown in the store's application list.
  - **Example**: `"My Awesome App"`.

- **`description`** (String)

  - **Description**: A brief description of your application.
  - **Usage**: Provides users with information about your app.
  - **Example**: `"An app that does amazing things"`.

- **`repo`** (String)

  - **Description**: The GitHub repository name where your application is hosted.
  - **Usage**: Used to construct download URLs.
  - **Example**: `"MyAwesomeApp"` if under your account, or `"username/MyAwesomeApp"` if under an organization or different user.

- **`branch`** (String)

  - **Description**: The branch in your repository where the application files are located.
  - **Usage**: Used to construct download URLs.
  - **Example**: `"main"`, `"master"`, `"release-v1.0"`.

- **`author`** (String)

  - **Description**: Your GitHub username or the name you wish to display as the author.
  - **Usage**: Displayed in the store and used to construct download URLs.
  - **Example**: `"YourGitHubUsername"`.

### Download URL Construction

The RG35XX Store app constructs download URLs for your application files using the information provided in your `index.json` entry. Here's how the URLs are built:

**Download URL Pattern:**

```
https://raw.githubusercontent.com/<author>/<repo>/<branch>/<root>/<file>
```

- `<author>`: The `author` field from your entry.
- `<repo>`: The `repo` field from your entry.
- `<branch>`: The `branch` field from your entry.
- `<root>`: The `root` field from your entry.
- `<file>`: Each filename listed in the `files` array.

**Example:**

Given the following entry:

```json
{
  "root": "Release",
  "files": ["MyApp"],
  "name": "My Awesome App",
  "description": "An app that does amazing things",
  "repo": "MyAwesomeApp",
  "branch": "main",
  "author": "YourGitHubUsername"
}
```

The download URL for `MyApp` would be:

```
https://raw.githubusercontent.com/YourGitHubUsername/MyAwesomeApp/main/Release/MyApp
```

**Important Notes:**

- **GitHub User Content Domain**: The `raw.githubusercontent.com` domain is used to directly access raw file contents from repositories.
- **File Accessibility**: Ensure that your repository and the files you wish to distribute are public or accessible without authentication.
- **Case Sensitivity**: GitHub URLs are case-sensitive. Ensure that the casing in your fields matches exactly with your repository.

## Best Practices

- **Validate Your JSON**: Use a JSON validator to ensure your `index.json` entry is correctly formatted.
- **Test Download URLs**: Before submitting, paste your constructed download URLs into a browser to verify they work.
- **Provide Accurate Information**: Double-check all fields for accuracy to prevent issues for users.
- **Keep Descriptions Concise**: Aim for clarity and brevity in your `description` field.
- **Respect Licensing**: Ensure that you have the rights to distribute any files you include, and that you comply with all applicable licenses.

## Need Help?

If you have any questions or need assistance:

- **Open an Issue**: Create an issue on the [RG35XX-Store repository](https://github.com/MrJackSpade/RG35XX-Store/issues).
- **Contact Maintainers**: Reach out to the repository maintainers directly via GitHub.

We're excited to see your application in the RG35XX Store!

---