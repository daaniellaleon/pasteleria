const pool = require("../db/db");
const bcrypt = require("bcrypt");
const { generarToken } = require("../auth/auth");

async function login(req, res) {
  console.log("Body recibido:", req.body);

  const { username, password } = req.body;
  if (!username || !password) 
    return res.status(400).json({ mensaje: "Faltan credenciales" });

  try {
    // Consulta al usuario en la BD
    const [rows] = await pool.query(
      "SELECT * FROM usuarios WHERE username = ?",
      [username]
    );

    if (rows.length === 0) 
      return res.status(401).json({ mensaje: "Usuario no encontrado" });

    const usuario = rows[0];

    // Comparación con bcrypt usando la contraseña hasheada de la BD
    const match = await bcrypt.compare(password, usuario.password);

    if (!match) 
      return res.status(401).json({ mensaje: "Contraseña incorrecta" });

    // Generar token JWT
    const token = generarToken(usuario);

    res.json({ token });
  } catch (err) {
    console.error(err); // log completo en consola
    res.status(500).json({ mensaje: "Error en el servidor", error: err.message });
  }
}

module.exports = { login };
