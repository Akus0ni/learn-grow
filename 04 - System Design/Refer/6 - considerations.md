## Considerations for Cache
 - In case of frequent Reads than writes
 - Not a pesistant store hence, store the important data into persistent data store
 - Implement expiration policy. Expiration Date should not be too short or too long.
 - Maintain synchronization between data-store and cache. If not done, inconsitency may occur as the data-store modification and cache have different transactions.
 - Provision multiple caching servers or over provision the required memory by certain percent to mitigate failures
 - Apply Eviction policies like LRU or LFU to avoid throttling of cache memory

## Considerations for CDN
  - Cost: CDNs charge for data transfers in and out.
  - Expiration Time should be appropriate especially for sensitive data
  - Cope for CDN failures. In case of any CDN outage, client should request the content from server
  - Invalidating cache by
     - invoking cache invalidation API provided by CDN vendor
     - use object versioning. it can be a parameter in URL

> **Autoscaling** means adding or removing web servers automatically based on the traffic load. Stateless web servers can be easily auto-scaled by adding or removing servers based on traffic load

## Data Centers
As our website grows rapidly and attracts a significant number of users internationally, having multiple data centers are crucial to improve availability and provide a better user experience across wider geographical areas.

 1. Deploy servers in different geographical regions
 2. In normal operations, users are geoDNS-routed to closest datacenter.
 3. in case of failure/outage, traffic is routed to healthy datacenter.

> **GeoDNS** is a DNS service that allows domain names to be resolved to IP addresses based on the location of a user.

### Considerations
 - Effective tools (like GeoDNS) for **traffic redirection** to correct datacenters
 - **Data Synchronization** as users in different regions may use different local databases. In failover cases user may be routed to a datacenter where data may not be available. __Data replication across multiple region__ can be one of the strategy.
 - Deligent testing and deployment strategies
 