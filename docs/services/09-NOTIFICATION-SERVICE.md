# Notification Service (.NET 10)

## Overview
Multi-channel notification service built with .NET 10. Handles email, SMS, push notifications, and in-app notifications. Consumes events from other services and delivers notifications through various channels.

## Technology Stack
- **Language**: .NET 10 (C#)
- **Framework**: ASP.NET Core Web API
- **Database**: PostgreSQL (Code-First EF Core)
- **Cache**: Redis
- **Messaging**: Kafka (Consumer)
- **Email**: SMTP (MailKit OSS)
- **SMS**: Twilio SDK (or similar OSS)
- **Push**: Firebase Cloud Messaging (FCM)
- **Template Engine**: Razor / Liquid
- **Queue**: Background job processing (Hangfire OSS)

## Core Responsibilities

### 1. **Multi-Channel Notification Delivery**
- Email notifications
- SMS notifications
- Push notifications (mobile, web)
- In-app notifications
- Webhook notifications

### 2. **Event-Driven Notifications**
- Listen to business events (order, payment, shipping)
- Process notification triggers
- Template selection based on event type
- Multi-language support

### 3. **Template Management**
- Email templates (HTML/Text)
- SMS templates
- Push notification templates
- Template versioning
- Multi-language templates

### 4. **User Preferences**
- Notification preferences per user
- Channel preferences (email, SMS, push)
- Opt-in/opt-out management
- Quiet hours/DND settings
- Frequency capping

### 5. **Delivery Tracking**
- Delivery status tracking
- Read receipts
- Click tracking (emails)
- Delivery analytics
- Failed delivery retry

### 6. **Priority & Scheduling**
- Priority-based delivery
- Scheduled notifications
- Batch notifications
- Rate limiting per channel

### 7. **Notification History**
- Sent notification history
- Delivery logs
- User notification inbox (in-app)
- Search and filtering

### 8. **Integration Management**
- SMTP provider configuration
- SMS gateway configuration
- Push notification credentials
- Failover between providers

## Clean Architecture Structure

```
NotificationService/
├── src/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── Notification.cs
│   │   │   ├── NotificationTemplate.cs
│   │   │   ├── NotificationPreference.cs
│   │   │   └── DeliveryLog.cs
│   │   ├── Enums/
│   │   │   ├── NotificationChannel.cs
│   │   │   ├── NotificationStatus.cs
│   │   │   ├── NotificationPriority.cs
│   │   │   └── EventType.cs
│   │   ├── Events/
│   │   │   └── NotificationSentEvent.cs
│   │   └── Repositories/
│   │       ├── INotificationRepository.cs
│   │       └── ITemplateRepository.cs
│   │
│   ├── Application/
│   │   ├── DTOs/
│   │   │   ├── NotificationRequest.cs
│   │   │   ├── NotificationResponse.cs
│   │   │   └── PreferenceRequest.cs
│   │   ├── UseCases/
│   │   │   ├── SendNotificationUseCase.cs
│   │   │   ├── SendBatchNotificationUseCase.cs
│   │   │   ├── GetNotificationHistoryUseCase.cs
│   │   │   └── UpdatePreferenceUseCase.cs
│   │   ├── Interfaces/
│   │   │   ├── INotificationService.cs
│   │   │   ├── IEmailService.cs
│   │   │   ├── ISmsService.cs
│   │   │   └── IPushService.cs
│   │   └── TemplateEngine/
│   │       └── ITemplateRenderer.cs
│   │
│   ├── Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── NotificationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── NotificationConfiguration.cs
│   │   │   │   └── TemplateConfiguration.cs
│   │   │   └── Repositories/
│   │   │       └── NotificationRepository.cs
│   │   ├── Messaging/
│   │   │   ├── KafkaConsumer.cs
│   │   │   └── EventHandlers/
│   │   │       ├── OrderEventHandler.cs
│   │   │       ├── PaymentEventHandler.cs
│   │   │       └── ShippingEventHandler.cs
│   │   ├── Channels/
│   │   │   ├── Email/
│   │   │   │   ├── SmtpEmailService.cs
│   │   │   │   └── EmailTemplateRenderer.cs
│   │   │   ├── Sms/
│   │   │   │   └── TwilioSmsService.cs
│   │   │   ├── Push/
│   │   │   │   └── FcmPushService.cs
│   │   │   └── InApp/
│   │   │       └── InAppNotificationService.cs
│   │   ├── Queue/
│   │   │   └── BackgroundJobService.cs
│   │   └── Cache/
│   │       └── RedisCache.cs
│   │
│   └── Api/
│       ├── Controllers/
│       │   ├── NotificationsController.cs
│       │   ├── PreferencesController.cs
│       │   ├── TemplatesController.cs
│       │   └── HealthController.cs
│       ├── BackgroundJobs/
│       │   ├── NotificationProcessorJob.cs
│       │   └── RetryFailedNotificationsJob.cs
│       ├── Program.cs
│       └── appsettings.json
│
├── templates/
│   ├── email/
│   │   ├── order-confirmation.html
│   │   ├── payment-success.html
│   │   └── shipping-update.html
│   └── sms/
│       └── order-confirmation.txt
│
├── Migrations/
└── NotificationService.csproj
```

## Domain Models (Code-First)

### Notification Entity
```csharp
public class Notification : BaseEntity
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }

    public NotificationChannel Channel { get; set; }
    public NotificationPriority Priority { get; set; }
    public NotificationStatus Status { get; set; }

    public string EventType { get; set; }  // order.created, payment.success
    public string EventId { get; set; }

    public string TemplateCode { get; set; }
    public string Language { get; set; }

    public string Subject { get; set; }
    public string Content { get; set; }

    // Channel-specific
    public string RecipientEmail { get; set; }
    public string RecipientPhone { get; set; }
    public string DeviceToken { get; set; }

    // Tracking
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? FailedAt { get; set; }

    public string FailureReason { get; set; }
    public int RetryCount { get; set; }

    // Navigation
    public ICollection<DeliveryLog> DeliveryLogs { get; set; }
}

public enum NotificationChannel
{
    Email,
    Sms,
    Push,
    InApp,
    Webhook
}

public enum NotificationStatus
{
    Pending,
    Queued,
    Sending,
    Sent,
    Delivered,
    Read,
    Failed,
    Cancelled
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}
```

### NotificationTemplate Entity
```csharp
public class NotificationTemplate : BaseEntity
{
    public Guid TemplateId { get; set; }
    public string Code { get; set; }  // order_confirmation
    public string Name { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Language { get; set; }

    public string Subject { get; set; }  // For email
    public string BodyTemplate { get; set; }  // HTML or text with placeholders
    public string TemplateEngine { get; set; }  // Razor, Liquid

    public bool IsActive { get; set; }
    public int Version { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### NotificationPreference Entity
```csharp
public class NotificationPreference : BaseEntity
{
    public Guid PreferenceId { get; set; }
    public Guid UserId { get; set; }

    public string EventType { get; set; }

    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
    public bool PushEnabled { get; set; }
    public bool InAppEnabled { get; set; }

    // Quiet hours
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }

    public DateTime UpdatedAt { get; set; }
}
```

### DeliveryLog Entity
```csharp
public class DeliveryLog : BaseEntity
{
    public Guid LogId { get; set; }
    public Guid NotificationId { get; set; }

    public string Action { get; set; }  // sent, delivered, failed, read
    public string Details { get; set; }
    public string Provider { get; set; }  // SMTP provider, SMS gateway
    public string ExternalId { get; set; }  // Provider's message ID

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Notification Notification { get; set; }
}
```

## API Endpoints

### REST API

#### Notifications
```
POST   /api/v1/notifications              # Send notification (manual trigger)
GET    /api/v1/notifications              # List notifications
GET    /api/v1/notifications/:id          # Get notification details
GET    /api/v1/notifications/inbox        # Get user's inbox (in-app)
PUT    /api/v1/notifications/:id/read     # Mark as read
DELETE /api/v1/notifications/:id          # Delete notification
```

#### Preferences
```
GET    /api/v1/preferences                # Get user preferences
PUT    /api/v1/preferences                # Update preferences
PUT    /api/v1/preferences/:eventType     # Update for specific event
```

#### Templates (Admin)
```
GET    /api/v1/templates                  # List templates
GET    /api/v1/templates/:code            # Get template
POST   /api/v1/templates                  # Create template
PUT    /api/v1/templates/:id              # Update template
POST   /api/v1/templates/:id/test         # Send test notification
```

#### Health
```
GET    /health                            # Health check
GET    /metrics                           # Metrics
```

## Kafka Event Handlers

### Consumed Events
```csharp
// order.created → Send order confirmation
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        await _notificationService.SendAsync(new NotificationRequest
        {
            UserId = @event.UserId,
            EventType = "order.created",
            TemplateCode = "order_confirmation",
            Channel = NotificationChannel.Email,
            Data = new
            {
                OrderNumber = @event.OrderNumber,
                Total = @event.Total,
                Items = @event.Items
            }
        });
    }
}

// payment.successful → Payment confirmation
// order.shipped → Shipping notification
// order.delivered → Delivery confirmation
```

## Email Service Implementation

```csharp
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ITemplateRenderer _templateRenderer;

    public async Task<bool> SendEmailAsync(EmailRequest request)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config["Email:FromName"], _config["Email:FromAddress"]));
        message.To.Add(new MailboxAddress(request.RecipientName, request.RecipientEmail));
        message.Subject = request.Subject;

        var builder = new BodyBuilder
        {
            HtmlBody = request.HtmlBody,
            TextBody = request.TextBody
        };

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_config["Email:Host"], int.Parse(_config["Email:Port"]), SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        return true;
    }
}
```

## Template Rendering

```csharp
public class RazorTemplateRenderer : ITemplateRenderer
{
    public async Task<string> RenderAsync(string templateCode, object data, string language)
    {
        var template = await _templateRepo.GetByCodeAsync(templateCode, language);

        // Use Razor engine or Liquid
        var engine = new RazorEngine();
        var rendered = await engine.RenderAsync(template.BodyTemplate, data);

        return rendered;
    }
}
```

## Background Job Processing

```csharp
[AutomaticRetry(Attempts = 3)]
public class NotificationProcessorJob : IJob
{
    public async Task ExecuteAsync()
    {
        // Fetch pending notifications
        var notifications = await _repo.GetPendingNotificationsAsync(100);

        foreach (var notification in notifications)
        {
            try
            {
                // Check user preferences
                var pref = await _prefRepo.GetUserPreferenceAsync(notification.UserId, notification.EventType);
                if (!IsChannelEnabled(pref, notification.Channel))
                {
                    notification.Status = NotificationStatus.Cancelled;
                    continue;
                }

                // Check quiet hours
                if (IsQuietHours(pref))
                {
                    notification.ScheduledFor = GetNextAllowedTime(pref);
                    continue;
                }

                // Render content
                var content = await _templateRenderer.RenderAsync(
                    notification.TemplateCode,
                    notification.Data,
                    notification.Language
                );

                // Send via appropriate channel
                switch (notification.Channel)
                {
                    case NotificationChannel.Email:
                        await _emailService.SendEmailAsync(...);
                        break;
                    case NotificationChannel.Sms:
                        await _smsService.SendSmsAsync(...);
                        break;
                    case NotificationChannel.Push:
                        await _pushService.SendPushAsync(...);
                        break;
                }

                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;

            }
            catch (Exception ex)
            {
                notification.RetryCount++;
                notification.FailureReason = ex.Message;
                if (notification.RetryCount >= 3)
                {
                    notification.Status = NotificationStatus.Failed;
                }
            }

            await _repo.UpdateAsync(notification);
        }
    }
}
```

## Environment Variables
```
ASPNETCORE_ENVIRONMENT=Production

# Database
DATABASE_HOST=postgres
DATABASE_PORT=5432
DATABASE_NAME=notification_service
DATABASE_USER=notification_service
DATABASE_PASSWORD=***

# Redis
REDIS_HOST=redis
REDIS_PORT=6379

# Kafka
KAFKA_BOOTSTRAP_SERVERS=kafka:9092
KAFKA_GROUP_ID=notification-service
KAFKA_TOPICS=order.events,payment.events,shipping.events

# Email (SMTP)
EMAIL_HOST=smtp.gmail.com
EMAIL_PORT=587
EMAIL_USERNAME=***
EMAIL_PASSWORD=***
EMAIL_FROM_ADDRESS=noreply@example.com
EMAIL_FROM_NAME=E-Commerce Platform

# SMS (Twilio)
TWILIO_ACCOUNT_SID=***
TWILIO_AUTH_TOKEN=***
TWILIO_PHONE_NUMBER=+1234567890

# Push (FCM)
FCM_SERVER_KEY=***
FCM_PROJECT_ID=***

# Background Jobs
HANGFIRE_CONNECTION_STRING=***
```

## Implementation Tasks

### Phase 1: Setup
- [ ] Create .NET 10 Web API project
- [ ] Set up Clean Architecture
- [ ] Configure EF Core and PostgreSQL
- [ ] Set up Hangfire for background jobs

### Phase 2: Domain Layer
- [ ] Define entities
- [ ] Create enums
- [ ] Define repository interfaces

### Phase 3: Application Layer
- [ ] Implement use cases
- [ ] Create DTOs
- [ ] Build template rendering engine

### Phase 4: Infrastructure Layer
- [ ] Implement repositories
- [ ] Set up Kafka consumers
- [ ] Integrate email service (MailKit)
- [ ] Integrate SMS service (Twilio)
- [ ] Integrate push service (FCM)

### Phase 5: API Layer
- [ ] Implement REST controllers
- [ ] Add authentication
- [ ] Create background jobs

### Phase 6: Templates
- [ ] Create email templates
- [ ] Create SMS templates
- [ ] Implement multi-language support

### Phase 7: Testing
- [ ] Unit tests
- [ ] Integration tests
- [ ] Test email delivery
- [ ] Test SMS delivery

## Template Examples

### Order Confirmation Email (Razor)
```html
<!-- templates/email/order-confirmation.html -->
<!DOCTYPE html>
<html>
<head>
    <title>Order Confirmation</title>
</head>
<body>
    <h1>Order Confirmation</h1>
    <p>Thank you for your order!</p>

    <h2>Order #@Model.OrderNumber</h2>

    <table>
        @foreach (var item in Model.Items)
        {
            <tr>
                <td>@item.ProductName</td>
                <td>@item.Quantity</td>
                <td>@item.Price</td>
            </tr>
        }
    </table>

    <p><strong>Total: @Model.Total @Model.Currency</strong></p>
</body>
</html>
```

## Testing Requirements

### Unit Tests
- [ ] Template rendering
- [ ] Preference checking
- [ ] Quiet hours logic

### Integration Tests
- [ ] Email delivery
- [ ] SMS delivery
- [ ] Kafka event handling
- [ ] Background job execution

## Success Criteria
- 99.9% uptime
- Notification delivery < 5 seconds (p95)
- Email delivery rate > 98%
- SMS delivery rate > 99%
- Support 1M+ notifications/day
- Zero data loss
