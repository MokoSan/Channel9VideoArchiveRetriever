# Channel 9 Video Archive Retriever

This repository contains the code that retrieves the now lost Channel 9 videos.

## Logic

1. Using the known number of pages and the Base URL: ``https://channel9.msdn.com/Browse/AllContent``, make calls to the Wayback Machine API: ``https://channel9.msdn.com/Browse/AllContent`` to get the archived URL for each of the pages.
2. With the archived URL of each of the pages that contain the video tiles, iterate through all the video tiles and grab the relevant information needed.
3. Persist the URLs and persist the video info into files that can be read in later on as JSON files.
4. Convert the JSON files into a Markdown table.