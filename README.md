# StudyCat

Usage:
First, create a directory for your book under Books.  Then copy StudyCat.bat into the new directory.  Open a command window in the new book directory.

Commands:

cmd: StudyCat new
Makes a new book
  Options:
    -t, --title:        [str, opt] sets book title
    -a, --authors:      [str, opt] sets book authors
    -y, --year:         [str, opt] sets book publication year
    -p, --publisher:    [str, opt] sets book publisher
    -c, --chapters:     [int, opt] sets the number of chapters in the book
  Example:
    StudyCat new -t Calculus -a Rogawski -y 2016 -c 16

cmd: StudyCat make
Makes the section data for the cards, given a book description.  Note that you must manually edit the JSON file for the book first!
  Options: none

cmd: StudyCat list
Lists information about the book, its chapters and sections.
  Options: none

cmd: StudyCat add
Adds a new card to a section in a book.  You should have run 'new' and 'make' first to generate the necessary JSON data.
  Options:
    -s, --section:  [str, req] Section into which to add the new card
    -t, --type:     [str, req] Card type (values: Problem, Definition, Theorem, Lemma, Note, Algorithm)
    --text:         [str, req] Card text, e.g. the name of the Theorem
  Example:
    StudyCat add -s 3.1 -t Definition -text Indefinite Integral

cmd: StudyCat study
Runs a study session for the current book, given the specified options
  Options:
    -s, --section:  [str, req] Section(s) to be studied.  Use single numeral, i.e. 4, for a whole chapter.  Use one or more string of the form 3.2 for individual sections.  Use 3.S1 for a special section.
    --serial:       [bool, opt] Presents cards in serial order (no randomization)
    --simulate:     [bool, opt] Simulates a study session, but does not record session results.
    -t, --typee:    [str, opt] Specifies which card types to study (defaults to all types)
    -n, --number    [int, opt] Number of items to study in the session.  0 means no limit.
    -o, --odds      [bool, opt] Only presents odd-numbered cards
    -e, --evens     [bool, opt] Only presents even-numbered cards
