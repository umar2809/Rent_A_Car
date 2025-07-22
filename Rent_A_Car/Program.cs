using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => 
    opt.UseInMemoryDatabase("RentalDb"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "RentalAPI";
    config.Title = "Car Rental API v1";
    config.Version = "v1";
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "Car Rental API";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

// Car endpoints
app.MapGet("/cars", async (AppDbContext db) => 
    await db.Cars.ToListAsync());

app.MapGet("/cars/available", async (AppDbContext db) => 
    await db.Cars.Where(c => c.IsAvailable).ToListAsync());

app.MapGet("/cars/{id}", async (int id, AppDbContext db) => 
    await db.Cars.FindAsync(id) is Car car 
        ? Results.Ok(car) 
        : Results.NotFound());

app.MapPost("/cars", async (Car car, AppDbContext db) =>
{
    db.Cars.Add(car);
    await db.SaveChangesAsync();
    return Results.Created($"/cars/{car.Id}", car);
});

// Add PUT endpoint for updating a car
app.MapPut("/cars/{id}", async (int id, Car updatedCar, AppDbContext db) =>
{
    var car = await db.Cars.FindAsync(id);
    if (car is null) return Results.NotFound();
    car.Make = updatedCar.Make;
    car.Model = updatedCar.Model;
    car.Year = updatedCar.Year;
    car.LicensePlate = updatedCar.LicensePlate;
    car.DailyRate = updatedCar.DailyRate;
    car.IsAvailable = updatedCar.IsAvailable;
    await db.SaveChangesAsync();
    return Results.Ok(car);
});

// Add DELETE endpoint for deleting a car
app.MapDelete("/cars/{id}", async (int id, AppDbContext db) =>
{
    var car = await db.Cars.FindAsync(id);
    if (car is null) return Results.NotFound();
    db.Cars.Remove(car);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// Customer endpoints
app.MapGet("/customers", async (AppDbContext db) => 
    await db.Customers.ToListAsync());

app.MapGet("/customers/{id}", async (int id, AppDbContext db) => 
    await db.Customers.FindAsync(id) is Customer customer 
        ? Results.Ok(customer) 
        : Results.NotFound());

app.MapPost("/customers", async (Customer customer, AppDbContext db) =>
{
    db.Customers.Add(customer);
    await db.SaveChangesAsync();
    return Results.Created($"/customers/{customer.Id}", customer);
});

// Add PUT endpoint for updating a customer
app.MapPut("/customers/{id}", async (int id, Customer updatedCustomer, AppDbContext db) =>
{
    var customer = await db.Customers.FindAsync(id);
    if (customer is null) return Results.NotFound();
    customer.FirstName = updatedCustomer.FirstName;
    customer.LastName = updatedCustomer.LastName;
    customer.Email = updatedCustomer.Email;
    customer.Phone = updatedCustomer.Phone;
    customer.IdCardNumber = updatedCustomer.IdCardNumber;
    await db.SaveChangesAsync();
    return Results.Ok(customer);
});

// Add DELETE endpoint for deleting a customer
app.MapDelete("/customers/{id}", async (int id, AppDbContext db) =>
{
    var customer = await db.Customers.FindAsync(id);
    if (customer is null) return Results.NotFound();
    db.Customers.Remove(customer);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// Booking endpoints
app.MapGet("/bookings", async (AppDbContext db) => 
    await db.Bookings
        .Include(b => b.Car)
        .Include(b => b.Customer)
        .ToListAsync());

app.MapPost("/bookings", async (Booking booking, AppDbContext db) =>
{
    // Validate car availability
    var car = await db.Cars.FindAsync(booking.CarId);
    if (car is null || !car.IsAvailable)
        return Results.BadRequest("Car not available for booking");

    // Calculate booking duration and cost
    var bookingDays = (int)(booking.EndDate - booking.StartDate).TotalDays;
    if (bookingDays < 1) 
        return Results.BadRequest("Invalid booking duration");
    
    booking.TotalCost = car.DailyRate * bookingDays;

    // Update car status
    car.IsAvailable = false;
    
    // Create booking
    db.Bookings.Add(booking);
    await db.SaveChangesAsync();
    
    return Results.Created($"/bookings/{booking.Id}", booking);
});

app.MapPut("/bookings/complete/{id}", async (int id, AppDbContext db) =>
{
    var booking = await db.Bookings.FindAsync(id);
    if (booking is null) return Results.NotFound();

    booking.IsCompleted = true;
    
    // Release car
    var car = await db.Cars.FindAsync(booking.CarId);
    if (car != null) car.IsAvailable = true;
    
    await db.SaveChangesAsync();
    return Results.Ok("Booking completed successfully");
});

// Add PUT endpoint for updating a booking
app.MapPut("/bookings/{id}", async (int id, Booking updatedBooking, AppDbContext db) =>
{
    var booking = await db.Bookings.FindAsync(id);
    if (booking is null) return Results.NotFound();

    // If car is changed, release old car and reserve new one if available
    if (booking.CarId != updatedBooking.CarId)
    {
        var oldCar = await db.Cars.FindAsync(booking.CarId);
        if (oldCar != null) oldCar.IsAvailable = true;
        var newCar = await db.Cars.FindAsync(updatedBooking.CarId);
        if (newCar == null || !newCar.IsAvailable)
            return Results.BadRequest("Selected car is not available");
        newCar.IsAvailable = false;
        booking.CarId = updatedBooking.CarId;
    }

    // Update other fields
    booking.CustomerId = updatedBooking.CustomerId;
    booking.StartDate = updatedBooking.StartDate;
    booking.EndDate = updatedBooking.EndDate;
    booking.IsCompleted = updatedBooking.IsCompleted;

    // Recalculate cost
    var carForRate = await db.Cars.FindAsync(booking.CarId);
    var bookingDays = (int)(booking.EndDate - booking.StartDate).TotalDays;
    if (bookingDays < 1) return Results.BadRequest("Invalid booking duration");
    booking.TotalCost = carForRate.DailyRate * bookingDays;

    await db.SaveChangesAsync();
    return Results.Ok(booking);
});

// Add DELETE endpoint for deleting a booking
app.MapDelete("/bookings/{id}", async (int id, AppDbContext db) =>
{
    var booking = await db.Bookings.FindAsync(id);
    if (booking is null) return Results.NotFound();

    // Release car if booking not completed
    if (!booking.IsCompleted)
    {
        var car = await db.Cars.FindAsync(booking.CarId);
        if (car != null) car.IsAvailable = true;
    }

    db.Bookings.Remove(booking);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();