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
OPENAI_MODEL = os.environ.get("OPENAI_MODEL", "gpt-5-mini")

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
    ".Tests/",
    "Tests.cs",
    "Test.cs",
    ".github/scripts/",
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


def parse_diff_lines(patch):
    """Parse a unified diff patch to get the set of valid new-side line numbers.

    Returns a set of line numbers that appear on the RIGHT (new) side of the
    diff.  These are the only lines GitHub will accept for inline comments when
    using the ``line`` + ``side`` API parameters.
    """
    if not patch:
        return set()
    valid_lines = set()
    current_new_line = 0
    for line in patch.split("\n"):
        if line.startswith("@@"):
            match = re.search(r"\+(\d+)", line)
            if match:
                current_new_line = int(match.group(1)) - 1
        elif line.startswith("-"):
            pass  # Deleted line — no new-side line number
        elif line.startswith("+"):
            current_new_line += 1
            valid_lines.add(current_new_line)
        else:
            current_new_line += 1
            valid_lines.add(current_new_line)
    return valid_lines


def annotate_patch_with_line_numbers(patch):
    """Convert a raw diff patch into line-numbered content for the LLM.

    Strips diff markers (+/-/@@) and labels each added/context line with its
    actual file line number so the AI can reference exact lines.
    """
    if not patch:
        return ""
    lines = []
    current_new_line = 0
    for line in patch.split("\n"):
        if line.startswith("@@"):
            match = re.search(r"\+(\d+)", line)
            if match:
                current_new_line = int(match.group(1)) - 1
        elif line.startswith("-"):
            # Deleted lines - show for context but don't assign a line number
            lines.append(f"     (deleted) {line[1:]}")
        elif line.startswith("+"):
            current_new_line += 1
            lines.append(f"L{current_new_line:>4}: {line[1:]}")
        else:
            current_new_line += 1
            lines.append(f"L{current_new_line:>4}: {line}")
    return "\n".join(lines)


def count_changed_lines(files):
    """Count total added/modified lines across all files."""
    total = 0
    for file in files:
        total += file.get("additions", 0) + file.get("deletions", 0)
    return total



def build_review_prompt(files_with_diffs):
    """Build the prompt for OpenAI with the diffs to review."""
    prompt = """You are an expert C#/.NET code reviewer. Review the following code changes for ACTUAL BUGS ONLY.

## Philosophy: Pragmatic, Not Pedantic

Your default stance is to APPROVE. Most PRs are fine. If the code works and is not a clear anti-pattern, say nothing. Working code that ships is better than theoretically perfect code that doesn't.

- If a solution works but a slightly better alternative exists, that is NOT worth flagging.
- If code is functional and not actively harmful, leave it alone.
- Only speak up when something will genuinely break, lose data, or create a security hole.
- When in doubt, stay silent. An empty comments array is a perfectly good review.

## IMPORTANT: Be Extremely Selective

You MUST only flag code that has a HIGH probability of causing a runtime bug, security vulnerability, or data loss.

DO NOT flag:
- Style preferences or formatting
- Missing error handling that would just cause the program to crash (that's often acceptable)
- Generic suggestions like "add logging" or "add validation"
- Code that retrieves environment variables (this is standard practice)
- Code that works correctly but could theoretically be "improved"
- Performance suggestions unless there's an actual O(n²) or worse issue in a hot path
- Suggestions to add try/catch blocks around code that already has error handling
- Test code — test secrets, test data, and test patterns are fine. Do not review test files for production concerns.
- Code in non-C# files (Python, JavaScript, YAML, Markdown, etc.)

## What to Actually Flag (BUGS ONLY)

Only flag these specific issues:
- **BANNED: AutoMapper** - Flag ANY use of AutoMapper. Suggest manual mapping instead.
- **`async void` methods** — Flag any `async void` method that is NOT an event handler. `async void` swallows exceptions and cannot be awaited. It should be `async Task` instead.
- Null dereference that WILL throw (not might throw)
- SQL/NoSQL injection with string interpolation in queries
- Regex injection — user input passed directly to `new Regex()` without escaping
- Hardcoded secrets/passwords/API keys in PRODUCTION source code (not test code)
- Resource leaks: FileStream/SqlConnection without using/dispose
- Infinite loops or obvious logic errors
- Thread safety issues with shared mutable state
- Division by zero when the divisor comes from user input without validation

## HARD RULES — NEVER VIOLATE

1. **Do NOT flag `.Result`, `.Wait()`, `.GetAwaiter().GetResult()`, or `ReadToEndAsync().Result`.** These synchronous-over-async patterns are accepted in this codebase. The only async issue worth flagging is `async void` (see above).
2. **Cache key formatting and validation is NOT a bug.** Do NOT flag cache key construction.
3. **If ObjectId.TryParse is already being used, the input IS validated.** Do NOT suggest additional validation for the same value.
4. **Do NOT review test files for security concerns.** Hardcoded keys, secrets, and dummy data in test projects are expected and correct.
5. **Do NOT review non-C# files.** Python scripts, YAML configs, JavaScript files, and Markdown are out of scope.

## Examples of Code to FLAG

```csharp
// FLAG: async void (swallows exceptions, cannot be awaited)
public async void ProcessData() { // should be async Task
    await DoWorkAsync();
}

// FLAG: SQL injection
var query = $"SELECT * FROM Users WHERE Id = {userId}";

// FLAG: Regex injection
var regex = new Regex(userInput); // user input not escaped

// FLAG: Hardcoded secret in production code
var apiKey = "sk-1234567890abcdef";

// FLAG: Resource leak
var stream = new FileStream(path, FileMode.Open);
// ... no using, no dispose

// FLAG: AutoMapper (BANNED)
var dto = _mapper.Map<UserDto>(user);

// FLAG: Division by zero from user input
var pages = totalCount / pageSize; // pageSize could be 0
```

## Examples of Code to IGNORE (do NOT flag)

```csharp
// IGNORE: .Result, .Wait(), .GetAwaiter().GetResult() — accepted in this codebase
CreateIndexesAsync().GetAwaiter().GetResult();
var body = reader.ReadToEndAsync().Result;
var data = GetDataAsync().Result;

// IGNORE: Standard env var retrieval
var config = Environment.GetEnvironmentVariable("CONFIG")

// IGNORE: FirstOrDefault with null check
var item = items.FirstOrDefault();
if (item == null) return NotFound();

// IGNORE: Proper async
var result = await GetDataAsync();

// IGNORE: Using statement
using var stream = new FileStream(path, FileMode.Open);

// IGNORE: Test code with hardcoded values
var testKey = "test-secret-key-12345"; // in a test file — fine

// IGNORE: Code that works even if an alternative exists
var items = list.Where(x => x.IsActive).ToList(); // works fine, don't suggest alternatives
```

## Response Rules

1. Your DEFAULT response should be: {"comments": []} — most PRs are fine
2. Only flag ACTUAL BUGS with HIGH confidence
3. Do not make generic suggestions or "nice to have" improvements
4. Do not flag test files or non-C# files
5. **CRITICAL: Each comment's "file" field MUST be the EXACT file path from the "### FILE:" header where the code appears. Do NOT attribute a finding in one file to a different file.**

## Code to Review

Each file below is inside its own clearly marked section.
Pay close attention to which file each code block belongs to.

"""
    for file_info in files_with_diffs:
        annotated = annotate_patch_with_line_numbers(file_info['patch'])
        prompt += f"\n{'='*60}\n### FILE: {file_info['filename']}\n{'='*60}\n```\n{annotated}\n```\n"

    prompt += """

Each line above is prefixed with its real file line number (e.g. L  28).
Use EXACTLY that number in the "line" field of your response.
Use EXACTLY the file path from the "### FILE:" header for the "file" field.

Respond with ONLY a JSON object in this exact format, no other text:
{
  "comments": [
    {
      "file": "exact/path/from/FILE/header.cs",
      "line": 42,
      "body": "Description of the bug"
    }
  ]
}

Each comment MUST have exactly these three fields: "file", "line", "body".
If there are no issues, return: {"comments": []}"""
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
        max_completion_tokens=16000,
        reasoning_effort="medium",
        response_format={"type": "json_object"}
    )

    content = response.choices[0].message.content

    # Log the raw LLM response so it's visible in the Action output
    print("=" * 60)
    print("RAW LLM RESPONSE:")
    print("=" * 60)
    print(content)
    print("=" * 60)

    # Parse the JSON response
    try:
        result = json.loads(content)
        # Handle both {"comments": [...]} and [...] formats
        if isinstance(result, list):
            return content, result
        elif isinstance(result, dict) and "comments" in result:
            return content, result["comments"]
        else:
            return content, []
    except json.JSONDecodeError:
        print(f"Failed to parse OpenAI response as JSON: {content}")
        return content, []



def post_review_comments(comments, files_data, raw_response):
    """Post inline review comments to the PR using GitHub's review API."""
    if not comments:
        print("No issues found - code looks good!")
        summary = "✅ **AI Code Review**: No issues found. The changes look good!\n\n"
        summary += "<details>\n<summary>Raw AI Response</summary>\n\n```json\n"
        summary += raw_response
        summary += "\n```\n</details>"
        post_summary_comment(summary)
        return

    # Build sets of valid new-side line numbers for each file in the diff
    valid_line_sets = {}
    for file in files_data:
        filename = file["filename"]
        valid_line_sets[filename] = parse_diff_lines(file.get("patch", ""))

    # Prepare review comments using line + side (not position)
    review_comments = []
    unmapped_comments = []
    for comment in comments:
        filename = comment.get("file", "")
        line = comment.get("line", 0)
        body = comment.get("body") or comment.get("comment", "")

        if not filename or not line or not body:
            continue

        # Verify the line exists in the diff for this file
        valid_lines = valid_line_sets.get(filename, set())

        if line in valid_lines:
            review_comments.append({
                "path": filename,
                "line": line,
                "side": "RIGHT",
                "body": f"{body}"
            })
        else:
            print(f"WARNING: Line {line} in {filename} is not in the diff (valid: {sorted(valid_lines)[:20]}...) - comment will be included in summary instead")
            unmapped_comments.append(comment)

    if review_comments:
        # Post as a PR review with inline comments
        url = f"{GITHUB_API_BASE}/repos/{REPO_FULL_NAME}/pulls/{PR_NUMBER}/reviews"
        headers = {
            "Authorization": f"token {GITHUB_TOKEN}",
            "Accept": "application/vnd.github.v3+json",
        }

        item_word = "item" if len(review_comments) == 1 else "items"
        review_body = f"Found {len(review_comments)} {item_word} to discuss."

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
            post_summary_comment(format_fallback_summary(comments, raw_response))
    else:
        post_summary_comment(format_fallback_summary(comments, raw_response))

    # If some comments were posted inline but others couldn't be mapped, append those as a follow-up
    if review_comments and unmapped_comments:
        fallback = format_fallback_summary(unmapped_comments, raw_response)
        post_summary_comment(fallback)


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


def format_fallback_summary(comments, raw_response=None):
    """Format comments as a summary when inline comments aren't possible."""
    lines = ["**Code Review**\n"]

    for comment in comments:
        filename = comment.get("file", "")
        line = comment.get("line", 0)
        body = comment.get("body") or comment.get("comment", "")

        # Skip comments with missing required fields
        if not filename or not line or not body:
            continue

        lines.append(f"**{filename}** (line {line}):\n> {body}\n")

    # If all comments were filtered out, return a success message
    if len(lines) == 1:
        result = "✅ **AI Code Review**: No issues found. The changes look good!"
    else:
        result = "\n".join(lines)

    # Always append the raw AI response in a collapsible section
    if raw_response:
        result += "\n\n<details>\n<summary>Raw AI Response</summary>\n\n```json\n"
        result += raw_response
        result += "\n```\n</details>"

    return result



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

    raw_response, comments = call_openai_for_review(prompt)
    print(f"Received {len(comments)} comment(s) from AI")

    # Post the review
    post_review_comments(comments, files, raw_response)

    print("AI Code Review complete!")


if __name__ == "__main__":
    main()