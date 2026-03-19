# Designing the Scalable System

## Step 1 - Small scale System
While designing any system, start with a lean system with an aspect to scale it in future for increasing load. Withi this in mind setup a single user system and scale it to millions of users. 
### Building Blocks
Major building blocks of any system would be
 - **Client** (Web/Mobile) *REQUEST*s data via web address.
 - Web address is resolved via **DNS** to IP address of the **server**.
 - Server processes the request internally of communicating with **database** and sends the *RESPONSE* back.
 - Client may recevie __JS,CSS, HTML, JSON__ etc. as the response.

### Choice of Database
In order to have pesistent store of the data of the system, choice of data store (Relational or NoSQL) is very crucial.
 1. **Relational databases** also called as relational database management system (RDBMS) or SQL database. represent and store data in tables and rows. You can perform join operations using SQL across different database tables. The most popular ones are __MySQL, Oracle database, PostgreSQL__, etc.
 > Relational databases are the best option because of storng and robust historical presence and relational data structure. However, if relational databases are not suitable for your specific use cases.
 2. **Non-Relational databases** are also called *NoSQL* databases. Some popular ones are __CouchDB, Neo4j, Cassandra, HBase, Amazon DynamoDB__, etc. These databases are grouped into four categories: 
     1. Key-value stores
     2. Graph stores
     3. Column stores
     4. Document stores. 
    Join operations are generally **not** supported in non-relational databases.
    Non-relational databases might be the right choice if:
     - Application requires super-low latency.
     - Data are unstructured, or you do not have any relational data.
     - Only need to serialization and deserialization data (JSON, XML, YAML, etc.).
     - Need to store a massive amount of data.

#### Choosing between Managed SQL vs Distributed SQL Databases

Choose managed Managed SQL (e.g.,PostgreSQL via AWS RDS or Aurora) for simpler workloads with moderate scale, complex SQL queries, and low-latency needs where vertical scaling or read replicas suffice, prioritizing operational ease over extreme distribution. Opt for distributed SQL databases (e.g., CockroachDB, YugabyteDB) when requiring horizontal scalability for massive data growth, global distribution, or high resilience without single points of failure, especially in geo-replicated government systems.

**Managed SQL Use Cases**
Ideal for single-region apps with predictable loads, leveraging primary-replica setups for high availability (99.99% uptime via Multi-AZ) and full PostgreSQL feature parity like advanced indexing or JSON support, but limited by manual sharding needs and write bottlenecks at high throughput. Best when team prefers managed backups, patching, and monitoring without distributed complexity; suits document metadata storage in gov collaboration where queries stay local.

**Distributed SQL Use Cases**
Select for write-heavy, multi-region scenarios demanding automatic sharding, linear scaling (add nodes for storage/throughput), and fault tolerance (no failover downtime via Raft consensus), trading minor latency for availability in CAP's AP model. Excels in gov apps with compliance-driven data locality or 10k+ concurrent users, supporting SQL compatibility while distributing tables across nodes.

**Comparison Table**

| Aspect              | Managed SQL                  | Distributed SQL                     |
|---------------------|-------------------------------------|-------------------------------------|
| Scalability        | Vertical + read replicas   | Horizontal (auto-sharding)  |
| Availability       | Multi-AZ failover (~60s)   | No-downtime resilience      |
| Latency            | Lower (single-node)        | Higher (network hops)      |
| Complexity         | Low ops overhead         | Higher but automated      |
| Best For           | Complex queries, moderate scale  | Global, high-write loads  |

---

## Step 2 - Making the System Available & Reliable
Ensuring that a web application can scale effectively as user traffic increases requires a combination of design strategies, architectural decisions, and the right set of technologies. Here are some key strategies and technologies that can help in building scalable web applications:

### Scaling the System
 To make the System relaible and available for increasing number of users and processes, we can either **Scale-up** (Vertically) o **Scale-out** (Horizontally)

#### **A. Horizontal Scaling:**
Horizontal scaling (also known as **scaling out**) involves adding more instances or machines to distribute the load. It is also known as **sharding**. It is a way to increase capacity by adding more resources in parallel by breaking larger DB into smaller managable parts called shards.
 - Each shard has **same schema** however the data may differ.
 - Choice of **Sharding Key** (Partition Key) is importaint while implementing sharding. It can be combination of one or more columns determing the __even__ distribution of data.

**Characteristics**:
- Involves adding more servers, nodes, or instances to the system.
- Often used in distributed systems where resources are spread across multiple machines or data centers.
- Works best for stateless applications where the workload can be easily divided and balanced across multiple instances.


**Use Cases for Horizontal Scaling**:
- **Web servers and APIs**: Adding more web server instances behind a load balancer to handle increasing web traffic.
- **Databases**: Scaling distributed databases like **Cassandra** or **Elasticsearch** by adding more nodes for data replication and distribution.
- **Microservices**: Scaling microservices independently to handle different types of workloads.

**Benefits**:
- High flexibility and scalability.
- Fault tolerance is improved since failure of one node doesn't take down the entire system.
- Allows for cost-effective scaling by using commodity hardware or cloud-based auto-scaling.

**Challenges**:
- Complexity in managing distributed systems (e.g., stateful systems, load balancing).
- Requires careful management of data consistency and partitioning in distributed systems.
- *Resharding data* when:
    - single shard cannot hold more data due to rapid growth
    - certain shards exausting faster due to uneven distribution of data. Sharding Function need to be updated to move the data around
    - **Celebrity/Hotspot Problem** - Excessive access to a specific shard could cause server overload. Further partitioning would be requred to solve the problem
 - **Joins & denormalization**: performing joins across databases(shards) is difficult. A common workaround is to denormalize the database so that queries can be performed in a single table.


#### **B. Vertical Scaling:**
Vertical scaling (also known as **scaling up**) involves upgrading the resources (e.g., CPU, RAM, disk) of a single server to increase its capacity.

**Characteristics**:
- Involves increasing the power of a single machine (e.g., upgrading the processor, adding more RAM, or increasing storage).
- Usually applied to monolithic or legacy systems that are not designed to scale horizontally.
- While simple to implement, it’s constrained by the physical or virtual limitations of the server hardware.

**Use Cases for Vertical Scaling**:
- **Legacy systems** that are not designed for horizontal scaling and require more resources to meet demand.
- **Databases** that may have complex relationships between data and require vertical scaling for performance reasons (e.g., increasing CPU power to handle complex queries).
- **High-performance computing (HPC)** applications that require a single high-power machine for data-intensive workloads.

**Benefits**:
- Simpler to manage since you only need to upgrade a single server instead of managing a distributed system.
- No need for complex load balancing or network communication between instances.

**Challenges**:
- Limited by the maximum resources a single machine can handle.
- Single points of failure: If the server fails, the whole system goes down.
- Less cost-effective than horizontal scaling for larger applications, as it may require expensive hardware.


#### **When to Use Each:**

1. **Horizontal Scaling**:
   - Use it when you expect **high growth** or **volatile traffic** and need the system to handle scaling efficiently.
   - Ideal for **distributed systems**, microservices, stateless services, and applications that can distribute workloads across multiple machines.
   - Best suited for systems that need to be fault-tolerant, highly available, and resilient to node failure.

2. **Vertical Scaling**:
   - Use it for **legacy systems** that are not designed to scale horizontally.
   - Good for applications with tightly coupled components or those requiring **high computational power** that benefit from large single-server resources (e.g., certain databases or high-performance computing tasks).
   - Can be useful for smaller systems with predictable load where it’s easier to manage a single powerful server.

---

### Load Balancing 
A load balancer evenly distributes incoming traffic among web servers that are defined in a load-balanced set.
```
                         ---------------- 
                         | Web Server 1 |
                         ----------------
                               |
---------    -------   -----------------
| Client |---| DNS |---| Load Balancer |
---------    -------   -----------------
                               |
                         ---------------- 
                         | Web Server 1 |
                         ----------------
```

 - Client interacts with public IP address of Load balancer while the web servers are inaccessible directly due to private IP address
 - Load Balancer(LB) intelligently routes the request to available web servers through an effecient algorithm
 - If either of the server goes down, LB routes the request to next available/healthy servers.
 - In case of increased loads, number of servers can be increased in the server pool to balance to load 

Get here a deeper dive on [load balancing](/01%20-%20system-design/concepts/load-balancing.md) topic.

---

### Database Optimization

* **Sharding:**
  * **How it works**: Split your database into smaller chunks (shards) across multiple servers based on a specific partition key (e.g., by user ID or region).
  * **Why it's effective**: Distributes the data and load across multiple databases, improving performance and scalability.
  * **Technology**:**MySQL Cluster**,**Cassandra**,**MongoDB Sharding**.
* **Read Replicas:**
  * **How it works**: Use**read replicas**to handle read-heavy workloads by replicating the data across multiple instances and directing read traffic to replicas.
  * **Why it's effective**: Relieves the primary database from handling read queries, improving read performance.
  * **Technology**:**Amazon RDS Read Replicas**,**MySQL Replication**,**PostgreSQL Replication**.
* **Database Indexing and Optimization:**
  * **How it works**: Ensure that database queries are optimized and indexes are applied on frequently queried fields to speed up search operations.
  * **Why it's effective**: Reduces query time and allows the database to handle more queries efficiently.
  * **Technology**:**MySQL**/**PostgreSQL**indexing,**MongoDB**indexing.

While the databases are being optimized to scale for increasing data load, it is very essential to **[maintain consistency across distributed data sets](/01%20-%20system-design//concepts/data-consistency.md)**.

---

### Caching

* **Application Caching:**
  * **How it works**: Cache frequently accessed data in memory to reduce the load on backend systems and databases (e.g., caching results of common queries).
  * **Why it's effective**: Reduces response times and the load on backend systems, improving overall application performance.
  * **Technology**:**Redis**,**Memcached**,**Varnish**,**Cloudflare CDN**.
* **Database Caching:**
  * **How it works**: Use caching mechanisms to store results of frequently queried database calls, preventing the need for repeated database hits.
  * **Why it's effective**: Offloads work from the database and reduces latency.
  * **Technology**:**Redis**,**Amazon DynamoDB Accelerator (DAX)**,**Memcached**.
* **Content Delivery Networks (CDN):**
  * **How it works**: Serve static assets (images, CSS, JavaScript) via distributed networks of servers closer to the user’s geographical location.
  * **Why it's effective**: Minimizes latency by serving content from edge locations, reducing server load.
  * **Technology**:**Cloudflare**,**AWS CloudFront**,**Akamai**,**Fastly**.

As we plan to use caching in our system design, it is essential to get a deeper insight on different [caching strategies](/01%20-%20system-design/concepts/caching-strategy.md) and various [cache invalidation techniques](/01%20-%20system-design/concepts/cache-invalidation-techniques.md).

---

### **Microservices Architecture:**
Before diving into building of microservices, understanding the core [differences between monolith and microservices](/01%20-%20system-design/concepts/soa-vs-monolith-vs-microservice.md) is very much essential from scalability and cost aspect.

* **Service Decomposition:**
  * **How it works**: Break the monolithic application into small, independently deployable services that communicate via APIs (usually REST or gRPC).
  * **Why it's effective**: Allows each microservice to scale independently, reducing the overall complexity of scaling.
  * **Technology**:**Spring Boot**,**Docker**,**Kubernetes**,**AWS Lambda**(for serverless microservices).
* **Containerization:**
  * **How it works**: Use**containers**(e.g., Docker) to package application components in a lightweight, portable way, making it easier to scale and deploy.
  * **Why it's effective**: Containers allow you to scale components independently and consistently across various environments.
  * **Technology**:**Docker**,**Kubernetes**,**Docker Swarm**.
* **Orchestration with Kubernetes:**
  * **How it works**: Use Kubernetes to orchestrate and manage microservices and containerized applications, automatically handling scaling, deployment, and fault tolerance.
  * **Why it's effective**: Kubernetes simplifies the deployment and scaling process, and enables automatic recovery in case of failures.
  * **Technology**:**Kubernetes**,**OpenShift**,**Amazon EKS**.

> Understanding [pros and cons](/01%20-%20system-design/concepts/microservices-benfits-and-challenges.md) of microservices.


### **Event-Driven Architecture:**

* **Message Queues and Event Streams:**
  * **How it works**: Use message queues (e.g.,**RabbitMQ**,**Apache Kafka**) to decouple services and allow for asynchronous communication between services.
  * **Why it's effective**: Helps with scaling by allowing services to process messages at different rates and reduces direct dependencies between services.
  * **Technology**:**Kafka**,**RabbitMQ**,**Amazon SQS**,**Google Cloud Pub/Sub**.
* **Event Sourcing:**
  * **How it works**: Use an event-driven model where the state of a system is determined by the events (or changes) that have occurred over time.
  * **Why it's effective**: Makes it easier to scale services since state changes are stored as events and can be reprocessed or replayed if needed.
  * **Technology**:**Kafka**,**EventStore**,**Axon Framework**.

---

### **Statelessness and Session Management:**

* **Stateless Services:**
  * **How it works**: Design services to be stateless, meaning they don’t store session information between requests. Instead, each request is self-contained, with all necessary data included.
  * **Why it's effective**: Statelessness makes it easier to scale since any instance of a service can handle any request without needing to access session data stored on a particular server.
  * **Technology**:**JWT**(JSON Web Tokens),**OAuth**(for session management).
* **Distributed Session Management:**
  * **How it works**: For scenarios where state needs to be retained, use distributed session storage (e.g.,**Redis**or**Memcached**) to share session data across instances.
  * **Why it's effective**: Enables consistent user experiences even as application instances scale horizontally.
  * **Technology**:**Redis**,**Memcached**,**Amazon DynamoDB**(for session persistence).

---

### **Asynchronous Processing:**

* **Task Queues and Background Jobs:**
  * **How it works**: For long-running or resource-intensive tasks (such as email sending, data processing, or image generation), offload these tasks to a background job queue.
  * **Why it's effective**: Allows the web application to continue handling user requests without being blocked by heavy tasks.
  * **Technology**:**Celery**,**RabbitMQ**,**AWS Lambda**,**Sidekiq**.

---

### **Distributed System Design**
In a typical distributed system, few core aspects thant need to be considered are:
 - Weightage between **[Consistency vs Availablity](/01%20-%20system-design//concepts/cap-theorem.md)** of services while designing the system
 - Designing for fault tolerance & resiliance considering distributed transactions using concepts like [CQRS](/01%20-%20system-design/concepts/cqrs.md), [Circuit Breaker](/01%20-%20system-design/concepts/circuit-breaker-pattern.md) and [SAGA Patterns](/01%20-%20system-design/concepts/saga-protocol.md)

---

### **Optimizing for High Availability:**

* **Multi-Region Deployments:**
  * **How it works**: Deploy your application across multiple data centers or cloud regions to ensure availability and low-latency access.
  * **Why it's effective**: Provides redundancy and disaster recovery, ensuring that traffic is routed to the nearest region and minimizing downtime in case of failures.
  * **Technology**:**AWS Global Accelerator**,**Google Cloud Load Balancer**,**Azure Traffic Manager**.
* **Redundancy & Failover:**
  * **How it works**: Implement redundant systems, such as database replication, web server clusters, and failover strategies to ensure that there’s always a backup available.
  * **Why it's effective**: Helps the application maintain uptime even in the event of hardware or software failure.
  * **Technology**:**AWS RDS Multi-AZ**,**Azure Availability Zones**,**Database Replication**.

---

### **Optimizing Code and Architecture:**

* **Code Optimization:**
  * **How it works**: Optimize code by reducing unnecessary complexity, improving algorithms, and using efficient data structures.
  * **Why it's effective**: Faster, more efficient code can handle more requests per second, improving overall scalability.
  * **Technology**: Performance profiling tools like**New Relic**,**Datadog**,**Prometheus**, and**Grafana**for performance monitoring and bottleneck identification.
* **API Rate Limiting and Throttling:**
  * **How it works**: Protect APIs by limiting the number of requests a user can make within a certain time frame.
  * **Why it's effective**: Prevents your application from being overwhelmed by excessive traffic from abusive or accidental overload.
  * **Technology**:**API Gateway**,**Nginx**,**Rate Limiting libraries**.

---

The strategies and technologies used to ensure web application scalability depend on the specific needs and architecture of the application. By combining horizontal scaling, caching, load balancing, database optimizations, event-driven architectures, and cloud-native technologies like Kubernetes, you can ensure that your web application scales effectively as user traffic increases. Additionally, technologies like microservices, containerization, and automated scaling ensure that your system is resilient, adaptable, and can handle growth with minimal disruption.
