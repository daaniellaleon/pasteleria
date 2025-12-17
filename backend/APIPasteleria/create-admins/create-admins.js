// create-admins.js
const bcrypt = require('bcrypt');
const pool = require('../db/db');

async function createAdmins() {
  const admins = [
    { username: 'diegoventas', password: 'admin12345678', rol: 'ventas' },
    { username: 'laurainventario', password: 'admin12345678', rol: 'inventario' }
  ];

  for (const a of admins) {
    const hashed = await bcrypt.hash(a.password, 10);
    try {
      await pool.execute('INSERT INTO usuarios (username, password, rol) VALUES (?, ?, ?)',
        [a.username, hashed, a.rol]);
      console.log('Creado', a.username);
    } catch (err) {
      if (err.code === 'ER_DUP_ENTRY') console.log('Ya existe', a.username);
      else console.error(err);
    }
  }
  process.exit(0);
}

createAdmins();
