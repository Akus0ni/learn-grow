# System Design Learning Plan in four weeks

## Week 1: System Design Fundamentals and Core Concepts

### System Design Fundamentals (Day 1-3)
- [ ] Introduction to system design
- [ ] Study concepts like 
     - Scalability
     - Fault Tolerance
     - High Availablity
- [ ] Learn about reliability and availability principles
     - [ ] Load balancing
     - [ ] Caching
     - [ ] Replication
- [ ] Research common architectural patterns
- [ ] Understand message queues and event-driven architecture
- [ ] Learning Activites
    - [ ] Study theory (read chapters in "[System Design Interview](https://github.com/shams-imran/books/blob/master/System%20Design/system-design-interview-an-insiders-guidepdf_compress.pdf)" or online resources)
     - [ ] Watch YouTube tutorials or courses (e.g., [Gaurav Sen’s System Design Series](https://www.youtube.com/playlist?list=PLMCXHnjXnTnvo6alSjVkgxV-VH6EPyvoX))
- [ ] Hands-on
     - [ ] Draw simple system diagrams and justify design choices (e.g., designing a URL shortener, a messaging system)

### Core CS Concepts (Day 4-5)
- [ ] Review data structures implementation and use cases
- [ ] Study OS concepts - processes, threads, memory management
- [ ] Deep dive into networking - TCP/IP, HTTP protocols
- [ ] Review database concepts 
     - [ ] ACID
     - [ ] CAP theorem
     - [ ] Sharding
     - [ ] Partitioning
- [ ] Learning Activites
     -  [ ] Read "[Designing Data-Intensive Applications](https://github.com/NirmalSilwal/system-design-resources/blob/master/Books/Designing%20Data%20Intensive%20Applications%20-%20Martin%20Kleppmann.pdf)" by Martin Kleppmann (focus on chapters on consistency and data modeling)
     -  [ ] Case studies: study systems like Netflix, Amazon, etc.
- [ ] Hands-on
     - [ ] Design a large-scale distributed system, e.g., designing an e-commerce platform with millions of users.

## Week 2: Strengthening Computer Science Fundamentals and AWS Core Services

### Computer Science Fundamentals & Advanced Algorithms  (Day 1-5)
- [ ] Data structures (arrays, linked lists, stacks, queues, heaps, trees, graphs)
- [ ] Algorithms (sorting, searching, dynamic programming, greedy algorithms)
- [ ] Graph algorithms (DFS, BFS, shortest path algorithms)
- [ ] Divide and conquer, dynamic programming patterns
- [ ] Big-O analysis and optimization strategies
- [ ] Learning Activities
     - [ ] Study theory (use textbooks like "Introduction to Algorithms" by Cormen or online resources)
     - [ ] Practice problems on **LeetCode** or **HackerRank** (start with easy and medium-level problems).
- [ ] Hands-on: Solve medium and hard-level problems on **LeetCode** with a focus on time complexity analysis.


## Week 3: AWS Core Services

### AWS & Cloud  (Day 1-3)
- [ ] Complete AWS Solutions Architect Associate basics
- [ ] Study EC2, IAM, VPC, and networking in AWS
- [ ] Learn AWS storage solutions - S3, EBS, EFS
- [ ] Understand AWS databases - RDS, DynamoDB
- [ ] Study AWS serverless - Lambda, API Gateway
- [ ] CloudFormation and Elastic Beanstalk for deployment
- [ ] Learning Activities
     - [ ] Set up and launch basic services like EC2 instances and S3 buckets.
     - [ ] Complete AWS free-tier projects (e.g., build a serverless application using Lambda and API Gateway).
     - [ ] Study best practices for scalability and security in AWS.
- [ ] Hands-on:
     - [ ] Deploy a simple web application on AWS EC2.
     - [ ] Set up a serverless architecture with AWS Lambda and DynamoDB.

### AWS Architecture & Monitoring (Day 4-6)
- [ ] Designing fault-tolerant, scalable systems on AWS
- [ ] Using CloudWatch, CloudTrail, and IAM for monitoring and security
- [ ] Learning Activities
     - [ ] Watch advanced AWS architecture tutorials or courses.
     - [ ] Study the AWS Well-Architected Framework.
- [ ] Hands-on: Implement logging and monitoring using AWS CloudWatch for a sample application.

## Week 4: Microservices and Full-Stack Projects

### Microservices  (Day 1-2)
- [ ] Learn microservices architecture patterns
- [ ] Study service discovery and API gateway patterns (service discovery, load balancing, API Gateway) 
- [ ] Learn Docker fundamentals and container concepts
- [ ] Study Kubernetes basics and orchestration
- [ ] Understand distributed system challenges
- [ ] Learning Activities
    - [ ] Read "[Building Microservices](https://github.com/themockingjester/Books/blob/master/Building%20Microservices%2C%202nd%20Edition.pdf)" by Sam Newman (Chapters on design principles).
     - [ ] Watch introductory tutorials on microservices architecture.
     - [ ] Study Kubernetes fundamentals and microservices orchestration.
- [ ] Hands-on: Set up Docker containers for microservices and deploy using Docker Compose.

### Microservices Advanced Concepts with Kubernetes and CI/CD(Day 3-5)
- [ ] Inter-service communication (synchronous vs. asynchronous, REST, gRPC)
- [ ] Event-driven architecture, message queues (Kafka, RabbitMQ)
- [ ] Introduction to Kubernetes and orchestration
- [ ] Setting up continuous integration/continuous deployment (CI/CD) pipelines
- [ ] Learning Activities
     - [ ] Study patterns for inter-service communication and best practices for microservices.
     - [ ] Review online resources on asynchronous messaging and event-driven architecture.
     - [ ] Learn how to implement a basic CI/CD pipeline with GitHub Actions, Jenkins, or GitLab CI.
- [ ] Hands-on: 
     - [ ] Build a simple microservices-based app using Node.js or Spring Boot (e.g., an e-commerce order management system).
    - [ ] Deploy a microservices application using Kubernetes and set up CI/CD pipelines for automatic deployment.

## **Ongoing Tasks (Every Week)**
- **Mock Interviews**: Participate in mock system design or technical interviews with peers or mentors to simulate real interview conditions and get feedback.
- **Review & Adjust**: At the end of each week, assess what you've learned and adjust the following week’s plan based on your strengths and weaknesses.
- **Track Progress**: Keep track of daily progress through a learning journal or a simple tracker (Google Sheets, Notion).

## 📚 Daily Schedule Template
- 09:00-11:00: Theory and Concepts
- 11:00-11:15: Break
- 11:15-13:00: Hands-on Practice
- 13:00-14:00: Lunch Break
- 14:00-16:00: Project Work
- 16:00-16:15: Break
- 16:15-18:00: Implementation and Documentation

## 🔄 Review Process
- Daily: Move tasks between columns
- Weekly: Review completed items and adjust next week's plan
- Bi-weekly: Mock interview to assess progress