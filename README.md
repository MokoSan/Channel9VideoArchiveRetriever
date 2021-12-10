# Channel 9 Video Archive Retriever

This repository contains the code that retrieves the now lost Channel 9 videos via [Wayback Machine](https://archive.org/web/). Not a perfect collection but gets you a majority of the videos.

## Videos

- [11/2021 - 4/2017](https://github.com/MokoSan/Channel9VideoArchiveRetriever/blob/main/Channel9VideoArchiveRetriever/output/Pages/Urls_1Thru300.md)
- [4/2017 - 11/2014](https://github.com/MokoSan/Channel9VideoArchiveRetriever/blob/main/Channel9VideoArchiveRetriever/output/Pages/Urls_301Thru1000.md)
- [11/2014 - 4/2009](https://github.com/MokoSan/Channel9VideoArchiveRetriever/blob/main/Channel9VideoArchiveRetriever/output/Pages/Urls_1001Thru2000.md)

## Topic Specific

[PerfView](https://github.com/MokoSan/Channel9VideoArchiveRetriever/blob/main/Channel9VideoArchiveRetriever/output/Mined/PerfView.md)

## Troubleshooting

- If Wayback machine doesn't seem to archive the videos, remove the Wayback Machine Prefix from the URL. For example:
http://web.archive.org//web/20200821010837/https://sec.ch9.ms/ch9/89e5/bab1e741-d3d4-4b06-a10b-fc1e1f4389e5/Video7_high.mp4 -> https://sec.ch9.ms/ch9/89e5/bab1e741-d3d4-4b06-a10b-fc1e1f4389e5/Video7_high.mp4 

## Logic

1. Using the known number of pages and the Base URL: ``https://channel9.msdn.com/Browse/AllContent``, make calls to the Wayback Machine API: ``https://channel9.msdn.com/Browse/AllContent`` to get the archived URL for each of the pages.
2. With the archived URL of each of the pages that contain the video tiles, iterate through all the video tiles and grab the relevant information needed.
3. Persist the URLs and persist the video info into files that can be read in later on as JSON files.
4. Convert the JSON files into a Markdown table.