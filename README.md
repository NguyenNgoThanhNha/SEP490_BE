# 🎓 SEP490_BE – Backend for Ideas Management SPA Booking System

**Backend repository** for the graduation thesis at **FPT University** – A system for managing bookings and ideas for spa services, supporting both **web** and **mobile apps**.

---

## 📌 Project Overview

- **University**: FPT University  
- **Course**: Graduation Thesis – Software Engineering  
- **Platform**: Web SPA + Mobile  
- **Goal**: To manage services, customers, staff, and booking schedules within the spa system.

---

## 💡 Real-World Problems

### 😕 Customer Challenges
- Cannot choose their preferred specialist
- Lack of clear information about services/products
- Complicated and non-transparent booking process

### 🧑‍💼 Spa Management Challenges
- Booking via multiple channels (Zalo, Facebook, phone calls) causing confusion
- Inconsistent staff working schedules
- Lack of CRM for customer management

---

## 🚀 Proposed Solution

- Auto-booking with specialist work schedules
- Check for conflicting bookings, suggest alternative specialists
- Create a chat channel between customers and specialists (using MongoDB)
- Send real-time notifications using **SignalR**
- Integrate ratings, feedback, and behavior analysis

---

## 🛠️ Technologies Used

- **Backend Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core
- **Database**: MySQL (relational), MongoDB (chat storage)
- **Real-time Notification**: SignalR
- **Authentication**: JWT
- **Cache**: Redis
- **Mapping**: AutoMapper
- **Containerization**: Docker, Docker Compose
- **Search**: ElasticSearch


---

## 🧪 How to Run the Project

```bash
# 1. Clone the repository
git clone https://github.com/your-username/SEP490_BE.git
cd SEP490_BE

# 2. Set up environment variables
# Create an .env file or set system environment variables for:
# - DB_CONNECTION
# - MONGODB_URI
# - REDIS_CONNECTION
# - JWT_SECRET
# - MAIL_CONFIG,...
# - ZALO_PAY or PAYOS info

# 3. Run using Docker Compose
docker-compose up --build
