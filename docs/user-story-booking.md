
---

# 📄 2. docs/user-story-booking.md

```md id="userstory001"
# User Story - Booking Flow (Primary Scenario)

## Title
Customer books a service using AlicIA virtual assistant

---

## Actors

- Customer (e.g. Maria Silva)
- Service provider (e.g. Amanda)
- AlicIA (virtual assistant)

---

## Scenario

Maria wants to book a beauty service with Amanda.

---

## Step-by-step Journey

### 1. Initial contact

Maria sends a message via WhatsApp:

"Oi, quero agendar cílios"

---

### 2. Automatic response

Amanda is busy. AlicIA responds automatically:

"Oi Maria, tudo bem? 💖  
No momento estou ocupada, mas você pode agendar direto comigo 👇"

Link:
https://chat.alicia.ayax.com.br?phone=21999999999

---

### 3. AlicIA starts interaction

AlicIA greets the user:

"Oi Maria! 😊  
Sou a AlicIA, secretária da Amanda.  
Em qual serviço você deseja agendar?"

---

### 4. Service selection

AlicIA presents available services:

- Design de sobrancelhas
- Alongamento de cílios
- Manutenção

Maria selects:
→ Alongamento de cílios

---

### 5. Availability lookup

AlicIA retrieves real availability:

- Business hours
- Existing bookings
- Google Calendar

AlicIA presents:

"Sábado 10:00  
Sábado 11:00  
Segunda 14:00"

---

### 6. Slot selection

Maria chooses:
→ Sábado 10:00

---

### 7. Confirmation

AlicIA asks:

"Confirmar agendamento?"

Maria confirms.

---

### 8. Booking execution (system)

AlicIA:

- Validates availability
- Creates or retrieves customer
- Creates request
- Creates Google Calendar event
- Stores ExternalEventId

---

### 9. Confirmation response

AlicIA responds:

"Agendamento confirmado 🎉  
Sábado às 10:00  
Te espero lá 💖"

---

## Alternative Scenario

### New customer

If the phone number is not found:

- AlicIA creates a new customer automatically

---

## Business Value

- Reduces manual scheduling effort
- Prevents double booking
- Enables 24/7 scheduling
- Increases conversion rate
- Provides real-time availability

---

## Key Insight

The user does not see:

- APIs
- Database
- Google integration

The user only sees:

→ fast, simple, reliable booking

## Execution Mapping

- [ ] User selects service
- [ ] User selects slot
- [ ] System validates availability
- [ ] System creates/reuses customer
- [ ] System creates request
- [ ] System creates Google event
- [ ] System confirms booking