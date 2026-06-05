# [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 # if you have problems with emojis

$commitsByUniqueCommitterDate = [System.Collections.Generic.Dictionary[int, object]]::new();
$labelsReferenced = @{};

$commitsByRebaseTimeUniqueSubject = @{};
$commitsByRebaseTimeUniqueHash = @{};
$orderedAuthors = '🏠', '🧱', '💾', '📦';
$orderedAuthorsLowerCase = 'domainman', 'facademan', 'persistman', 'streamman';
git log --date-order --reverse --format='%D|%ct|%h|%p|%s' --decorate-refs="refs/heads/*" | ForEach-Object {
    $branch, $commiterDateUnixTime, $abbreviatedHash, $abbreviatedParentHashes, $subject = $_ -split '\|', 5;
    $cmdToAdd = ''; $useForLabel = ''; $commitsWithoutIteration = $null; $parentHash = '';

    if ($subject -eq 'init') {
        $commitsByRebaseTimeUniqueHash.Add($abbreviatedHash, @{ Label = "$abbreviatedHash # always reset onto init for safety`n"; })
        return;
    }

    $iteration = if ($subject -match '^(?<iteration>\S*?)\|(?<author>.*?):') {
        if ($matches['iteration'] -eq 'N') { 777 } else { [int]$matches['iteration'] }
    } else { $null };
    $author = if ($iteration -is [int]) { ($matches['author'] -split ' ')[0] } else { $null };

    $parentHashes = $abbreviatedParentHashes -split ' ';
    $commitsWithoutIteration = $parentHashes |
        ForEach-Object { @($commitsByRebaseTimeUniqueHash[$_].CommitsWithoutIteration).Where({ $_ -ne $null }) };

    if ($parentHashes.Count -gt 1) {
        $parentHash = $parentHashes[0];
        $cmdToAdd = "merge -C $abbreviatedHash";
        $parent = $commitsByRebaseTimeUniqueHash[$parentHash];
        $commitsWithoutIteration = @($parent.CommitsWithoutIteration).Where({ $_ -ne $null });
        $parentHashes | Select-Object -Skip 1 | ForEach-Object {
            $mergedParent = $commitsByRebaseTimeUniqueHash[$_];
            $parentLabel = $mergedParent.Label;
            $labelsReferenced[$parentLabel] = $true;
            $cmdToAdd += ' ' + $parentLabel;
        }
        if ($iteration -isnot [int]) { $commitsWithoutIteration += , $abbreviatedHash; }
        $useForLabel = $subject;
    } elseif ($subject -match '^(?<type>fixup|squash|amend)! (?<subject>.+)') {
        $editedCommit = $commitsByRebaseTimeUniqueSubject[$matches['subject']];
        $editedCommit.Cmd = switch ($matches['type']) {
            'fixup' { "$($editedCommit.Cmd)`nfixup $abbreviatedHash" }
            'squash' { "$($editedCommit.Cmd)`nsquash $abbreviatedHash" }
            'amend' { "$($editedCommit.Cmd)`nfixup -C $abbreviatedHash" }
        };
        $firstBodyLine = git show --pretty=format:"%b" -s $abbreviatedHash | Select-String -Pattern '\S' | Select-Object -First 1;
        if ($firstBodyLine) { $editedCommit.Cmd = "$($editedCommit.Cmd) # $firstBodyLine"; }
        $commitsByRebaseTimeUniqueHash.Add($abbreviatedHash, @{
            ParentHash = $abbreviatedParentHashes;
            Label = $commitsByRebaseTimeUniqueHash[$abbreviatedParentHashes].Label;
            CommitsWithoutIteration = $commitsByRebaseTimeUniqueHash[$abbreviatedParentHashes].CommitsWithoutIteration;
        });

        if ($branch) {
            $fixupHashToTakeParentRevFrom = $abbreviatedHash;
            do {
                $fixupHashToTakeParentRevFrom = git rev-parse --short "$fixupHashToTakeParentRevFrom^";
                $parentCommit = $commitsByRebaseTimeUniqueHash[$fixupHashToTakeParentRevFrom];
                if (-not $parentCommit) { throw "-not `$parentCommit: $subject"; }
                if ($parentCommit.Cmd) {
                    $parentCommit.Branch += $branch;
                    $fixupHashToTakeParentRevFrom = $null;
                }
            } while ($fixupHashToTakeParentRevFrom);
        }
    } else {
        if ($iteration -isnot [int] -or -not $author) { throw "`$iteration -isnot [int] -or -not `$author: $subject"; }
        $cmdToAdd = "pick $abbreviatedHash";
        $parentHash = $abbreviatedParentHashes;
        $useForLabel = $subject.Replace($author, $orderedAuthorsLowerCase[$orderedAuthors.IndexOf($author)]);
    }

    if ($iteration -is [int] -and $commitsWithoutIteration) {
        $commitsWithoutIteration | ForEach-Object {
            if ($_ -is [string] -and $_ -ne $abbreviatedHash) { throw "`$_ -is [string] -and `$_ -ne `$abbreviatedHash: $subject" }
            if ($_ -is [string]) { return; }
            if ($_.Iteration -is [int]) { throw "`$_.Iteration -is [int]: $subject"; }
            $_.Iteration = $iteration;
            $_.Author = $orderedAuthors.IndexOf($author);
        }
        $commitsWithoutIteration = $null;
    }

    if ($cmdToAdd) {
        while (-not $commitsByRebaseTimeUniqueHash[$parentHash].Cmd) {
            $nextParentHash = $commitsByRebaseTimeUniqueHash[$parentHash].ParentHash;
            if ($nextParentHash) { $parentHash = $nextParentHash; } else { break; }
        }

        $commit = [PSCustomObject]@{
            ParentHash = $parentHash;
            AbbreviatedHash = $abbreviatedHash;
            Branch = $branch;
            Iteration = $iteration;
            Author = $orderedAuthors.IndexOf($author);
            CommitterDate = [int]$commiterDateUnixTime;
            Cmd = "# $subject`n$cmdToAdd";
            Label = ($useForLabel -replace '\W', '-').ToLower();
            CommitsWithoutIteration = $null;
        };

        try { $commitsByUniqueCommitterDate.Add([int]$commiterDateUnixTime, $commit); }
        catch {
            $commit.Cmd = "# Skipped: $($commit.Cmd)";
            $commit.Cmd = $commit.Cmd -replace '\n', "`n# ";
            $conflictingCommit = $commitsByUniqueCommitterDate[[int]$commiterDateUnixTime];
            $conflictingCommit.Cmd = $conflictingCommit.Cmd -replace '^(?!# Skipped: )', "# Skipped: ";
            $conflictingCommit.Cmd = $conflictingCommit.Cmd -replace '\n(?!#)', "`n# ";
            $conflictingCommit.Cmd = "$($conflictingCommit.Cmd)`n# label $($conflictingCommit.Label)`n# and`n$($commit.Cmd)`n# label $($commit.Label)";
            $conflictingCommit.Cmd = "$($conflictingCommit.Cmd)`n# were previously rebased without breaking in between what resulted in CommitterDate duplicating. Skipping these commits! Please edit manually...";
            $commit.Cmd = "`n$($commit.Cmd)`nbreak`n# it won't appear in the todo file because of CommitterDate duplicating`n`n# label $($commit.Label)`n";
            Write-Error $conflictingCommit.Cmd;
        }

        $commitsByRebaseTimeUniqueSubject.Add($subject, $commit);
        $commitsByRebaseTimeUniqueHash.Add($abbreviatedHash, $commit);

        $commit.CommitsWithoutIteration = $commitsWithoutIteration |
            ForEach-Object { if ($_ -is [string]) { $commitsByRebaseTimeUniqueHash[$_] } else { $_ } };
    }
};

$expectedParentHash = '';
$commitsByUniqueCommitterDate.Values | Sort-Object -Property Iteration, Author, CommitterDate | ForEach-Object {
    if ($_.Branch -ne 'public' -and $_.Iteration -isnot [int]) { throw "`$_.Branch -ne 'public' -and `$_.Iteration -isnot [int]: $subject"; }

    if ($_.ParentHash -ne $expectedParentHash) {
        $resetOntoLabel = $commitsByRebaseTimeUniqueHash[$_.ParentHash].Label;
        $_.Cmd = "reset $resetOntoLabel`n$($_.Cmd)";
        $labelsReferenced[$resetOntoLabel] = $true;
    }

    $expectedParentHash = $_.AbbreviatedHash;
};

$content = [System.IO.File]::ReadAllText('bonus-last-git-rebase-todo', [System.Text.Encoding]::UTF8);
$content = $content -replace '(?ms)(^#[^\n]+\n)(?:\n{5}#).*$', '$1';

$lastIteration = -1;
$commitsByUniqueCommitterDate.Values | Sort-Object -Property Iteration, Author, CommitterDate | ForEach-Object {
    if ($_.Branch -eq 'public') {
        $content += "`n`n`n`n`n# Public`n";
    } else {
        if ($lastIteration -ne $_.Iteration) {
            $lastIteration = $_.Iteration;
            $iteration = if ($_.Iteration -eq 777) { 'N' } else { $_.Iteration };
            $content += "`n`n`n`n`n# Startup Iteration $iteration. Don't forget to update or revert the iteration's goal.`n";
        }
    }

    $notSkipped = $_.Cmd -match '^(?!\n# Skipped: )';
    $_.Cmd = "`n$($_.Cmd)`n" +
        "exec git commit --amend --no-edit --date=now`n" +
        "exec git config --get alias.interactiverebasebeforebreak && git interactiverebasebeforebreak || true`n" +
        "break`n" +
        "# don't forget to amend now with updated stats in README.md`n";
    if ($_.Branch) { $_.Cmd = "$($_.Cmd)update-ref refs/heads/$($_.Branch)`n"; }
    if ($notSkipped -and $labelsReferenced[$_.Label]) { $_.Cmd = "$($_.Cmd)`nlabel $($_.Label)`n"; }
    $content += $_.Cmd;
}

[System.IO.File]::WriteAllText('bonus-last-git-rebase-todo', $content);
