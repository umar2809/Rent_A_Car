# Rent_A_Car

A full-stack Car Rental Management System designed for rental agencies to efficiently handle car inventory, customer data, and booking operations.

## Features

- **Car Management**: Add, edit, view, and delete cars, including details like make, model, year, license plate, and daily rate.
- **Customer Management**: Manage customer records with personal details, contact info, and ID card numbers.
- **Bookings**: Create, view, and complete car bookings for customers, specifying rental period and calculating total cost.
- **Dashboard**: Track total cars, available cars, total customers, active bookings, and total revenue.
- **RESTful API**: Backend exposes endpoints for cars, customers, and bookings.
- **Frontend**: Simple HTML/JS interface for dashboard, cars, customers, and bookings management.
- **Swagger/OpenAPI**: Interactive API documentation available in development mode.
- **CORS Enabled**: Frontend and backend communicate via HTTP API.

## Tech Stack

- **Backend**: ASP.NET Core Minimal API, Entity Framework Core (InMemory DB)
- **Frontend**: HTML, CSS, JavaScript
- **API Documentation**: NSwag/Swagger
- **Database**: InMemory (for demo/testing; replace with SQL for production)

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Node.js (if you want to expand the frontend or use npm packages)
- Modern web browser

### Running the Backend

1. Clone the repository:
    ```sh
    git clone https://github.com/umar2809/Rent_A_Car.git
    cd Rent_A_Car
    ```

2. Run the API:
    ```sh
    dotnet run --project Rent_A_Car
    ```
    The API will start (by default on `http://localhost:5050`).

### Running the Frontend

1. Open `Rent_A_Car/frontend/index.html` in your browser.
2. The frontend communicates with the backend API at `http://localhost:5050`.

> **Note:** Make sure backend is running and accessible to the frontend. Adjust `API_BASE` in `frontend/api.js` if you change backend port.

## API Endpoints

- `GET /cars` - List all cars
- `POST /cars` - Add a new car
- `PUT /cars/{id}` - Update car info
- `DELETE /cars/{id}` - Remove a car

- `GET /customers` - List all customers
- `POST /customers` - Add a new customer
- `PUT /customers/{id}` - Update customer info
- `DELETE /customers/{id}` - Remove a customer

- `GET /bookings` - List all bookings
- `POST /bookings` - Create a booking
- `PUT /bookings/complete/{id}` - Complete a booking

## Project Structure

```
Rent_A_Car/
  ├── Program.cs        # Backend API entrypoint
  ├── AppDbContext.cs   # Entity Framework Core DB context
  ├── Models/           # Data models (Car, Customer, Booking)
  ├── frontend/
      ├── api.js        # Frontend JS API client
      ├── index.html    # Dashboard
      ├── cars.html     # Car management
      ├── customers.html# Customer management
      ├── bookings.html # Booking management
      └── style.css     # UI styles
```

## Customization

- Replace InMemory DB with a persistent database for production.
- Add authentication/authorization for secure access.
- Enhance frontend with frameworks like React or Angular.

## License

This project is for educational/demo purposes. Please contact the repository owner for commercial use or further customization.

## Author

Developed by [umar2809](https://github.com/umar2809)
