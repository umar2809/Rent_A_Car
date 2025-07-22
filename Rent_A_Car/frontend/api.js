const API_BASE = "http://localhost:5050"; // Changed to match backend port

const api = {
  async getCars() {
    const res = await fetch(`${API_BASE}/cars`);
    return res.json();
  },
  async getCar(id) {
    const res = await fetch(`${API_BASE}/cars/${id}`);
    return res.json();
  },
  async createCar(car) {
    const res = await fetch(`${API_BASE}/cars`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(car)
    });
    return res.json();
  },
  async updateCar(id, car) {
    const res = await fetch(`${API_BASE}/cars/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(car)
    });
    return res.json();
  },
  async deleteCar(id) {
    const res = await fetch(`${API_BASE}/cars/${id}`, {
      method: 'DELETE'
    });
    return res.ok;
  },
  async getCustomers() {
    const res = await fetch(`${API_BASE}/customers`);
    return res.json();
  },
  async getCustomer(id) {
    const res = await fetch(`${API_BASE}/customers/${id}`);
    return res.json();
  },
  async createCustomer(customer) {
    const res = await fetch(`${API_BASE}/customers`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(customer)
    });
    return res.json();
  },
  async updateCustomer(id, customer) {
    const res = await fetch(`${API_BASE}/customers/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(customer)
    });
    return res.json();
  },
  async deleteCustomer(id) {
    const res = await fetch(`${API_BASE}/customers/${id}`, {
      method: 'DELETE'
    });
    return res.ok;
  },
  async getBookings() {
    const res = await fetch(`${API_BASE}/bookings`);
    return res.json();
  },
  async createBooking(booking) {
    const res = await fetch(`${API_BASE}/bookings`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(booking)
    });
    return res.json();
  },
  async completeBooking(id) {
    const res = await fetch(`${API_BASE}/bookings/complete/${id}`, {
      method: 'PUT'
    });
    return res.json();
  }
};
