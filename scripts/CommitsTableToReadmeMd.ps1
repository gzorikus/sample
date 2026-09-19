# [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 # if you have problems with emojis

$rebaseTodoPath = '.git/rebase-merge/git-rebase-todo';
if (-not (Test-Path $rebaseTodoPath)) { return; }

$branchName = [System.IO.File]::ReadAllLines($rebaseTodoPath) |
    ForEach-Object { if ($_ -match '^update-ref refs/heads/(?<name>.+)$') { $matches['name'] } } |
    Select-Object -First 1;
if (-not $branchName) { return; }

$currentSubject = '';
$currentSubjectExampleLinks = @();
$currentSubjectIndex = 0;
$diffTotal = 0;
$rows = git log --no-merges --no-renames `
    --date-order --reverse `
    --pretty=format:"%s" --numstat `
    --invert-grep --grep='^fixup! ' --grep='^amend! ' --grep='^squash! ' `
    -- '*.cs' 'examples/*/README.md' ':(exclude)examples/**/*.cs' ':(exclude)**/bin/*' ':(exclude)**/obj/*' |
    ForEach-Object {
        if (-not $_) { return; }
        if ($_ -match '^(?<subject>\w+\|.*)$') {
            $newSubject = $matches['subject'];
            if ($currentSubject -ne $newSubject) {
                $currentSubject = $newSubject;
                $currentSubjectExampleLinks = @();
                $currentSubjectIndex++;
            }
        } else {
            if (-not $currentSubject) { throw '-not $currentSubject'; }
            if ($_ -match '^\d+\s+\d+\s+(?<readmePath>examples/(?<exampleSolution>.+?)/README.md)') {
                $currentSubjectExampleLinks += [PSCustomObject]@{
                    ReadmePath = $matches['readmePath'];
                    Solution = $matches['exampleSolution'].Replace('-', [char]0x2011);
                };
            } elseif ($_ -match '^(?<added>\d+)\s+(?<deleted>\d+)\s+src/(?<module>YourCompany\..+?)/') {
                if (-not $currentSubjectExampleLinks) {
                    $currentSubjectExampleLinks += [PSCustomObject]@{
                        ReadmePath = 'examples/at-least-one-example-solution-per-commit/README.md';
                        Solution = 'TODO';
                    };
                }
                $commitLogsSectionLinkText = $currentSubject.Replace(' ', [char]0xA0).Replace('|', "|$([char]0x2060)");
                $commitLogsSectionHashTag = "#commit-$(($currentSubject -replace '\W', '-' -replace '(-)\1+', '$1').ToLower())";
                [PSCustomObject]@{
                    SubjectIndex = $currentSubjectIndex;
                    SubjectCommitLogsSectionLink = "[$commitLogsSectionLinkText]($commitLogsSectionHashTag)";
                    File = [PSCustomObject]@{
                        Module = $matches['module'];
                        Diff = [int]$matches['added'] - [int]$matches['deleted'];
                    };
                    OverExampleReadmeLinks = $currentSubjectExampleLinks |
                        ForEach-Object { "<kbd>$([char]0xA0)[$($_.Solution)]($($_.ReadmePath)$commitLogsSectionHashTag)$([char]0xA0)</kbd>" };
                };
            } else { throw "not matching: $_"; }
        }
    } |
    Group-Object SubjectIndex |
    Sort-Object  { [int]$_.Name } |
    ForEach-Object {
        $modules = $_.Group.File | Group-Object Module | Sort-Object Name | ForEach-Object {
            $module = $_.Group[0].Module;
            $diff = "{0:+0;-0;0}" -f ($_.Group.Diff | Measure-Object -Sum).Sum;
            $diffTotal += $diff;
            "<kbd>$module$([char]0xA0)($diff$([char]0xA0)lines)</kbd>"
        };
        "| $($_.Group[0].SubjectCommitLogsSectionLink.Replace('|', '\|')) " +
        "| $($_.Group[0].OverExampleReadmeLinks -join '<br>') " +
        "| $($modules -join '<br>') " +
        '|';
    };

$metrics = Get-ChildItem -Exclude examples |
    Get-ChildItem -Recurse -Filter '*.cs' -File |
    Where-Object { $_.FullName -match '^(?:(?!examples\\).)*src\\YourCompany(?:(?!\\obj\\|\\bin\\).)+$' } |
    ForEach-Object {
        $measured = [System.IO.File]::ReadAllText($_.FullName, [System.Text.Encoding]::UTF8) | Measure-Object -Line;
        [PSCustomObject]@{ Folder = $_.FullName; LineCount = $measured.Lines };
    } |
    Measure-Object -Property LineCount -Sum;

if ($metrics.Sum -ne $diffTotal) { throw "`$metrics.Sum -ne `$diffTotal"; }

$entryPointBranchBlock = @(
    "### Branch: $branchName",
    '',
    "> Files: $($metrics.Count) | Lines: $($metrics.Sum)  ",
    'Pros: why do you choose this entry point  ',
    'Cons: why it might be not enough for you',
    '',
    '| Log | Examples | Modules |',
    '|-|-|-|') + $rows + @(
    '',
    "<!-- ### Branch: $branchName END -->");

$content = [System.IO.File]::ReadAllText('README.md', [System.Text.Encoding]::UTF8);
$branchBlockMatches = [regex]::Matches($content, $('(?m)' +
    '(?<placeholderLine>^<!-- ### Branch: (?<name>.+?) (?<!END )-->\n)|' +
    '(?<replacingBranch>^(?:<!-- )?### Branch: (?<name>.+?)\n(?:.|\n)+?<!-- ### Branch: \k<name> END -->)'
));

$replaced = '';
foreach ($match in $branchBlockMatches) {
    if ($match.Groups['name'].Value -ne $branchName) { continue; }
    $placeholderLine = $match.Groups['placeholderLine'];
    $replacingBranch = $match.Groups['replacingBranch'];
    if ($placeholderLine.Success) {
        $replaced = $content.Remove($match.Index, $match.Length).Insert($match.Index, "`n$($entryPointBranchBlock -join "`n")`n");
    } else {
        if (-not $replacingBranch.Success) { throw "-not `$placeholderLine -and -not `$replacingBranch: $match"; }
        # The pros and cons are authored by hand, so an existing block keeps them
        # while the derived parts after them are regenerated.
        $existingBlock = $replacingBranch.Value;
        $authoredHead = $existingBlock.Substring(0, $existingBlock.IndexOf('| Log | Examples | Modules |'));
        $authoredHead = $authoredHead -replace '(?m)^> Files: .*$', "> Files: $($metrics.Count) | Lines: $($metrics.Sum)  ";
        $existingBlock = $authoredHead +
            ((@('| Log | Examples | Modules |', '|-|-|-|') + $rows) -join "`n") +
            "`n`n<!-- ### Branch: $branchName END -->";
        $replaced = $content.Remove($match.Index, $match.Length).Insert($match.Index, $existingBlock);
    }

    if ($replaced) {
        $content = $replaced;
    }
}

if (-not $replaced) { throw "no '$branchName' entry point branch block found in README.md"; }

[System.IO.File]::WriteAllText('README.md', $content);
