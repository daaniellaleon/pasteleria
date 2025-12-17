const pool = require("../db/db");

async function listarCategorias(req, res) {
  try {
    const [rows] = await pool.query("SELECT * FROM categorias");
    res.json(rows);
  } catch (err) {
    res.status(500).json({ mensaje: "Error al listar categorías", error: err });
  }
}


// EXPORTA las funciones correctamente
module.exports = { listarCategorias };
