**CAP Theorem**:
   - The **CAP Theorem** (also known as Brewer’s Theorem) states that a distributed system can guarantee at most two out of the following three properties:
     - **Consistency**: Every read returns the most recent write. All nodes in the system see the same data at the same time.
     - **Availability**: Every request (read or write) gets a response, even if some nodes are unavailable. The system remains operational.
     - **Partition Tolerance**: The system continues to operate even if network partitions occur between nodes or data centers, meaning that nodes can't communicate with each other.

   **Impact on Design Decisions**:
   - The **CAP Theorem** indicates that we can’t have all three guarantees simultaneously. Instead, we have to choose two depending on the use case:
     - **CP (Consistency + Partition Tolerance)**: The system ensures consistency even during network partitions, but it may become unavailable in certain failure scenarios. **Example**: **HBase**, **Zookeeper**.
     - **AP (Availability + Partition Tolerance)**: The system remains available and can tolerate partitions, but it may return inconsistent data. **Example**: **Cassandra**, **DynamoDB**.
     - **CA (Consistency + Availability)**: The system guarantees consistency and availability but cannot tolerate network partitions. This scenario is rare in distributed systems because partition tolerance is almost always required. **Example**: It’s typically not feasible in large-scale distributed systems.
  
   **Design Implications**:
   - The decision to prioritize **Consistency** over **Availability** or vice versa has direct implications on the user experience and system behavior. For example, a banking system would prioritize **Consistency** (ensuring all users see the same balance), while an e-commerce website might favor **Availability** (ensuring users can always place an order, even if some inventory data is temporarily inconsistent).

   - **CAP Theorem** plays a central role in understanding the trade-offs when designing a distributed system. The choice of **Consistency**, **Availability**, and **Partition Tolerance** influences how you design your system and which guarantees you can make.