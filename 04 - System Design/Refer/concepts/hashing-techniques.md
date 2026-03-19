Hashing is a process of transforming data into a fixed-size value, often used in data structures (like hash tables), cryptography, and data integrity checks. There are various hashing techniques, each serving different purposes. Here’s an overview of some of the most common hashing techniques:

### 1. **Division Method (Modular Hashing)**
- **How it works**: The key is divided by a prime number (or a number of your choice), and the remainder is used as the hash value.
  - Hash Value = Key % Prime Number
- **Use Cases**: This method is often used for creating hash values in hash tables, especially when dealing with numerical keys.
- **Pros**: Simple and easy to implement.
- **Cons**: If the divisor is not chosen properly (i.e., it's not a prime number), it can lead to clustering of hash values (collisions).

### 2. **Multiplicative Hashing**
- **How it works**: Instead of using division, a constant multiplier is used. The key is multiplied by a constant, and the fractional part of the result is used as the hash value.
  - Hash Value = floor(m * Key % 1) * Table Size, where `m` is a constant multiplier.
- **Use Cases**: Common in applications like hash tables for distributing keys evenly across a hash table.
- **Pros**: Often results in better distribution of hash values.
- **Cons**: Requires careful selection of the constant multiplier.

### 3. **Cryptographic Hashing (e.g., MD5, SHA)**
- **How it works**: Cryptographic hashing algorithms take an input (message) and produce a fixed-size string of characters, typically a digest.
  - Examples include **MD5**, **SHA-1**, **SHA-256**, **SHA-512**, and **SHA-3**.
- **Use Cases**: Used in data integrity, digital signatures, password hashing, and message authentication codes.
- **Pros**: Difficult to reverse-engineer (pre-image resistance), and small changes in the input result in drastic changes to the output (avalanche effect).
- **Cons**: Some algorithms (e.g., MD5, SHA-1) have been found vulnerable to collision attacks (where two different inputs produce the same hash).

### 4. **Linear Probing Hashing**
- **How it works**: This is a method used for collision resolution in hash tables. When a collision occurs (i.e., the computed hash value is already taken), the next available slot is checked (typically in a linear manner, checking the next slot in the array).
  - If the desired slot is full, move to the next slot in sequence until an empty slot is found.
- **Use Cases**: Common in hash tables when resolving collisions.
- **Pros**: Simple and efficient for small datasets.
- **Cons**: Can lead to clustering, where groups of consecutive slots are filled, reducing performance.

### 5. **Chaining (Separate Chaining)**
- **How it works**: In this technique, each slot in the hash table contains a linked list (or another data structure). When a collision occurs, the new item is simply added to the linked list at the corresponding slot.
  - Hash Value = Key % Table Size → Use the corresponding linked list at that index.
- **Use Cases**: Also used in hash tables for collision resolution.
- **Pros**: Easy to implement and handles collisions effectively.
- **Cons**: The performance can degrade when many elements are hashed to the same index (long linked lists), requiring extra memory.

### 6. **Double Hashing**
- **How it works**: This is another collision resolution technique used in open addressing schemes. If a collision occurs, a second hash function is used to determine how far to probe for the next available slot.
  - If `H1(Key)` is the first hash, then `H2(Key)` is used as a step size for probing.
  - New Hash Value = (H1(Key) + i * H2(Key)) % Table Size, where `i` is the number of attempts made.
- **Use Cases**: Used in hash tables where a good distribution and minimal clustering are required.
- **Pros**: Reduces clustering problems compared to linear probing.
- **Cons**: Slightly more complex due to the need for a second hash function.

### 7. **FNV-1 and FNV-1a (Fowler–Noll–Vo) Hashing**
- **How it works**: This is a fast and efficient hash function used for creating hash values, especially for strings. The FNV-1 and FNV-1a algorithms are based on prime numbers and XOR operations.
  - The algorithm uses a constant prime multiplier and applies XOR to each byte of the input data.
- **Use Cases**: Often used in file integrity checks, hash tables, and checksums.
- **Pros**: Fast and provides good distribution for strings.
- **Cons**: Not suitable for cryptographic use cases due to its predictability.

### 8. **CityHash**
- **How it works**: A family of hash functions developed by Google. It is designed to quickly hash small strings or integers and is used in applications like distributed systems.
- **Use Cases**: Used in systems that require very fast hashing, such as databases, file systems, and distributed systems.
- **Pros**: Highly optimized and fast.
- **Cons**: Not cryptographically secure, so not suitable for security-critical applications.

### 9. **MurmurHash**
- **How it works**: A non-cryptographic hash function known for its speed and good distribution properties. It is widely used in various applications.
- **Use Cases**: Often used in big data systems, databases, and distributed systems.
- **Pros**: Very fast and produces good hash values for general use cases.
- **Cons**: Not cryptographically secure, so not suitable for security purposes.

### 10. **BuzHash**
- **How it works**: This is a fast, non-cryptographic hash function that uses a combination of multiplication and bit shifting to generate a hash value.
- **Use Cases**: Used in applications where speed is critical, and security is not a concern.
- **Pros**: Efficient and produces reasonably good hash values.
- **Cons**: Like other non-cryptographic hashes, it's not secure for use in cryptographic contexts.

---

### When to Use Each Hashing Technique

- **Cryptographic Hashing (MD5, SHA, etc.)**: When you need security, such as for digital signatures, password hashing, or verifying the integrity of data.
- **Division Method/Multiplicative Hashing**: For simple applications like hash tables where the data is mainly numerical or requires evenly distributed keys.
- **Linear Probing and Double Hashing**: For resolving hash collisions in hash tables.
- **Chaining**: When you need to handle collisions without needing extra probing or if you expect a lot of collisions.
- **FNV-1, MurmurHash, CityHash**: When you need fast, non-secure hashes for applications like distributed systems, databases, or quick checksums.

In general, choose the hashing technique based on the nature of your data (e.g., strings, numbers, or security-related data), the need for speed, and whether or not you require cryptographic security.