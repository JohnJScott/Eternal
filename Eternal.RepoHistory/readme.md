# RepoHistory

## RepoHistory utility for Net10.0
Copyright Eternal Developments, LLC. All Rights Reserved.

## License

MIT

# Functionality
## RepoHistory

This utility does the following
* Finds the local Perforce workspace based on the local folder and P4PORT
 * P4PORT and P4USER may need to be set for it to find the correct Perforce server
* Iterates over the history of all files (which takes some time) to locate the repo they are in.
 * The repo in this case is the first clause of the depot path e.g. 'streams' would be the repo for this file //streams/mainline/GameTex/doxygen.config
* Dumps a summary of the found repos at the end

After waiting patiently, you should see a result something like:

21:00:00: Checked 15,459 files in 2254 directories in 2 minutes 12 seconds
21:00:00: Found 1 active repos
21:00:00:  .. Eternal

If only a single active repo is found, then all the files and meta data are local to that repo. If more than one is found, some of the file history is split between repos and you have to keep all the repos around to see the history.

# Explanation of the problem

When hosting Perforce on premises, disk space is effectively free. Branching from one repo to another is fine as there is no issue with keeping old repos around.
However, when being hosted in the cloud, disk space is the primary cost. 

[How to fix cross repo branching](https://portal.perforce.com/s/article/15336)

For my old company, doing this would reduce storage needs from 1.8TB ($180 pm) to 400GB ($40 pm). 

This tool does not do the repo fixup, it just tells you if it would be beneficial to do so.

# Changes 1st September 2026

Updated meta data.

# Notes

Full Doxygen documentation at https://eternaldevelopments.com/docs

I have no intention of maintaining backwards compatability, but will endeavor to mention if I make any changes.

This utility appeals to the most niche aspects of development, but if you feel like making a donation, please send DOGE to DFbEt36Qg2s2CVAdk5hZgRJfH8p1g6tW9i or buy a [#programmerlife t-shirt](https://www.bonfire.com/store/programmer-life/)

