# AWS SES — Simple Email Service

## 1. Core Concepts

### What is SES?
Amazon SES (Simple Email Service) is a **cloud-based email sending and receiving service** built on AWS's reliable, scalable infrastructure. It's used for:
- **Transactional emails**: order confirmations, password resets, account notifications
- **Marketing emails**: newsletters, promotional campaigns
- **Bulk email**: high-volume sending at low cost
- **Email receiving**: receive inbound email, process via Lambda or store in S3

### Key Concepts
| Term | Description |
|------|-------------|
| **Identity** | Email address or domain verified for sending |
| **Sandbox mode** | Default state — can only send to verified addresses (limits testing) |
| **Production access** | Request to AWS to send to any recipient |
| **Sending quota** | Default 200 emails/day in sandbox; up to millions/day in production |
| **Sending rate** | Max emails per second (default 1/s in sandbox; up to 14,000/s in production) |
| **Reputation** | Sender reputation based on bounce/complaint rates |
| **DKIM** | DomainKeys Identified Mail — cryptographic signature on outbound email |
| **SPF** | Sender Policy Framework — specifies authorized sending IP ranges |
| **DMARC** | Domain-based Message Authentication — policy for handling unauthenticated mail |
| **Configuration Set** | A group of rules for tracking and managing email events |
| **Suppression List** | Addresses that should never receive email (bounced/complained) |

### Email Authentication (Deliverability)
Proper authentication is essential to avoid the spam folder:
```
DKIM:   Adds cryptographic signature to each email header
        → Receiving server verifies using public key in DNS TXT record

SPF:    DNS TXT record lists authorized IP ranges for your domain
        → Receiving server checks if SES IP is in your SPF record

DMARC:  Policy in DNS: if DKIM/SPF fails → quarantine or reject
        → Provides visibility reports on who is sending on your behalf

SES + Custom Domain = DKIM + SPF auto-configured when you verify domain
```

### Configuration Sets
Configuration Sets enable tracking of email events:
| Event | Description |
|-------|-------------|
| **Send** | Email accepted by SES for delivery |
| **Delivery** | Successfully delivered to recipient's mail server |
| **Bounce** | Delivery failed (permanent or temporary) |
| **Complaint** | Recipient marked email as spam |
| **Open** | Recipient opened the email (requires tracking pixel) |
| **Click** | Recipient clicked a link (requires link wrapping) |
| **Rendering Failure** | Template rendering failed |

Events can be sent to **CloudWatch, Kinesis Firehose, SNS, EventBridge, or Pinpoint** for analytics and automated handling.

### Bounce and Complaint Handling
**Critical**: SES suspends your account if:
- **Bounce rate** > 5% (hard bounces permanently fail delivery)
- **Complaint rate** > 0.1% (recipients click "This is spam")

Best practices:
1. Enable the **SES account-level suppression list** — automatically suppresses hard-bounced and complained addresses
2. Subscribe to bounce/complaint SNS notifications → Lambda to remove bad addresses from your mailing list
3. Use **double opt-in** for marketing email to reduce complaints
4. **Never** send to purchased lists

### SES Receiving
SES can receive email for your domain and route it to:
- **S3**: store raw email
- **Lambda**: trigger processing function
- **SNS**: publish notification
- **WorkMail**: AWS email client

Receiving rules evaluate recipients and can filter, block, or process emails.

### SES vs SES v2
**SES v2** is the current version with:
- Improved API (single endpoint for all regions)
- **Virtual Deliverability Manager (VDM)**: AI-driven deliverability recommendations
- **Subscription Management**: built-in unsubscribe link handling
- **Multi-region endpoints**
- Better contact list management

### Pricing
| Usage | Cost |
|-------|------|
| Emails sent (from EC2/Lambda) | $0.10 per 1,000 emails |
| Emails sent (from other sources) | $0.10 per 1,000 emails |
| Attachments | $0.12 per GB |
| Email receiving | $0.10 per 1,000 emails + S3 storage |
| Dedicated IP leasing | $24.95/month per IP |

SES is dramatically cheaper than transactional email providers (SendGrid, Mailgun) at scale.

---

## 2. Interview Questions & Answers

### Q1. What is AWS SES and when would you use it over a third-party email provider?
SES is AWS's managed email sending service. Use SES when:
- You're already on AWS and want native integration (Lambda triggers, IAM auth, no extra API keys)
- You send high volumes of email (SES is cheaper at scale — $0.10/1,000 vs SendGrid's $~0.50+/1,000)
- You need email receiving integrated into your AWS architecture (S3 + Lambda)

Use a third-party provider (SendGrid, Mailgun) when:
- You need a rich marketing campaign UI out-of-the-box
- You need fast setup without domain verification complexity
- Your team doesn't want to manage deliverability and suppression lists themselves

---

### Q2. What is the SES sandbox and how do you get out of it?
**Sandbox mode** is the default state for new SES accounts. In sandbox:
- Can only send to **verified email addresses or domains**
- Sending limit: 200 emails/day, 1 email/second

To request **production access**:
1. Open an AWS Support case requesting SES production access
2. Describe your sending use case (transactional, marketing), expected volume, and how you handle bounces/complaints
3. AWS reviews and typically approves within 24–48 hours

---

### Q3. How do DKIM, SPF, and DMARC improve email deliverability?
- **SPF**: A DNS TXT record listing IP addresses authorized to send email for your domain. Receiving servers check if the SES IP is in your SPF record. Prevents spoofing.
- **DKIM**: SES signs each email with a private key; the receiving server verifies the signature using the public key published in DNS. Proves the email was sent by SES and wasn't tampered with.
- **DMARC**: A DNS policy record that tells receiving servers what to do if SPF or DKIM fails (`none`, `quarantine`, or `reject`). Also provides aggregate reports on who is sending email on your behalf.

SES automatically configures DKIM when you verify a domain. Add SPF and DMARC records manually in Route 53.

---

### Q4. What are bounce and complaint rates and why do they matter?
**Hard bounce rate** measures the percentage of emails that permanently fail delivery (invalid address, domain doesn't exist). SES suspends accounts with bounce rate > **5%**.  
**Complaint rate** measures the percentage of recipients who mark your email as spam. SES suspends accounts with complaint rate > **0.1%**.  

These thresholds are strict because sending to bad addresses or spamming harms AWS's IP reputation, which affects all SES customers. Use suppression lists and confirmed opt-in to stay well below these thresholds.

---

### Q5. How do you handle bounces and complaints automatically?
1. Enable a **Configuration Set** with an SNS event destination for `Bounce` and `Complaint` events
2. Subscribe a **Lambda function** to the SNS topic
3. Lambda parses the event:
   - `Bounce` (permanent): add the address to your suppression list / unsubscribe from mailing list
   - `Complaint`: immediately unsubscribe the address
4. Also enable the **SES account-level suppression list** — SES automatically suppresses hard bounces and complaints at the account level (you don't need to manage this list manually)

---

### Q6. What is an SES Configuration Set and what can you do with it?
A Configuration Set is a named group of rules applied when sending emails tagged with that set. With a Configuration Set you can:
- **Track events**: Send events (open, click, bounce, complaint, delivery) to CloudWatch, Firehose, SNS, or Pinpoint
- **Enforce TLS**: Require TLS for delivery
- **Add custom headers**: Include tracking headers automatically
- **Set IP pool**: Route emails through a specific dedicated IP or shared pool

Attach a Configuration Set to every send call for full observability.

---

### Q7. What is a dedicated IP address in SES and when should you use it?
By default, SES sends from a **shared IP pool** — your reputation is partially shared with other SES customers. A **dedicated IP** is exclusively yours.

Use dedicated IPs when:
- You send a very high volume of email (> 100,000/day per IP)
- You need maximum control over your sender reputation
- You're running marketing campaigns where reputation is critical
- You want IP **warmup control** (gradually increasing volume)

Cost: $24.95/month per IP. Requires **IP warmup** — gradually increasing volume from the IP to build reputation with ISPs.

---

### Q8. How do you send templated emails with SES?
Use **SES Email Templates**:
1. Create a template with Handlebars syntax:
```json
{
  "Template": {
    "TemplateName": "OrderConfirmation",
    "SubjectPart": "Your order {{orderId}} is confirmed!",
    "HtmlPart": "<h1>Hello {{name}}</h1><p>Order total: ${{total}}</p>",
    "TextPart": "Hello {{name}}, Order total: ${{total}}"
  }
}
```
2. Send with `SendTemplatedEmail` API, providing template data as JSON:
```json
{"name": "Alice", "orderId": "ORD-123", "total": "99.99"}
```
3. For bulk sending to multiple recipients with different data, use `SendBulkTemplatedEmail` — send to thousands of recipients in one API call.

---

### Q9. How does SES integrate with Lambda for automated transactional email?
```python
import boto3
ses = boto3.client('ses', region_name='us-east-1')

def send_order_confirmation(order):
    ses.send_templated_email(
        Source='orders@shopwave.com',
        Destination={'ToAddresses': [order['customerEmail']]},
        Template='OrderConfirmation',
        TemplateData=json.dumps({
            'name': order['customerName'],
            'orderId': order['id'],
            'total': str(order['total'])
        }),
        ConfigurationSetName='shopwave-transactional'
    )
```
Lambda is triggered by SQS (order confirmation queue) or EventBridge (Order Placed event). IAM role grants `ses:SendTemplatedEmail` permission.

---

### Q10. What is the SES Virtual Deliverability Manager (VDM)?
**VDM** (part of SES v2) is an AI-powered deliverability advisor that:
- Analyzes your sending patterns and identifies deliverability issues
- Recommends actions to improve inbox placement (e.g., "your open rate is dropping for Gmail users — check your subject lines")
- Provides **deliverability dashboard**: open rates, click rates, inbox placement by ISP
- **ISP-level diagnostics**: shows which ISPs are delivering to inbox vs spam

Enable VDM for any production email program — it reduces the guesswork in deliverability troubleshooting.

---

### Q11. How do you implement email receiving with SES?
1. Verify your receiving domain in SES (add MX record pointing to `inbound-smtp.us-east-1.amazonaws.com`)
2. Create a **receipt rule set** with rules:
   - Match: recipient `support@shopwave.com`
   - Actions: S3 (store email) + Lambda (process email) + SNS (notify)
3. Lambda receives the email metadata and the S3 key; reads the email from S3 and processes it (create support ticket, auto-reply, etc.)

Receiving is only available in specific regions (us-east-1, us-west-2, eu-west-1).

---

### Q12. How would you build a double opt-in subscription flow with SES?
1. User enters email → your service stores address with `status=PENDING` in DynamoDB
2. SES sends a confirmation email with a unique token link (token stored in DynamoDB with TTL=24hr)
3. User clicks link → your API validates the token → update `status=CONFIRMED`
4. Only send marketing email to `CONFIRMED` subscribers
5. Every marketing email includes an **unsubscribe link** → click sets `status=UNSUBSCRIBED`

Double opt-in reduces spam complaints significantly — users explicitly confirm they want email.

---

### Q13. What regions is SES available in and does it matter?
SES is available in select regions (us-east-1, us-west-2, eu-west-1, ap-southeast-2, etc.). The sending region determines:
- Your SES quotas (quotas are per region)
- Inbound email receiving (only us-east-1, us-west-2, eu-west-1)
- Dedicated IP pools (region-specific)

For global applications, send from the region closest to your Lambda/application to reduce API latency. For compliance (GDPR), send from eu-west-1 if EU customer data must stay in the EU.

---

### Q14. How do you track email open and click rates with SES?
1. Enable **open tracking** and **click tracking** in the Configuration Set
2. SES automatically wraps links with a tracking URL and inserts a tracking pixel
3. When a user opens the email or clicks a link, SES generates an **Open** or **Click** event
4. Events flow to CloudWatch → metrics dashboard, or Kinesis Firehose → S3/Redshift → analytics

Note: Open tracking requires recipients to load images (blocked by some email clients). Click tracking requires link wrapping (may affect deliverability with some spam filters).

---

### Q15. Design an SES email architecture for an e-commerce platform.
```
Sending:
[Order Events (EventBridge)] → [Lambda: EmailDispatcher]
  ├── Order Confirmed   → ses.send_templated_email (OrderConfirmation template)
  ├── Order Shipped     → ses.send_templated_email (ShipmentNotification template)
  ├── Password Reset    → ses.send_email (inline HTML)
  └── Weekly Newsletter → ses.send_bulk_templated_email (Marketing template)

SES Configuration:
  Domain: shopwave.com (verified, DKIM enabled)
  Configuration Set: shopwave-transactional
    ├── CloudWatch: metrics per event type
    ├── SNS: bounce + complaint → Lambda (suppression handler)
    └── Firehose: all events → S3 → Athena (analytics)
  Dedicated IP: 2 IPs (warmed up) for transactional
  Shared IP pool: for marketing (separate reputation)
  Suppression List: account-level (auto-suppress bounces/complaints)

Bounce/Complaint Handler Lambda:
  → Parse SNS notification
  → DynamoDB: mark address as suppressed
  → Never send to suppressed addresses

Receiving:
  support@shopwave.com → SES Receipt Rule
    → S3: raw email storage
    → Lambda: parse email → create Zendesk ticket via API
```

---

## 3. Real-World Use Case: Transactional Email for E-Commerce

### Scenario
ShopWave sends 50,000 order confirmation emails and 5,000 password reset emails daily. They need reliable delivery, < 2% bounce rate, and automated handling of bounces/complaints.

### Architecture

```
[EventBridge: Order Placed]
         │
[Lambda: email-dispatcher]
         │ ses:SendTemplatedEmail
         ▼
[SES: shopwave.com domain]
  DKIM: Auto-configured by SES
  SPF:  v=spf1 include:amazonses.com ~all
  DMARC: v=DMARC1; p=quarantine; rua=mailto:dmarc@shopwave.com
  Configuration Set: shopwave-transactional
         │
         ├── [Delivery] → CloudWatch metric (DeliveredEmails)
         ├── [Bounce]   → SNS → Lambda (remove from list)
         ├── [Complaint]→ SNS → Lambda (unsubscribe)
         └── [Open/Click]→ Firehose → S3 → Athena

[Suppression List]
  → Auto-suppresses hard bounces + complaints
  → DynamoDB: app-level suppression (check before sending)

[SES Receiving: returns@shopwave.com]
  → S3: store bounce emails
  → Parsed by Lambda for manual review
```

### Key Design Decisions
| Decision | Why |
|----------|-----|
| DKIM + SPF + DMARC | Maximum deliverability; required for Gmail/Yahoo (2024 requirement for > 5k/day) |
| Account-level suppression list | Automatic; no custom code needed for SES-level bounce management |
| Lambda suppression handler | App-level list prevents sending even before SES rejects |
| Configuration Set on all sends | Full observability on every email; required for bounce tracking |
| Dedicated IPs for transactional | Insulate transactional reputation from marketing campaigns |
| Separate sending for marketing | Different IP pool + configuration set; marketing complaints don't affect transactional IPs |

### Interview Narration (Whiteboard Script)
> "SES is our email backbone. Domain is verified with DKIM — SES auto-configures the DNS records. We add SPF and DMARC manually in Route 53, which is now required by Gmail and Yahoo for bulk senders. Every send goes through a Configuration Set that streams bounce and complaint events to an SNS topic; a Lambda subscribes and marks those addresses as suppressed in DynamoDB. We never send to suppressed addresses — this keeps our bounce rate well below 1% and our complaint rate near zero. Transactional email goes through dedicated IPs that are completely isolated from our marketing campaigns so a marketing spike in complaints can't hurt order confirmation delivery."
