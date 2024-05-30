This document spells out the url weighting (priority) system.


The urls in the urlFrontier are being prioritized based on the following factors:

1. **Relevancy Score**: Each url in the urlFrontier is associated with a relevancy score. Higher the relevancy score, higher is the priority of the url. Relevancy score is calculated based on the relevancy of the web page content with respect to the search query.

2. **Crawl Budget**: Crawl budget of an url is the total number of times a web crawler will visit the url. Urls with higher crawl budget are prioritized as they offer more valuable information.

3. **Domain Authority**: Domain authority is a search engine ranking score that predicts how likely a website is to rank in search engine result pages. Urls from domains with higher authority are given higher priority as they are considered more trustworthy and likely to hold more valuable information.

4. **Link Structure**: The urls that are deeper into the site structure or linked from many pages may get higher priority. The logic being, if many links are pointing to a particular url, that url may hold important information.

These factors can be independently weighted and combined into a final priority score. The urls are then queued in the order of their final priority scores. 

This url prioritization strategy ensures that the most relevant and authoritative resources are crawled first, optimizing the efficiency and efficacy of the web crawling process.