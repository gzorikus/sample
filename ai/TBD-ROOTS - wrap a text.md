# Wrap a text

**Open this when** a text travels: into a commit body, a document,  
another commit's quote, a review.

## Orders

1. **Count the raw source line: 72 characters.** Do not count the  
   rendered text: backticks, asterisks and every markup symbol count.
2. **Fill to 70, then append two spaces.** The two spaces make a hard  
   break in Markdown: they close a line, they do not pad a counter.
3. **Put the two spaces on every filled line of a block and on no  
   other line.** Never on a block's last line, never on the subject,  
   never on a heading, a table row or a fence.
4. **Fill by words.** Take the next word while the line, a space and  
   that word stay within 70; end the line only when the next word  
   would cross.
5. **Join the block and fill it again.** Never re-wrap line by line: a  
   fill of a fill drifts, and two runs give two texts.
6. **Split words on the ASCII space and the tab only.** Keep a  
   no-break space inside its word.
7. **Wrap nothing in the verbatim set**: fences, table rows, ATX  
   headings, the marker of a list item, and the two spaces standing  
   before a blank line, a heading, a list item, a table row or a  
   fence. **Fill a list item like a block**: the marker opens the  
   first line, every continuation aligns under the text after it, and  
   the fill is the measure minus the marker's width.
8. **Never break a token.** Keep a URL, a marker literal, a code  
   string whole; over 72 it is reported, never cut. **Call a line over  
   72 that is one token `token`, not a defect**: the check names it  
   and passes. **Move a command that must stand into a fence**: a  
   fenced block is verbatim and owes no measure.
9. **Never break inside a code literal.** Text between two backticks  
   is one token: move the whole span to the next line, or let it stand  
   over 72.
10. **Never start a continuation line with a list marker.** A dash or  
    a number with a dot at the start of a wrapped line reads as a new  
    bullet on the next pass.
11. **Use one measure; exempt per surface.** A body, a document, a  
    table: the same 70 plus two. A surface that needs another number  
    says so once, where it is used.
12. **Strip a carriage return before counting**, or every line reports  
    zero trailing spaces.
13. **Charge the two spaces outside the measure**, or a hole opens at  
    the end of nearly every line.
14. **Count the backticks in every line: an odd number is a split  
    literal.** No reader sees it otherwise, so this count is the  
    witness.
15. **Write a tool as a port, on first use, kept outside the tree.**  
    Check it against this file; where it disagrees, this file wins.
16. **Give this law two map entries: `wrap-check` (the witness) and  
    `wrap-fix` (the fill).** Mark both `settled`: the check accepts a  
    clean pass over the text, the fix accepts a second run that  
    changes nothing. The consumer of both is every run that writes a  
    text.
17. **Commit nothing without a clean check.**
18. **Let this file wrap itself.** A rule that cannot be kept in its  
    own text is not kept at all.
