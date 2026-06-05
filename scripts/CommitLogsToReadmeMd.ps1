# [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 # if you have problems with emojis

$content = [System.IO.File]::ReadAllText('README.md', [System.Text.Encoding]::UTF8);
$allRelativePaths = Get-ChildItem -Exclude examples |
    Get-ChildItem -Recurse -Filter '*.cs' -File |
    Where-Object { $_.FullName -match '^(?:(?!examples\\).)*src\\YourCompany(?:(?!\\obj\\|\\bin\\).)+$' } |
    ForEach-Object { $_.FullName.Substring((Get-Location).Path.Length + 1).Replace('\', '/') };

$lastReplacedSubject = "";
(git log --no-merges --no-renames `
    --date-order --reverse `
    --pretty=format:'-COMMITSEPARATOR-%s-BODYSEPARATOR-%b-STATSSEPARATOR-' `
    --stat=333 --stat-name-width=177 --stat-graph-width=7 `
    --invert-grep --grep='^fixup! ' --grep='^amend! ' --grep='^squash! ' `
    -- '*.cs' ':(exclude)examples' ':(exclude)**/bin/*' ':(exclude)**/obj/*') -join "`n" -split '-COMMITSEPARATOR-' |
    Where-Object { $_ } |
    ForEach-Object {
        if ($_ -notmatch '^(?<subject>.+)-BODYSEPARATOR-(?<body>(?:.|\n)*)-STATSSEPARATOR-\n(?<stats>(?:.|\n)+)$') { throw "`$logs_ -notmatch: $_"; }
        $subject = $matches['subject']; $body = $matches['body'].Trim("`n"); $stats = $matches['stats'] -split "`n" | Where-Object { $_ };

        $statsTable = $stats | Select-Object -SkipLast 1 | ForEach-Object {
            if ($_ -notmatch '^ (?<subpath>.*)\s+\| (?<diff>.*)$') { throw "`$stats_ -notmatch: $_"; }
            $subpath = $matches['subpath'].Trim(); $diff = $matches['diff'];
            $relativePath = if (Test-Path $subpath) { $subpath } else {
                $ending = $subpath.TrimStart('.').TrimStart('/');
                $allRelativePaths | Where-Object { $_.EndsWith($ending) };
            };

            $diffLines, $diffGraph = $diff -split '(?<=\d) (?=\+|-)';
            $directory = [System.IO.Path]::GetDirectoryName($subpath).Replace('\', '/').Replace(' ', [char]0xA0);
            $fileName = [System.IO.Path]::GetFileName($subpath);

            if ($relativePath.Count -gt 1) { throw "`$relativePath.Count -gt 1: $_"; }
            if ($relativePath.Count -eq 0) {
                $directory = "<sub><sub><del>$directory/</del></sub></sub><br>"
            } else {
                $directory = "<sub><sub>$directory/</sub></sub><br>";
            };

            $row = '<kbd>'
            $row += $diffGraph.Trim().PadLeft(8, [char]0xA0).Replace('-', [char]0x2011);
            $row += [char]0xA0;
            $row += $diffLines.Trim().PadRight(4, [char]0xA0);
            $row += '|';
            $row += [char]0x2060;
            $row += [char]0xA0;
            if ($relativePath.Count -eq 0) {
                $row += "$($fileName.PadRight(111, [char]0xA0))";
            } else {
                $row += "[$($fileName.PadRight(111, [char]0xA0))]($relativePath)";
            };
            $row += '</kbd><br>';

            [PSCustomObject]@{
                DiffLines = [int]$diffLines;
                DiffGraph = $diffGraph;
                Directory = $directory;
                FileName = $fileName;
                Row = $row;
            };
        } | Group-Object Directory | ForEach-Object {
            [PSCustomObject]@{
                Directory = $_.Name;
                Files = $_.Group;
                DiffLines = ($_.Group.DiffLines | Measure-Object -Sum).Sum;
            }
        } | Sort-Object DiffLines -Desc | ForEach-Object {
            $_.Directory;
            $_.Files |
                Sort-Object -Property @{Expression="DiffLines"; Descending=$true;}, DiffGraph, RelativePath |
                Select-Object -ExpandProperty Row;
        };

        $commit = (
            , ("### Commit: $subject") +
            , ('<table><tbody><tr><td>') +
            , ($stats[-1].Trim() + "<br>`n" + ($statsTable -join "`n")) +
            $(if ($body) { , $body } else { @() }) +
            , ('</td></tr></tbody></table>') +
            , ("<!-- ### Commit: $subject END -->")
        ) -join "`n`n";

        $matches = [regex]::Matches($content, $('(?m)' + 
            '(?<placeholderLine>^<!-- ### Commit: (?<subject>.+?) (?<!END )-->\n)|' +
            '(?<replacingCommit>^(?:<!-- )?### Commit: (?<subject>.+?)\n(?:.|\n)+<!-- ### Commit: \k<subject> END -->)'
        ));

        $replaced = '';

        foreach ($match in $matches) {
            if ($match.Groups['subject'].Value -ne $subject) { continue; }
            $placeholderLine = $match.Groups['placeholderLine'];
            $replacingCommit = $match.Groups['replacingCommit'];
            if ($placeholderLine.Success) {
                $replaced = $content.Remove($match.Index, $match.Length).Insert($match.Index, "`n$commit`n");
            } else {
                if (-not $replacingCommit.Success) { throw "-not `$placeholderLine -and -not `$replacingCommit: $match"; }
                $replaced = $content.Remove($match.Index, $match.Length).Insert($match.Index, $commit);
            }

            if ($replaced) {
                $content = $replaced;
            }
        }

        if (-not $replaced) {
            if ($lastReplacedSubject) {
                $ending = "<!-- ### Commit: $lastReplacedSubject END -->";
                $content = $content.Replace($ending, "$ending`n`n$commit");
            } else {
                $content = "$content`n$commit`n";
            }
        }

        $lastReplacedSubject = $subject;
    };

[System.IO.File]::WriteAllText('README.md', $content);
