#!/usr/bin/env python3
"""
AI Code Review Bot
Analyzes PR diffs using OpenAI and posts inline review comments.

TODO: Future enhancements:
- Model ramping based on diff size (gpt-4o-mini for small, gpt-4o for large)
- File type specific prompts (C# vs Python vs JS)
- Ignore patterns (generated files, lock files, etc.)
- Rate limiting / cost tracking
"""

import os
import sys
import json
import re
import requests
from openai import OpenAI, AzureOpenAI

# Configuration from environment
GITHUB_TOKEN = os.environ.get("GITHUB_TOKEN")
PR_NUMBER = os.environ.get("PR_NUMBER")
REPO_FULL_NAME = os.environ.get("REPO_FULL_NAME")
HEAD_SHA = os.environ.get("HEAD_SHA")
BASE_SHA = os.environ.get("BASE_SHA")
MAX_DIFF_LINES = int(os.environ.get("MAX_DIFF_LINES", 750))

# OpenAI Configuration - supports both Azure OpenAI and OpenAI directly
# Set USE_AZURE_OPENAI=true to use Azure OpenAI
USE_AZURE_OPENAI = os.environ.get("USE_AZURE_OPENAI", "false").lower() == "true"

# Standard OpenAI settings
OPENAI_API_KEY = os.environ.get("OPENAI_API_KEY")
OPENAI_MODEL = os.environ.get("OPENAI_MODEL", "gpt-4o-mini")

# Azure OpenAI settings (only used if USE_AZURE_OPENAI=true)
AZURE_OPENAI_API_KEY = os.environ.get("AZURE_OPENAI_API_KEY")
AZURE_OPENAI_ENDPOINT = os.environ.get("AZURE_OPENAI_ENDPOINT")  # e.g., https://your-resource.openai.azure.com
AZURE_OPENAI_DEPLOYMENT = os.environ.get("AZURE_OPENAI_DEPLOYMENT")  # Your deployment name
AZURE_OPENAI_API_VERSION = os.environ.get("AZURE_OPENAI_API_VERSION", "2024-08-01-preview")

# File extensions to review (focused on C#/.NET - skip config/workflow files)
REVIEWABLE_EXTENSIONS = {".cs"}

# Files/patterns to ignore
IGNORE_PATTERNS = {
    "package-lock.json",
    ".md",
    "yarn.lock",
    ".designer.cs",
    ".g.cs",
    "Migrations/",
}

GITHUB_API_BASE = "https://api.github.com"


def get_pr_files():
    """Fetch the list of changed files in the PR."""
    url = f"{GITHUB_API_BASE}/repos/{REPO_FULL_NAME}/pulls/{PR_NUMBER}/files"
    headers = {
        "Authorization": f"token {GITHUB_TOKEN}",
        "Accept": "application/vnd.github.v3+json",
    }
    response = requests.get(url, headers=headers)
    response.raise_for_status()
    return response.json()


def should_review_file(filename):
    """Check if a file should be reviewed based on extension and ignore patterns."""
    for pattern in IGNORE_PATTERNS:
        if pattern in filename:
            return False
    _, ext = os.path.splitext(filename)
    return ext.lower() in REVIEWABLE_EXTENSIONS


def parse_diff_for_positions(patch):
    """Parse a unified diff patch to map line numbers to diff positions."""
    if not patch:
        return {}
    line_map = {}
    diff_position = 0
    current_new_line = 0
    for line in patch.split("\n"):
        diff_position += 1
        if line.startswith("@@"):
            match = re.search(r"\+(\d+)", line)
            if match:
                current_new_line = int(match.group(1)) - 1
        elif line.startswith("-"):
            pass  # Deleted line - no new line number
        elif line.startswith("+"):
            current_new_line += 1
            line_map[current_new_line] = diff_position
        else:
            current_new_line += 1
            line_map[current_new_line] = diff_position
    return line_map


def count_changed_lines(files):
    """Count total added/modified lines across all files."""
    total = 0
    for file in files:
        total += file.get("additions", 0) + file.get("deletions", 0)
    return total



def build_review_prompt(files_with_diffs):
    """Build the prompt for OpenAI with the diffs to review."""
    prompt = """You are an expert C#/.NET code reviewer. Review the following code changes for ACTUAL BUGS ONLY.

## IMPORTANT: Be Extremely Selective

You MUST only flag code that has a HIGH probability of causing a runtime bug, security vulnerability, or data loss.

DO NOT flag:
- Style preferences or formatting
- Missing error handling that would just cause the program to crash (that's often acceptable)
- Generic suggestions like "add logging" or "add validation"
- Code that retrieves environment variables (this is standard practice)
- Code that could theoretically be improved but works correctly
- Performance suggestions unless there's an actual O(n²) or worse issue in a hot path
- Suggestions to add try/catch blocks around code that already has error handling

## What to Actually Flag (BUGS ONLY)

Only flag these specific issues:
- **BANNED: AutoMapper** - Flag ANY use of AutoMapper. Suggest manual mapping instead.
- Async/await deadlocks: `.Result`, `.Wait()`, `Task.Run(...).Result`
- Null dereference that WILL throw (not might throw)
- SQL/NoSQL injection with string interpolation in queries
- Hardcoded secrets/passwords/API keys in source code
- Resource leaks: FileStream/SqlConnection without using/dispose
- .First() without .FirstOrDefault() when collection might be empty
- Infinite loops or obvious logic errors
- Thread safety issues with shared mutable state

## Examples of Code to FLAG

```csharp
// FLAG: Deadlock risk
var result = GetDataAsync().Result;

// FLAG: Will throw on empty collection
var item = items.First();

// FLAG: SQL injection
var query = $"SELECT * FROM Users WHERE Id = {userId}";

// FLAG: Hardcoded secret
var apiKey = "sk-1234567890abcdef";

// FLAG: Resource leak
var stream = new FileStream(path, FileMode.Open);
// ... no using, no dispose

// FLAG: AutoMapper (BANNED)
var dto = _mapper.Map<UserDto>(user);
```

## Examples of Code to IGNORE (do NOT flag)

```csharp
// IGNORE: Standard env var retrieval - this is fine
var token = os.environ.get("API_KEY")
var config = Environment.GetEnvironmentVariable("CONFIG")

// IGNORE: Error handling that crashes is acceptable
response.raise_for_status()  // Let it throw, that's fine

// IGNORE: FirstOrDefault with null check is safe
var item = items.FirstOrDefault();
if (item == null) return NotFound();

// IGNORE: Proper async
var result = await GetDataAsync();

// IGNORE: Using statement is safe
using var stream = new FileStream(path, FileMode.Open);
```

## Response Rules

1. If the code looks reasonable, return: {"comments": []}
2. Only flag ACTUAL BUGS with HIGH confidence
3. Do not make generic suggestions
4. Do not flag Python/JavaScript/YAML for C#-specific issues

## Code to Review

"""
    for file_info in files_with_diffs:
        prompt += f"\n### File: {file_info['filename']}\n```\n{file_info['patch']}\n```\n"

    prompt += "\n\nRespond with ONLY a JSON object containing a comments array, no other text."
    return prompt

def get_openai_client():
    """Get the appropriate OpenAI client based on configuration."""
    if USE_AZURE_OPENAI:
        print(f"Using Azure OpenAI: {AZURE_OPENAI_ENDPOINT}")
        return AzureOpenAI(
            api_key=AZURE_OPENAI_API_KEY,
            api_version=AZURE_OPENAI_API_VERSION,
            azure_endpoint=AZURE_OPENAI_ENDPOINT
        )
    else:
        print("Using OpenAI directly")
        return OpenAI(api_key=OPENAI_API_KEY)


def call_openai_for_review(prompt):
    """Send the diff to OpenAI and get review comments."""
    client = get_openai_client()

    # Use deployment name for Azure, model name for OpenAI
    model = AZURE_OPENAI_DEPLOYMENT if USE_AZURE_OPENAI else OPENAI_MODEL

    response = client.chat.completions.create(
        model=model,
        messages=[
            {
                "role": "system",
                "content": "You are a helpful code reviewer. Always respond with valid JSON."
            },
            {
                "role": "user",
                "content": prompt
            }
        ],
        temperature=0.3,  # Lower temperature for more consistent reviews
        response_format={"type": "json_object"}
    )

    content = response.choices[0].message.content

    # Parse the JSON response
    try:
        result = json.loads(content)
        # Handle both {"comments": [...]} and [...] formats
        if isinstance(result, list):
            return result
        elif isinstance(result, dict) and "comments" in result:
            return result["comments"]
        else:
            return []
    except json.JSONDecodeError:
        print(f"Failed to parse OpenAI response as JSON: {content}")
        return []



def post_review_comments(comments, files_data):
    """Post inline review comments to the PR using GitHub's review API."""
    if not comments:
        print("No issues found - code looks good!")
        post_summary_comment("✅ **AI Code Review**: No issues found. The changes look good!")
        return

    # Build position maps for all files
    position_maps = {}
    for file in files_data:
        filename = file["filename"]
        position_maps[filename] = parse_diff_for_positions(file.get("patch", ""))

    # Prepare review comments
    review_comments = []
    for comment in comments:
        filename = comment.get("file", "")
        line = comment.get("line", 0)
        body = comment.get("body", "")

        if not filename or not line or not body:
            continue

        # Find the diff position for this line
        position_map = position_maps.get(filename, {})
        position = position_map.get(line)

        if position:
            review_comments.append({
                "path": filename,
                "position": position,
                "body": f"🤖 **AI Review**:\n\n{body}"
            })
        else:
            print(f"Could not map line {line} in {filename} to diff position")

    if review_comments:
        # Post as a PR review with inline comments
        url = f"{GITHUB_API_BASE}/repos/{REPO_FULL_NAME}/pulls/{PR_NUMBER}/reviews"
        headers = {
            "Authorization": f"token {GITHUB_TOKEN}",
            "Accept": "application/vnd.github.v3+json",
        }

        review_body = f"🤖 **AI Code Review** found {len(review_comments)} item(s) to discuss."

        data = {
            "commit_id": HEAD_SHA,
            "body": review_body,
            "event": "COMMENT",  # Use COMMENT, not REQUEST_CHANGES (less aggressive)
            "comments": review_comments
        }

        response = requests.post(url, headers=headers, json=data)

        if response.status_code == 200:
            print(f"Successfully posted review with {len(review_comments)} inline comments")
        else:
            print(f"Failed to post review: {response.status_code}")
            print(response.text)
            post_summary_comment(format_fallback_summary(comments))
    else:
        post_summary_comment(format_fallback_summary(comments))


def post_summary_comment(body):
    """Post a general comment on the PR (fallback if inline comments fail)."""
    url = f"{GITHUB_API_BASE}/repos/{REPO_FULL_NAME}/issues/{PR_NUMBER}/comments"
    headers = {
        "Authorization": f"token {GITHUB_TOKEN}",
        "Accept": "application/vnd.github.v3+json",
    }

    response = requests.post(url, headers=headers, json={"body": body})
    if response.status_code == 201:
        print("Posted summary comment")
    else:
        print(f"Failed to post summary comment: {response.status_code}")


def format_fallback_summary(comments):
    """Format comments as a summary when inline comments aren't possible."""
    lines = ["🤖 **AI Code Review**\n"]

    for comment in comments:
        filename = comment.get("file", "")
        line = comment.get("line", 0)
        body = comment.get("body", "")

        # Skip comments with missing required fields
        if not filename or not line or not body:
            continue

        lines.append(f"**{filename}** (line {line}):\n> {body}\n")

    # If all comments were filtered out, return a success message
    if len(lines) == 1:
        return "✅ **AI Code Review**: No issues found. The changes look good!"

    return "\n".join(lines)



def main():
    """Main entry point for the AI code review."""
    print(f"Starting AI Code Review for PR #{PR_NUMBER}")
    print(f"Using model: {OPENAI_MODEL}")
    print(f"Max diff lines: {MAX_DIFF_LINES}")

    # Validate environment
    if not GITHUB_TOKEN:
        print("ERROR: Missing GITHUB_TOKEN")
        sys.exit(1)

    if USE_AZURE_OPENAI:
        if not AZURE_OPENAI_API_KEY or not AZURE_OPENAI_ENDPOINT or not AZURE_OPENAI_DEPLOYMENT:
            print("ERROR: Missing Azure OpenAI environment variables (AZURE_OPENAI_API_KEY, AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_DEPLOYMENT)")
            sys.exit(1)
    else:
        if not OPENAI_API_KEY:
            print("ERROR: Missing OPENAI_API_KEY")
            sys.exit(1)

    # Get changed files
    files = get_pr_files()
    print(f"Found {len(files)} changed file(s)")

    # Count total changes
    total_lines = count_changed_lines(files)
    print(f"Total changed lines: {total_lines}")

    # Check if diff is too large
    if total_lines > MAX_DIFF_LINES:
        message = (
            f"⚠️ **AI Code Review**: Too many changes ({total_lines} lines) - unable to review.\n\n"
            f"This PR exceeds the {MAX_DIFF_LINES} line limit for automated review. "
            "Please request a manual review."
        )
        post_summary_comment(message)
        print(f"Skipping review: {total_lines} lines exceeds limit of {MAX_DIFF_LINES}")
        return

    # Filter to reviewable files
    files_to_review = []
    for file in files:
        filename = file["filename"]
        if should_review_file(filename) and file.get("patch"):
            files_to_review.append({
                "filename": filename,
                "patch": file["patch"]
            })
            print(f"  Will review: {filename}")
        else:
            print(f"  Skipping: {filename}")

    if not files_to_review:
        print("No reviewable files found")
        return

    # Build prompt and call OpenAI
    prompt = build_review_prompt(files_to_review)
    print(f"Sending {len(files_to_review)} file(s) to OpenAI for review...")

    comments = call_openai_for_review(prompt)
    print(f"Received {len(comments)} comment(s) from AI")

    # Post the review
    post_review_comments(comments, files)

    print("AI Code Review complete!")


if __name__ == "__main__":
    main()